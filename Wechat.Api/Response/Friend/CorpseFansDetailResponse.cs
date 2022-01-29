using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Wechat.Api.Response.Friend
{
    public class CorpseFansDetailResponse
    {
        /// <summary>
        /// 清理信息
        /// </summary>
        public string Info { get; set; }

        /// <summary>
        /// 僵尸粉
        /// </summary>
        public List<micromsg.ModContact> CorpseFans { get; set; }

        /// <summary>
        /// 被锁粉
        /// </summary>
        public List<micromsg.ModContact> BlockFans { get; set; }
    }
}