﻿using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;
using GuXin.Core.Controllers.Basic;
using GuXin.Core.Extensions;
using GuXin.Core.Filters;
using GuXin.System.IServices;

namespace GuXin.System.Controllers
{
    public partial class Sys_DictionaryController
    {
        [HttpPost, Route("GetVueDictionary")]
        [ApiActionPermission()]
        public async Task<IActionResult> GetVueDictionary([FromBody]string[] dicNos)
        {
            return Content((await Service.GetVueDictionary(dicNos)).Serialize());
        }
        /// <summary>
        /// table加载数据后刷新当前table数据的字典项(适用字典数据量比较大的情况)
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        [HttpPost, Route("getTableDictionary")]
        public IActionResult GetTableDictionary([FromBody] Dictionary<string, object[]> keyData)
        {
            return Json(Service.GetTableDictionary(keyData));
        }
        /// <summary>
        /// 远程搜索
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        [HttpPost, Route("getSearchDictionary")]
        public async Task<IActionResult> GetSearchDictionary(string dicNo, string value)
        {
            return Json(await Service.GetSearchDictionary(dicNo, value));
        }

        /// <summary>
        /// 表单设置为远程查询，重置或第一次添加表单时，获取字典的key、value
        /// </summary>
        /// <param name="dicNo"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        [HttpPost, Route("getRemoteDefaultKeyValue")]
        public async Task<IActionResult> GetRemoteDefaultKeyValue(string dicNo, string key)
        {
            return Json(await Service.GetRemoteDefaultKeyValue(dicNo, key));
        }
        /// <summary>
        /// 代码生成器获取所有字典项(超级管理权限)
        /// </summary>
        /// <returns></returns>
        [HttpPost, Route("GetBuilderDictionary")]
        [ApiActionPermission(ActionRolePermission.SuperAdmin)]
        public async Task<IActionResult> GetBuilderDictionary()
        {
            return Json(await Service.GetBuilderDictionary());
        }

    }
}
