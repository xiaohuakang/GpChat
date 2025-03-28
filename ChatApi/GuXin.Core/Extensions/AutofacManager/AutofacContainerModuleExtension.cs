﻿using Autofac;
using Autofac.Extensions.DependencyInjection;
using Dapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;
using GuXin.Core.CacheManager;
using GuXin.Core.Configuration;
using GuXin.Core.Const;
using GuXin.Core.Dapper;
using GuXin.Core.DBManager;
using GuXin.Core.EFDbContext;
using GuXin.Core.Enums;
using GuXin.Core.Extensions.AutofacManager;
using GuXin.Core.ManageUser;
using GuXin.Core.ObjectActionValidator;
using GuXin.Core.Services;

namespace GuXin.Core.Extensions
{
    public static class AutofacContainerModuleExtension
    {
        //  private static bool _isMysql = false;
        public static IServiceCollection AddModule(this IServiceCollection services, ContainerBuilder builder, IConfiguration configuration)
        {
            //services.AddSession();
            //services.AddMemoryCache();
            //初始化配置文件
            AppSetting.Init(services, configuration);
            Type baseType = typeof(IDependency);
            var compilationLibrary = DependencyContext.Default
                .CompileLibraries
                .Where(x => !x.Serviceable
                && x.Type == "project")
                .ToList();
            var count1 = compilationLibrary.Count;
            List<Assembly> assemblyList = new List<Assembly>();

            foreach (var _compilation in compilationLibrary)
            {
                try
                {
                    assemblyList.Add(AssemblyLoadContext.Default.LoadFromAssemblyName(new AssemblyName(_compilation.Name)));
                }
                catch (Exception ex)
                {
                    Console.WriteLine(_compilation.Name + ex.Message);
                }
            }
            builder.RegisterAssemblyTypes(assemblyList.ToArray())
             .Where(type => baseType.IsAssignableFrom(type) && !type.IsAbstract)
             .AsSelf().AsImplementedInterfaces()
             .InstancePerLifetimeScope();
            builder.RegisterType<UserContext>().InstancePerLifetimeScope();
            builder.RegisterType<ActionObserver>().InstancePerLifetimeScope();
            //model校验结果
            builder.RegisterType<ObjectModelValidatorState>().InstancePerLifetimeScope();
            string connectionString = DBServerProvider.GetConnectionString(null);

            if (DBType.Name == DbCurrentType.MySql.ToString())
            {
                //2020.03.31增加dapper对mysql字段Guid映射
                SqlMapper.AddTypeHandler(new DapperParseGuidTypeHandler());
                SqlMapper.RemoveTypeMap(typeof(Guid?));

                //services.AddDbContext<GuXinContext>();
                //mysql8.x的版本使用Pomelo.EntityFrameworkCore.MySql 3.1会产生异常，需要在字符串连接上添加allowPublicKeyRetrieval=true
                services.AddDbContextPool<GuXinContext>(optionsBuilder => { optionsBuilder.UseMySql(connectionString); }, 64);
            }
            else if (DBType.Name == DbCurrentType.PgSql.ToString())
            {
                services.AddDbContextPool<GuXinContext>(optionsBuilder => { optionsBuilder.UseNpgsql(connectionString); }, 64);
            }
            {
                services.AddDbContextPool<GuXinContext>(optionsBuilder => { optionsBuilder.UseSqlServer(connectionString); }, 64);
            }
            //启用缓存
            if (AppSetting.UseRedis)
            {
                builder.RegisterType<RedisCacheService>().As<ICacheService>().SingleInstance();
            }
            else
            {
                builder.RegisterType<MemoryCacheService>().As<ICacheService>().SingleInstance();
            }
            return services;
        }

    }
}
