using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using Wechat.Api.Abstracts;
using Wechat.Api.Extensions;
using Wechat.Api.Filters;
using Wechat.Api.Request.Common;
using Wechat.Api.Request.Friend;
using Wechat.Api.Request.Login;
using Wechat.Api.Request.Profile;
using Wechat.Protocol;
using Wechat.Util.Extensions;
using static MMPro.MM;

namespace Wechat.Api.Controllers
{
    /// <summary>
    /// 个人信息
    /// </summary>
    public class ProfileController : WebchatControllerBase
    {


        /// <summary>
        /// 初始化用户信息
        /// </summary>
        /// <param name="wxId">微信Id</param>
        /// <returns></returns>
        [HttpPost]
        [Route("api/Profile/NewInit/{wxId}")]
        public Task<HttpResponseMessage> NewInit(string wxId)
        {
            ResponseBase<InitResponse> response = new ResponseBase<InitResponse>();
            var result = wechat.Init(wxId);
            response.Data = result;

            return response.ToHttpResponseAsync();
        }

      

    /// <summary>
    /// 获取自己简介信息
    /// </summary>
    /// <param name="wxId"></param> 
    /// <returns></returns>     
    [HttpPost]
        [Route("api/Profile/GetContractProfile/{wxId}")]
        public Task<HttpResponseMessage> GetContractProfile(string wxId)
        {
            ResponseBase<MMPro.MM.GetProfileResponse> response = new ResponseBase<MMPro.MM.GetProfileResponse>();
            var result = wechat.GetContractProfile(wxId, wxId);
            response.Data = result;
            return response.ToHttpResponseAsync();
        }


        /// <summary>
        /// 设置微信号 
        /// </summary>
        /// <param name="setAlisa"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("api/Profile/SetAlisa")]
        public async Task<HttpResponseMessage> SetAlisa(SetAlisa setAlisa)
        {
            ResponseBase<micromsg.GeneralSetResponse> response = new ResponseBase<micromsg.GeneralSetResponse>();

            var result = wechat.SetAlisa(setAlisa.WxId, setAlisa.Alisa);
            response.Data = result;
            return await response.ToHttpResponseAsync();
        }

        /// <summary>
        /// 修改头像
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [NoRequestLog]
        [Route("api/Profile/UploadHeadImage")]
        public async Task<HttpResponseMessage> UploadHeadImage(UploadHeadImage uploadHeadImage)
        {
            ResponseBase<micromsg.UploadHDHeadImgResponse> response = new ResponseBase<micromsg.UploadHDHeadImgResponse>();
            var buffer = Convert.FromBase64String(uploadHeadImage.Base64);
            var result = wechat.UploadHeadImage(uploadHeadImage.WxId, buffer);
            response.Data = result;
            return await response.ToHttpResponseAsync();
        }


        /// <summary>
        /// 修改头像(表单)
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [NoRequestLog]
        [Route("api/Profile/UploadHeadImageForm")]
        public async Task<HttpResponseMessage> UploadHeadImageForm()
        {
            ResponseBase<micromsg.UploadHDHeadImgResponse> response = new ResponseBase<micromsg.UploadHDHeadImgResponse>();
            if (!Request.Content.IsMimeMultipartContent())
            {
                response.Success = false;
                response.Code = "400";
                response.Message = "请表单提交";
                return await response.ToHttpResponseAsync();
            }
            var fileCount = HttpContext.Current.Request.Files.Count;
            if (fileCount == 0)
            {
                response.Success = false;
                response.Code = "400";
                response.Message = "请上传文件";
                return await response.ToHttpResponseAsync();
            }
            var file = HttpContext.Current.Request.Files[0];

            var wxId = HttpContext.Current.Request["WxId"];
            if (string.IsNullOrEmpty(wxId))
            {
                response.Success = false;
                response.Code = "400";
                response.Message = "WxId不能为空";
                return await response.ToHttpResponseAsync();
            }
            var result = wechat.UploadHeadImage(wxId, file.InputStream.ToBuffer());
            response.Data = result;
            return await response.ToHttpResponseAsync();
        }



        /// <summary>
        /// 修改资料
        /// </summary>
        /// <param name="updateProfile"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("api/Profile/UpdateProfile")]
        public Task<HttpResponseMessage> UpdateProfile(UpdateProfile updateProfile)
        {
            ResponseBase response = new ResponseBase();

            var getProfileResponse = wechat.GetContractProfile(updateProfile.WxId, updateProfile.WxId);

            var modUserInfo = getProfileResponse.userInfo;

            //if (!string.IsNullOrEmpty(updateProfile.NewWxId))
            //{
            //    modUserInfo.userName = new SKBuiltinString()
            //    {
            //        @string = updateProfile.NewWxId
            //    };
            //}

            modUserInfo.sex = updateProfile.Sex;

            if (!string.IsNullOrEmpty(updateProfile.Province))
            {
                modUserInfo.province = updateProfile.Province;
            }
            if (!string.IsNullOrEmpty(updateProfile.City))
            {
                modUserInfo.city = updateProfile.City;
            }
            if (!string.IsNullOrEmpty(updateProfile.Signature))
            {
                modUserInfo.signature = updateProfile.Signature;
            }
            if (!string.IsNullOrEmpty(updateProfile.NickName))
            {
                modUserInfo.nickName = new SKBuiltinString()
                {
                    @string = updateProfile.NickName
                };
            }
            //};

            var result = wechat.OpLog(updateProfile.WxId, 1, modUserInfo);
            if (result == null || result.Ret != 0 || result.OplogRet.Ret.FirstOrDefault() != 0)
            {
                response.Success = false;
                response.Code = "501";
                response.Message = "修改失败";
            }
            else
            {
                response.Message = "修改成功";
            }
            return response.ToHttpResponseAsync();
        }

        /// <summary>
        /// 验证密码
        /// </summary>
        /// <param name="newVerifyPasswd"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("api/Profile/NewVerifyPasswd")]
        public Task<HttpResponseMessage> NewVerifyPasswd(NewVerifyPasswd newVerifyPasswd)
        {
            ResponseBase<micromsg.NewVerifyPasswdResponse> response = new ResponseBase<micromsg.NewVerifyPasswdResponse>();

            var result = wechat.NewVerifyPasswd(newVerifyPasswd.WxId, newVerifyPasswd.Password);
            if (result == null || result.BaseResponse.Ret != (int)MMPro.MM.RetConst.MM_OK)
            {
                response.Success = false;
                response.Code = "501";
                response.Message = result?.BaseResponse?.ErrMsg?.String ?? "验证失败";
                return response.ToHttpResponseAsync();
            }
            else
            {
                response.Message = "验证成功";
                response.Data = result;
            }

            return response.ToHttpResponseAsync();
        }
        /// <summary>
        /// 修改密码
        /// </summary>
        /// <param name="changePassword"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("api/Profile/ChangePassword")]
        public Task<HttpResponseMessage> ChangePassword(ChangePassword changePassword)
        {
            ResponseBase<micromsg.NewSetPasswdResponse> response = new ResponseBase<micromsg.NewSetPasswdResponse>();

            var result = wechat.NewSetPasswd(changePassword.WxId, changePassword.NewPassword, changePassword.Ticket);
            if (result == null || result.BaseResponse.Ret != (int)MMPro.MM.RetConst.MM_OK)
            {
                response.Success = false;
                response.Code = "501";
                response.Message = result?.BaseResponse?.ErrMsg?.String ?? "修改失败";
                return response.ToHttpResponseAsync();
            }
            else
            {
                response.Message = "修改成功";
                response.Data = result;
            }

            return response.ToHttpResponseAsync();
        }


        /// <summary>
        /// 一键修改密码
        /// </summary>
        /// <param name="changePassword"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("api/user/OneChangePassword")]
        public Task<HttpResponseMessage> OneChangePassword(OneChangePwd changePassword)
        {
            ResponseBase<micromsg.NewSetPasswdResponse> response = new ResponseBase<micromsg.NewSetPasswdResponse>();

            var result = wechat.NewVerifyPasswd(changePassword.WxId, changePassword.Password);

            if (result == null || result.BaseResponse.Ret != (int)MMPro.MM.RetConst.MM_OK)
            {
                response.Success = false;
                response.Code = "501";
                response.Message = result?.BaseResponse?.ErrMsg?.String ?? "验证失败";
                return response.ToHttpResponseAsync();
            }
            else
            {
                var m_result = wechat.NewSetPasswd(changePassword.WxId, changePassword.NewPassword, result.Ticket);
                if (m_result == null || m_result.BaseResponse.Ret != (int)MMPro.MM.RetConst.MM_OK)
                {
                    response.Success = false;
                    response.Code = "501";
                    response.Message = m_result?.BaseResponse?.ErrMsg?.String ?? "修改失败";
                    return response.ToHttpResponseAsync();
                }
                else
                {
                    response.Message = "修改成功";
                    response.Data = m_result;
                }
            }

            return response.ToHttpResponseAsync();
        }

        /*
        /// <summary>
        /// 绑定邮箱
        /// </summary>
        /// <param name="bindEmail"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("api/Profile/BindEmail")]
        public Task<HttpResponseMessage> BindEmail(BindEmail bindEmail)
        {
            ResponseBase response = new ResponseBase();

            var result = wechat.BindEmail(bindEmail.WxId, bindEmail.Email);
            if (result == null || result.BaseResponse.Ret != (int)MMPro.MM.RetConst.MM_OK)
            {
                response.Success = false;
                response.Code = "402";
                response.Message = result.BaseResponse.ErrMsg.String ?? "绑定失败";

            }
            else
            {
                response.Message = "绑定成功";
            }

            return response.ToHttpResponseAsync();
        }

        /// <summary>
        /// 获取短信验证码
        /// </summary>
        /// <param name="bindMobile"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("api/Profile/GetVerifycode")]
        public Task<HttpResponseMessage> GetVerifycode(BindMobile bindMobile)
        {
            ResponseBase<micromsg.BindOpMobileResponse> response = new ResponseBase<micromsg.BindOpMobileResponse>();

            var result = wechat.BindMobile(bindMobile.WxId, bindMobile.phone, bindMobile.code, 1);

            if (result == null || result.BaseResponse.Ret != (int)MMPro.MM.RetConst.MM_OK)
            {
                response.Success = false;
                response.Code = "501";
                response.Message = result?.BaseResponse?.ErrMsg?.String ?? "获取失败";
                return response.ToHttpResponseAsync();
            }
            else
            {
                response.Message = "获取成功";
                response.Data = result;
            }

            return response.ToHttpResponseAsync();
        }

        /// <summary>
        /// 绑定手机号
        /// </summary>
        /// <param name="bindMobile"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("api/Profile/BindMobile")]
        public Task<HttpResponseMessage> BindMobile(BindMobile bindMobile)
        {
            ResponseBase<micromsg.BindOpMobileResponse> response = new ResponseBase<micromsg.BindOpMobileResponse>();

            var result = wechat.BindMobile(bindMobile.WxId, bindMobile.phone, bindMobile.code, 2);

            if (result == null || result.BaseResponse.Ret != (int)MMPro.MM.RetConst.MM_OK)
            {
                response.Success = false;
                response.Code = "501";
                response.Message = result?.BaseResponse?.ErrMsg?.String ?? "绑定失败";
                return response.ToHttpResponseAsync();
            }
            else
            {
                response.Message = "绑定成功";
                response.Data = result;
            }

            return response.ToHttpResponseAsync();
        }

        /// <summary>
        /// 解绑手机号
        /// </summary>
        /// <param name="bindMobile"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("api/Profile/UnBindMobile")]
        public Task<HttpResponseMessage> UnBindMobile(BindMobile bindMobile)
        {
            ResponseBase<micromsg.BindOpMobileResponse> response = new ResponseBase<micromsg.BindOpMobileResponse>();

            var result = wechat.BindMobile(bindMobile.WxId, bindMobile.phone, bindMobile.code, 3);

            if (result == null || result.BaseResponse.Ret != (int)MMPro.MM.RetConst.MM_OK)
            {
                response.Success = false;
                response.Code = "501";
                response.Message = result?.BaseResponse?.ErrMsg?.String ?? "解绑失败";
                return response.ToHttpResponseAsync();
            }
            else
            {
                response.Message = "解绑成功";
                response.Data = result;
            }

            return response.ToHttpResponseAsync();
        }

        /// <summary>
        /// 验证身份证
        /// </summary>
        /// <param name="verifyIdCard"></param>
        /// <returns></returns>
        [Route("api/Profile/VerifyIdCard")]
        public Task<HttpResponseMessage> VerifyIdCard(VerifyIdCard verifyIdCard)
        {
            ResponseBase<micromsg.VerifyPersonalInfoResp> response = new ResponseBase<micromsg.VerifyPersonalInfoResp>();
            var result = wechat.VerifyPersonalInfo(verifyIdCard.WxId, verifyIdCard.RealName, verifyIdCard.IdCardType, verifyIdCard.IDCardNumber);
            response.Data = result;
            return response.ToHttpResponseAsync();
        }
        */
        /// <summary>
        /// 设置好友加我验证
        /// </summary>
        /// <param name="setFunctionSwitch"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("api/Profile/SetFunctionSwitch")]
        public Task<HttpResponseMessage> SetFunctionSwitch(SetFunctionSwitch setFunctionSwitch)
        {
            ResponseBase response = new ResponseBase();
            micromsg.FunctionSwitch function = new micromsg.FunctionSwitch()
            {
                FunctionId = (uint)setFunctionSwitch.FunctionId,
                SwitchValue = (uint)setFunctionSwitch.SwitchValue
            };

            var result = wechat.OpLog(setFunctionSwitch.WxId, 23, function);
            if (result == null || result.Ret != 0 || result.OplogRet.Ret.FirstOrDefault() != 0)
            {
                response.Success = false;
                response.Code = "501";
                response.Message = "修改失败";
            }
            else
            {
                response.Success = true;
                response.Message = "修改成功";
            }

            return response.ToHttpResponseAsync();
        }



        /// <summary>
        /// 获取自己二维码或群
        /// </summary>
        /// <param name="getMyQrCode"></param> 
        /// <returns></returns>
        [HttpPost()]
        [NoResponseLog]
        [Route("api/Profile/GetMyQrCode")]
        public Task<HttpResponseMessage> GetMyQrCode(GetMyQrCode getMyQrCode)
        {
            ResponseBase<string> response = new ResponseBase<string>();
            getMyQrCode.ToWxId = string.IsNullOrWhiteSpace(getMyQrCode.ToWxId) ? getMyQrCode.WxId : getMyQrCode.ToWxId;
            var result = wechat.GetMyQRCode(getMyQrCode.WxId, getMyQrCode.ToWxId);
            if (result != null && result.BaseResponse.Ret == (int)MMPro.MM.RetConst.MM_OK)
            {
                response.Data = $"data:img/jpg;base64,{Convert.ToBase64String(result.QRCode.Buffer)}";
            }
            else
            {
                response.Success = false;
                response.Code = "501";
                response.Message = "获取二维码失败";
            }

            return response.ToHttpResponseAsync();
        }
    }
}