using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using micromsg;

namespace Wechat.Api.Response.Device
{
    public class DeviceListResponse
    {
        public List<LoginDevice> list { get; set; }
    }
}