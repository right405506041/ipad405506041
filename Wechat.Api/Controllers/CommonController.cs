using org.apache.rocketmq.common.message;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using Wechat.Api.Abstracts;
using Wechat.Api.Extensions;
using Wechat.Api.Helper;
using Wechat.Api.Request.Common;
using Wechat.Api.Request.Login;
using Wechat.Api.Response.Common;
using Wechat.Protocol;
using Wechat.Util;
using Wechat.Util.Cache;
using Wechat.Util.Exceptions;
using Wechat.Util.Extensions;
using Wechat.Util.FileStore;
using Wechat.Util.Mq;
using Wechat.Util.QrCode;

namespace Wechat.Api.Controllers
{
    /// <summary>
    /// 公共
    /// </summary>
    public class CommonController : WebchatControllerBase
    {
        /// <summary>
        /// 摇一摇
        /// </summary>
        /// <param name="sharkItOff"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("api/Common/GetSharkItOff")]
        public Task<HttpResponseMessage> GetSharkItOff(SharkItOff sharkItOff)
        {
            ResponseBase<IList<micromsg.ShakeGetItem>> response = new ResponseBase<IList<micromsg.ShakeGetItem>>();

            var result = wechat.ShakeReport(sharkItOff.WxId, sharkItOff.Latitude, sharkItOff.Longitude);
            if (result == null || result.BaseResponse.Ret != (int)MMPro.MM.RetConst.MM_OK)
            {
                response.Success = false;
                response.Code = "402";
                response.Message = result.BaseResponse.ErrMsg.String ?? "未找到";
            }
            else
            {
                response.Data = result.ShakeGetList;
            }
            return response.ToHttpResponseAsync();
        }


        /// <summary>
        /// 附近的人
        /// </summary>
        /// <param name="peopleNearby"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("api/Common/GetPeopleNearby")]
        public Task<HttpResponseMessage> GetPeopleNearby(PeopleNearby peopleNearby)
        {
            ResponseBase<MMPro.MM.LBsContactInfo[]> response = new ResponseBase<MMPro.MM.LBsContactInfo[]>();

            var result = wechat.LbsLBSFind(peopleNearby.WxId, peopleNearby.Latitude, peopleNearby.Longitude);
            if (result == null || result.baseResponse.ret != (int)MMPro.MM.RetConst.MM_OK)
            {
                response.Success = false;
                response.Code = "402";
                response.Message = result.baseResponse.errMsg.@string ?? "未找到";
            }
            else
            {
                response.Data = result.contactList;
            }
            return response.ToHttpResponseAsync();
        }






        /// <summary>
        /// 关注公众号
        /// </summary>
        /// <param name="forkOfficialAccount"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("api/Common/ForkOfficialAccountsMessage")]
        public Task<HttpResponseMessage> ForkOfficialAccountMessage(ForkOfficialAccount forkOfficialAccount)
        {
            ResponseBase<string> response = new ResponseBase<string>();

            var result = wechat.VerifyUser(forkOfficialAccount.WxId, MMPro.MM.VerifyUserOpCode.MM_VERIFYUSER_ADDCONTACT, "", "", forkOfficialAccount.AppId, 0);
            if (result == null || result.baseResponse.ret != (int)MMPro.MM.RetConst.MM_OK)
            {
                response.Success = false;
                response.Code = "501";
                response.Message = result?.baseResponse?.errMsg?.@string;
                return response.ToHttpResponseAsync();
            }
            else
            {
                response.Data = result.userName;
            }

            return response.ToHttpResponseAsync();
        }


        /// <summary>
        /// 阅读文章
        /// </summary>
        /// <param name="readArticle"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("api/Common/ReadArticle")]
        public async Task<HttpResponseMessage> ReadArticle(ReadArticle readArticle)
        {
            ResponseBase<ReadInfoResponse> response = new ResponseBase<ReadInfoResponse>();

            var result = wechat.GetA8KeyRead(readArticle.WxId, readArticle.UserName, readArticle.Url);

            if (string.IsNullOrEmpty(result))
            {
                response.Success = false;
                response.Code = "501";
                response.Message = "阅读失败";
                return await response.ToHttpResponseAsync();
            }
            else
            {
                var arr = result.Split('|');
                var readInfoResponse = arr[0].ToObj<ReadInfoResponse>();
                readInfoResponse.GhId = arr[1];
                readInfoResponse.Article = arr[2];
                response.Data = readInfoResponse;
                response.Message = "阅读成功";
            }

            return await response.ToHttpResponseAsync();
        }


        /// <summary>
        /// 点赞文章
        /// </summary>
        /// <param name="readArticle"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("api/Common/LikeArticle")]
        public async Task<HttpResponseMessage> LikeArticle(ReadArticle readArticle)
        {
            ResponseBase<ReadInfoResponse> response = new ResponseBase<ReadInfoResponse>();

            var result = wechat.GetA8KeyLike(readArticle.WxId, readArticle.UserName, readArticle.Url);

            if (string.IsNullOrEmpty(result))
            {
                response.Success = false;
                response.Code = "501";
                response.Message = "点赞失败";
                return await response.ToHttpResponseAsync();
            }
            else
            {
                var arr = result.Split('|');
                var readInfoResponse = arr[0].ToObj<ReadInfoResponse>();
                readInfoResponse.GhId = arr[1];
                readInfoResponse.Article = arr[2];
                response.Data = readInfoResponse;
                response.Message = "点赞成功";
            }

            return await response.ToHttpResponseAsync();
        }


        /// <summary>
        /// app，网页，公众号等登录
        /// </summary>
        /// <param name="authorizationLogin"></param> 
        /// <returns></returns>
        [HttpPost]
        [Route("api/Common/AuthorizationLogin")]
        public async Task<HttpResponseMessage> AuthorizationLogin(AuthorizationLogin authorizationLogin)
        {
            ResponseBase<micromsg.GetA8KeyResp> response = new ResponseBase<micromsg.GetA8KeyResp>();

            var result = wechat.GetA8KeyLogin(authorizationLogin.WxId, "", authorizationLogin.Url, 2);
            response.Data = result;

            return await response.ToHttpResponseAsync();
        }


        /// <summary>
        /// GetA8Key
        /// </summary>
        /// <param name="getA8Key"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("api/Common/GetA8Key")]
        public async Task<HttpResponseMessage> GetA8Key(Wechat.Api.Request.Common.GetA8Key getA8Key)
        {
            ResponseBase<micromsg.GetA8KeyResp> response = new ResponseBase<micromsg.GetA8KeyResp>();
            var result = wechat.GetMpA8KeySence(getA8Key.WxId, getA8Key.UserName, getA8Key.Url, 2, getA8Key.Sence);
            response.Data = result;
            return await response.ToHttpResponseAsync();
        }

        /// <summary>
        /// GetMpA8Key
        /// </summary>
        /// <param name="getA8Key"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("api/Common/GetMpA8Key")]
        public async Task<HttpResponseMessage> GetMpA8Key(Wechat.Api.Request.Common.GetA8Key getA8Key)
        {
            ResponseBase<MMPro.MM.GetA8KeyResponse> response = new ResponseBase<MMPro.MM.GetA8KeyResponse>();
            var result = wechat.GetMpA8Key(getA8Key.WxId, getA8Key.UserName, getA8Key.Url, getA8Key.Sence);
            response.Data = result;
            return await response.ToHttpResponseAsync();
        }

        /// <summary>
        /// 小程序登录
        /// </summary>
        /// <param name="jsLoginRequest"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("api/Common/JsLogin")]
        public async Task<HttpResponseMessage> JsLogin(JsLoginRequest jsLoginRequest)
        {
            ResponseBase<JSLoginResponse> response = new ResponseBase<JSLoginResponse>();
            var result = wechat.JSLogin(jsLoginRequest.WxId, jsLoginRequest.AppId);
            response.Data = result;
            return await response.ToHttpResponseAsync();
        }

        /// <summary>
        /// 小程序操作
        /// </summary>
        /// <param name="jSOperateWxData"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("api/Common/JSOperateWxData")]
        public async Task<HttpResponseMessage> JSOperateWxData(JSOperateWxData jSOperateWxData)
        {
            ResponseBase<JSOperateWxDataResponse> response = new ResponseBase<JSOperateWxDataResponse>();
            var result = wechat.JSOperateWxData(jSOperateWxData.WxId, jSOperateWxData.AppId, jSOperateWxData.Data, jSOperateWxData.Opt);
            response.Data = result;
            return await response.ToHttpResponseAsync();
        }

    }
}