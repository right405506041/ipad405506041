using micromsg;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
 

namespace Wechat.Api.Response.Pay
{
    public class CorpseFansResponse
    {
        /// <summary>
        /// 清理信息
        /// </summary>
        public string Info { get; set; }

        /// <summary>
        /// 僵尸粉
        /// </summary>
        public List<string> CorpseFans { get; set; }

        /// <summary>
        /// 被锁粉
        /// </summary>
        public List<string> BlockFans { get; set; }





    }
    public class CorpseFansDetailResponse
    {
        /// <summary>
        /// 清理信息
        /// </summary>
        public string Info { get; set; }

        /// <summary>
        /// 僵尸粉
        /// </summary>
        public List<ModContact> CorpseFans { get; set; }

        /// <summary>
        /// 被锁粉
        /// </summary>
        public List<ModContact> BlockFans { get; set; }





    }
}