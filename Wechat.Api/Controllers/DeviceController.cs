using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using Wechat.Api.Abstracts;
using Wechat.Api.Extensions;
using Wechat.Api.Request.Common;
using Wechat.Api.Request.Device;
using Wechat.Api.Request.Favor;
using Wechat.Api.Response.Device;
using Wechat.Protocol;
using Wechat.Util.Exceptions;

namespace Wechat.Api.Controllers
{
    /// <summary>
    /// 设备管理
    /// </summary>
    public class DeviceController : WebchatControllerBase
    {

        /// <summary>
        /// 获取安全设备
        /// </summary>
        /// <param name="wxId"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("api/Device/GetSafeDevice/{wxId}")]
        public async Task<HttpResponseMessage> GetSafeDevice(string wxId)
        {
            ResponseBase<micromsg.GetSafetyInfoResponse> response = new ResponseBase<micromsg.GetSafetyInfoResponse>();
            var result = wechat.GetSafeDeviceNew(wxId);

            if (result != null && result.BaseResponse.Ret == (int)MMPro.MM.RetConst.MM_OK)
            {

                response.Data = result;
            }
            else
            {
                response.Success = false;
                response.Code = "402";
                response.Message = "获取失败";
            }
            return await response.ToHttpResponseAsync();

        }


        /// <summary>
        /// 删除安全设备
        /// </summary>
        /// <param name="delSafeDevice"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("api/Device/DelSafeDevice")]
        public async Task<HttpResponseMessage> DelSafeDevice(DelSafeDevice delSafeDevice)
        {
            ResponseBase<micromsg.DelSafeDeviceResponse> response = new ResponseBase<micromsg.DelSafeDeviceResponse>();
            var result = wechat.DelSafeDevice(delSafeDevice.WxId, delSafeDevice.Uuid);
            response.Data = result;
            return await response.ToHttpResponseAsync();
        }


    }
}