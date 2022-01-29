using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Wechat.Protocol;

namespace Wechat.Api.Response.Login
{
    public class InitUserResponse
    {
        /// <summary>
        /// 消息
        /// </summary>
        public InitResponse InitResponse { get; set; }

        /// <summary>
        /// buffer
        /// </summary>
        public string Buffer { get; set; }

        /// <summary>
        /// 同步Key
        /// </summary>
        public int SyncKey { get; set; }
    }
}