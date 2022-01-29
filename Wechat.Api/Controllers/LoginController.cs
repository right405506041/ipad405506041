using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using Wechat.Api.Abstracts;
using Wechat.Api.Extensions;
using Wechat.Api.Filters;
using Wechat.Api.Helper;
using Wechat.Api.Request.Login;
using Wechat.Api.Response.Login;
using Wechat.Protocol;
using Wechat.Protocol.Andriod;
using Wechat.Util.Extensions;
using static MMPro.MM;
using System.Configuration;

namespace Wechat.Api.Controllers
{
    /// <summary>
    /// 登陆
    /// </summary> 
    public class LoginController : WebchatControllerBase
    {

        #region 生成二维码
        ///// <summary>
        ///// 生成二维码
        ///// </summary>
        ///// <returns></returns>
        //[HttpGet()]
        //[NoRequestLog]
        //[Route("api/Login/GetQrCode")]
        //public async Task<HttpResponseMessage> GetQrCodeWithStream()
        //{
        //    ResponseBase response = new ResponseBase();

        //    var result = wechat.GetLoginQRcode(0);
        //    if (result != null && result.baseResponse.ret == MMPro.MM.RetConst.MM_OK)
        //    {
        //        Image image = new Bitmap(new MemoryStream(result.qRCode.src));
        //        Dictionary<string, string> dic = new Dictionary<string, string>();
        //        dic.Add("Uuid", result.uuid);
        //        dic.Add("ExpiredTime", DateTime.Now.AddSeconds(result.expiredTime).ToString());

        //        //image = image.ToCustomeImage(dic);
        //        var res = await image.ToHttpImageResponseAsync();
        //        res.Headers.Add("Uuid", result.uuid);
        //        res.Headers.Add("ExpiredTime", dic["ExpiredTime"]);
        //        return res;
        //    }
        //    else
        //    {
        //        response.Success = false;
        //        response.Code = "501";
        //        response.Message = "获取二维码失败";
        //    }

        //    return await response.ToHttpResponseAsync();
        //}
        #endregion


        /// <summary>
        /// 获取登陆二维码
        /// </summary>
        /// <returns></returns> 
        [HttpPost()]
        [NoRequestLog]
        [Route("api/Login/GetQrCode")]
        public Task<HttpResponseMessage> GetQrCode(GetQrCode getQrCode)
        {
            ResponseBase<QrCodeResponse> response = new ResponseBase<QrCodeResponse>();

            var result = wechat.GetLoginQRcode(0, getQrCode?.ProxyIp, getQrCode?.ProxyUserName, getQrCode?.ProxyPassword, getQrCode?.DeviceId, getQrCode?.DeviceName);
            if (result != null && result.baseResponse.ret == MMPro.MM.RetConst.MM_OK)
            {
                QrCodeResponse qrCodeResponse = new QrCodeResponse();
                qrCodeResponse.QrBase64 = $"data:img/jpg;base64,{Convert.ToBase64String(result.qRCode.src)}";
                qrCodeResponse.Uuid = result.uuid;
                qrCodeResponse.ExpiredTime = DateTime.Now.AddSeconds(result.expiredTime);
                response.Data = qrCodeResponse;
            }
            else
            {
                response.Success = false;
                response.Code = "501";
                response.Message = "获取二维码失败";
            }

            return response.ToHttpResponseAsync();
        }



       /// <summary>
       ///  验证登录（扫码登录后）
       /// </summary>
       /// <param name="uuid"></param>
       /// <returns></returns>
        [HttpPost]
        [Route("api/Login/CheckLogin/{Uuid}")]
        public Task<HttpResponseMessage> CheckLogin(string uuid)
        {
            ResponseBase<CheckLoginResponse> response = new ResponseBase<CheckLoginResponse>();

            var result = wechat.CheckLoginQRCode(uuid);
            CheckLoginResponse checkLoginResponse = new CheckLoginResponse();
            checkLoginResponse.State = result.State;
            checkLoginResponse.Uuid = result.Uuid;
            checkLoginResponse.WxId = result.WxId;
            checkLoginResponse.NickName = result.NickName;
            checkLoginResponse.Device = result.Device;
            checkLoginResponse.HeadUrl = result.HeadUrl;
            checkLoginResponse.Mobile = result.BindMobile;
            checkLoginResponse.Email = result.BindEmail;
            checkLoginResponse.Alias = result.Alias;
            checkLoginResponse.DeviceId = result.DeviceId;
            checkLoginResponse.DeviceName = result.DeviceName;
            if (result.WxId != null)
            {
                checkLoginResponse.Data62 = wechat.Get62Data(result.WxId);
            }

            response.Data = checkLoginResponse;

            return response.ToHttpResponseAsync();
        }
        /// <summary>
        /// 检查代理IP可用性
        /// </summary>
        /// <param name="CheckProxy"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("api/Login/CheckProxy")]
        public Task<HttpResponseMessage> CheckProxy(CheckProxy CheckProxy)
        {
            ResponseBase<ManualAuthResponse> response = new ResponseBase<ManualAuthResponse>();

            var result = wechat.CheckProxy(CheckProxy.ProxyIp, CheckProxy.ProxyUserName, CheckProxy.ProxyPassword);
            if (result != null)
            {
                response.Success = true;
                response.Code = "1";
                response.Message = result;
                return response.ToHttpResponseAsync();
            }
            else
            {
                response.Success = false;
                response.Code = "401";
                response.Message = "IP无用";
                return response.ToHttpResponseAsync();
            }

        }

        /// <summary>
        /// 心跳
        /// </summary>
        /// <param name="wxId">微信Id</param>
        /// <returns></returns>
        [HttpPost]
        [Route("api/Profile/HeartBeat/{wxId}")]
        public Task<HttpResponseMessage> HeartBeat(string wxId)
        {
            ResponseBase<micromsg.HeartBeatResponse> response = new ResponseBase<micromsg.HeartBeatResponse>();
            var result = wechat.HeartBeat(wxId);
            response.Data = result;
            return response.ToHttpResponseAsync();
        }

        /// <summary>
        /// 0 未启动 1：已启动 2：已关闭
        /// </summary>
        public static Dictionary<string, int> State = new Dictionary<string, int>();

        /// <summary>
        /// 记录错误次数
        /// </summary>
        public static Dictionary<string, int> Count = new Dictionary<string, int>();

        /// <summary>
        /// 启动自动心跳
        /// </summary>
        /// <returns></returns>
        [HttpGet()]
        [Route("api/Profile/StartHeartBeat/{wxId}")]
        public Task<HttpResponseMessage> StartHeartBeat(string wxId)
        {
            ResponseBase response = new ResponseBase();
            //任务开始
            Task.Run(() =>
            {
                State[wxId] = 1;
                while (true)
                {
                    if (State[wxId] == 2)
                    {
                        return;
                    }
                    try
                    {
                        //核心心跳代码，主要是10-30秒执行一次这个方法，不掉线的话，正常返回，掉线会抛出异常
                        var result = wechat.HeartBeat(wxId);
                    }
                    catch (Exception e)
                    {
                        Util.Log.Logger.GetLog<LoginController>().Info($"HeartBeat [wxId: {wxId}]：{e.Message}");
                        //失败后发送三次错误提醒
                        for (int i = 1; i <= 3; i++)
                        {
                            string urlEncode = System.Web.HttpUtility.UrlEncode(e.Message);
                            wechat.sendNotice(ConfigurationManager.AppSettings["chatServer"] + "/server/api/dropped/wxid/" + wxId + "/count/" + i + "/error/" + urlEncode);
                            Debug.WriteLine("e.Message:" + e.Message);
                            if (i == 3)
                            {
                                //标记该账户掉线不用心跳了
                                State[wxId] = 2;
                            }
                        }
                    }
                    //随机时间心跳10000-30000
                    Random randomNums = new Random();
                    int delay = randomNums.Next(10000, 30000);
                    //休息10-30秒
                    Thread.Sleep(delay);
                }
            });
            response.Message = "启动成功";
            return response.ToHttpResponseAsync();
        }

        /// <summary>
        /// 关闭自动心跳
        /// </summary>
        /// <returns></returns>
        [HttpGet()]
        [Route("api/Profile/CloseHeartBeat/{wxId}")]
        public Task<HttpResponseMessage> CloseHeartBeat(string wxId)
        {
            ResponseBase response = new ResponseBase();
            State[wxId] = 2;
            response.Message = "关闭成功";
            return response.ToHttpResponseAsync();
        }

        /// <summary>
        /// 心跳状态
        /// </summary>
        /// <returns></returns>
        [HttpGet()]
        [Route("api/Profile/StateHeartBeat/{wxId}")]
        public Task<HttpResponseMessage> StateHeartBeat(string wxId)
        {
            ResponseBase response = new ResponseBase();
            if (State.ContainsKey(wxId))
            {
                if (State[wxId] == 1)
                {
                    response.Message = "已启动";
                }
                else
                {
                    response.Message = "已关闭";
                }
            }
            else
            {
                response.Message = "未启动";
            }

            return response.ToHttpResponseAsync();
        }

        /// <summary>
        /// Data62登陆
        /// </summary>
        /// <param name="data62Login"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("api/Login/Data62Login")]
        public Task<HttpResponseMessage> Data62Login(Data62Login data62Login)
        {
            ResponseBase<ManualAuthResponse> response = new ResponseBase<ManualAuthResponse>();

            var result = wechat.UserLogin(data62Login.UserName, data62Login.Password, data62Login.Data62, data62Login.ProxyIp, data62Login.ProxyUserName, data62Login.ProxyPassword);
            if (result != null || result.baseResponse.ret == (int)MMPro.MM.RetConst.MM_OK)
            {
                response.Data = result;

                if (!string.IsNullOrEmpty(result.accountInfo.wxid))
                {
                    //开始初始化
                    wechat.Init(result.accountInfo.wxid);
                    //开启自动心跳
                    Task.Run(() =>
                    {
                        State[result.accountInfo.wxid] = 1;
                        while (true)
                        {
                            if (State[result.accountInfo.wxid] == 2)
                            {
                                return;
                            }
                            try
                            {
                                wechat.HeartBeat(result.accountInfo.wxid);
                            }
                            catch (Exception e)
                            {
                                Util.Log.Logger.GetLog<LoginController>().Info($"HeartBeat [wxId: {result.accountInfo.wxid}]：{e.Message}");
                                //失败后发送三次错误提醒
                                for (int i = 1; i <= 3; i++)
                                {
                                    string urlEncode = System.Web.HttpUtility.UrlEncode(e.Message);
                                    wechat.sendNotice(ConfigurationManager.AppSettings["chatServer"] +"/server/api/dropped/wxid/" + result.accountInfo.wxid + "/count/" + i + "/error/" + urlEncode);
                                    Debug.WriteLine("e.Message:" + e.Message);
                                    if (i == 3)
                                    {
                                        State[result.accountInfo.wxid] = 2;
                                    }
                                }
                            }
                            //随机时间心跳10000-30000
                            Random randomNums = new Random();
                            int delay = randomNums.Next(10000, 30000);
                            Thread.Sleep(delay);
                        }
                    });
                }
            }
            return response.ToHttpResponseAsync();

        }

        /// <summary>
        /// DataA16登陆
        /// </summary>
        /// <param name="dataA16Login"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("api/Login/DataA16Login")]
        public Task<HttpResponseMessage> DataA16Login(DataA16Login dataA16Login)
        {
            ResponseBase<ManualAuthResponse> response = new ResponseBase<ManualAuthResponse>();

            var result = wechat.AndroidManualAuth(dataA16Login.UserName, dataA16Login.Password, dataA16Login.DataA16, Guid.NewGuid().ToString(), dataA16Login.ProxyIp, dataA16Login.ProxyUserName, dataA16Login.ProxyPassword);
            if (result != null || result.baseResponse.ret == (int)MMPro.MM.RetConst.MM_OK)
            {
                response.Data = result;
                if (!string.IsNullOrEmpty(result.accountInfo.wxid))
                {
                    //开始初始化
                    wechat.Init(result.accountInfo.wxid);
                    //开启自动心跳
                    Task.Run(() =>
                    {
                        State[result.accountInfo.wxid] = 1;
                        while (true)
                        {
                            if (State[result.accountInfo.wxid] == 2)
                            {
                                return;
                            }
                            try
                            {
                                wechat.HeartBeat(result.accountInfo.wxid);
                            }
                            catch (Exception e)
                            {
                                Util.Log.Logger.GetLog<LoginController>().Info($"HeartBeat [wxId: {result.accountInfo.wxid}]：{e.Message}");
                                //失败后发送三次错误提醒
                                for (int i = 1; i <= 3; i++)
                                {
                                    string urlEncode = System.Web.HttpUtility.UrlEncode(e.Message);
                                    wechat.sendNotice(ConfigurationManager.AppSettings["chatServer"] + "/server/api/dropped/wxid/" + result.accountInfo.wxid + "/count/" + i + "/error/" + urlEncode);
                                    Debug.WriteLine("e.Message:" + e.Message);
                                    if (i == 3)
                                    {
                                        State[result.accountInfo.wxid] = 2;
                                    }
                                }
                            }
                            //随机时间心跳10000-30000
                            Random randomNums = new Random();
                            int delay = randomNums.Next(10000, 30000);
                            Thread.Sleep(delay);
                        }
                    });
                }
            }
            return response.ToHttpResponseAsync();
        }

        /// <summary>
        /// 62转A16
        /// </summary>
        /// <param name="data62Login"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("api/Login/Data62ToA16")]
        public Task<HttpResponseMessage> Data62ToA16(Data62Login data62Login)
        {
            ResponseBase<Data62ToA16Response> response = new ResponseBase<Data62ToA16Response>();

            var result = wechat.UserLogin(data62Login.UserName, data62Login.Password, data62Login.Data62, data62Login.ProxyIp, data62Login.ProxyUserName, data62Login.ProxyPassword);
            Data62ToA16Response data62ToA16Response = new Data62ToA16Response();
            data62ToA16Response.ManualAuthResponse = result;
            response.Data = data62ToA16Response;
            if (result.baseResponse.ret == RetConst.MM_OK)
            {
                string a16 = Fun.GenDeviceID();
                data62ToA16Response.A16 = a16;
                var qrcode = wechat.A16LoginAndGetQRCode(data62Login.UserName, data62Login.Password, a16, data62Login.ProxyIp, data62Login.ProxyUserName, data62Login.ProxyPassword);
                if (qrcode.status == 1)
                {

                    response.Message = "转换A16成功";
                    return response.ToHttpResponseAsync();

                }

                if (!string.IsNullOrEmpty(qrcode.uuid))
                {
                    string qrurl = "https://login.weixin.qq.com/q/" + qrcode.uuid;
                    var a8KeyResp = wechat.GetA8Key(result.accountInfo.wxid, "", qrurl);

                    if (a8KeyResp.baseResponse.ret == RetConst.MM_OK)
                    {
                        if (a8KeyResp.fullURL != "")
                        {

                            HttpHelper httpHelper = new HttpHelper();
                            HttpItem httpItem4 = new HttpItem
                            {
                                URL = a8KeyResp.fullURL,
                                UserAgent = "Mozilla/5.0 (Windows NT 10.0; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/63.0.3239.132 Safari/537.36",
                                Accept = "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,image/apng,*/*;q=0.8"
                            };
                            HttpResult httpResult4 = httpHelper.GetHtml(httpItem4);
                            string urlConfirm = HttpHelper.GetBetweenHtml(httpResult4.Html, "confirm=1", ">");
                            string relaUrl = "https://login.weixin.qq.com/confirm?confirm=1" + urlConfirm.Replace("\"", "");
                            string cookies = HttpHelper.GetSmallCookie(httpResult4.Cookie);

                            httpItem4 = new HttpItem
                            {
                                URL = relaUrl,
                                Method = "POST",
                                ContentType = "application/x-www-form-urlencoded",
                                Cookie = cookies,
                                UserAgent = "Mozilla/5.0 (Windows NT 10.0; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/63.0.3239.132 Safari/537.36",
                                Accept = "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,image/apng,*/*;q=0.8"
                            };

                            httpResult4 = httpHelper.GetHtml(httpItem4);
                            string returl = "https://login.weixin.qq.com/cgi-bin/mmwebwx-bin/login?uuid=" + qrcode.uuid + "&r=" + (DateTime.Now.ToUniversalTime().Ticks - 621355968000000000L) / 10000000 + "&t=simple_auth/w_qrcode_show&&ticket=" + qrcode.ticket + "&wechat_real_lang=zh_CN&idc=2&qrcliticket=" + qrcode.qrcliticket;
                            httpItem4 = new HttpItem
                            {
                                URL = returl,
                                UserAgent = "Mozilla/5.0 (Windows NT 10.0; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/63.0.3239.132 Safari/537.36",
                                Accept = "*/*"
                            };

                            httpResult4 = httpHelper.GetHtml(httpItem4);
                            string redirect_uri = HttpHelper.GetBetweenHtml(httpResult4.Html, "window.redirect_uri=", ";").Replace("\"", "").Trim();
                            httpItem4 = new HttpItem
                            {
                                URL = redirect_uri,
                                UserAgent = "Mozilla/5.0 (Windows NT 10.0; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/63.0.3239.132 Safari/537.36",
                                Accept = "*/*"
                            };
                            httpResult4 = httpHelper.GetHtml(httpItem4);

                            var ret = wechat.AndroidManualAuth(data62Login.UserName, data62Login.Password, a16, Guid.NewGuid().ToString(), data62Login.ProxyIp, data62Login.ProxyUserName, data62Login.ProxyPassword);

                            if (ret.baseResponse.ret == RetConst.MM_OK)
                            {
                                data62ToA16Response.A16 = a16;
                                data62ToA16Response.ManualAuthResponse = ret;
                                response.Data = data62ToA16Response;
                                response.Message = "转换A16成功";
                                return response.ToHttpResponseAsync();
                            }


                        }


                    }

                }


            }
            response.Data = data62ToA16Response;
            response.Message = "转换A16失败";
            response.Success = false;
            return response.ToHttpResponseAsync();
        }

        /// <summary>
        /// A16转62
        /// </summary>
        /// <param name="dataA16Login"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("api/Login/A16ToData62")]
        public Task<HttpResponseMessage> DataA16To62(DataA16Login dataA16Login)
        {
            ResponseBase<DataA16To62Response> response = new ResponseBase<DataA16To62Response>();
            var result = wechat.AndroidManualAuth(dataA16Login.UserName, dataA16Login.Password, dataA16Login.DataA16, Guid.NewGuid().ToString(), dataA16Login.ProxyIp, dataA16Login.ProxyUserName, dataA16Login.ProxyPassword);
            DataA16To62Response dataA16To62Response = new DataA16To62Response();
            dataA16To62Response.ManualAuthResponse = result;
            if (result.baseResponse.ret == RetConst.MM_OK)
            {
                string wxnew62 = Wechat.Protocol.Util.SixTwoData(Guid.NewGuid().ToString("N"));
                var qrcode = wechat.UserLoginQRCode(dataA16Login.UserName, dataA16Login.Password, wxnew62, dataA16Login.ProxyIp, dataA16Login.ProxyUserName, dataA16Login.ProxyPassword);

                if (!string.IsNullOrEmpty(qrcode.uuid))
                {
                    string qrurl = "https://login.weixin.qq.com/q/" + qrcode.uuid;

                    var rsult = wechat.GetA8Key(result.accountInfo.wxid, "", qrurl);
                    if (!string.IsNullOrEmpty(rsult.fullURL))
                    {
                        HttpHelper httpHelper = new HttpHelper();
                        HttpItem httpItem4 = new HttpItem
                        {
                            URL = rsult.fullURL,
                            UserAgent = "Mozilla/5.0 (Windows NT 10.0; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/63.0.3239.132 Safari/537.36",
                            Accept = "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,image/apng,*/*;q=0.8"
                        };
                        HttpResult httpResult4 = httpHelper.GetHtml(httpItem4);
                        string urlConfirm = HttpHelper.GetBetweenHtml(httpResult4.Html, "confirm=1", ">");
                        string relaUrl = "https://login.weixin.qq.com/confirm?confirm=1" + urlConfirm.Replace("\"", "");
                        string cookies = HttpHelper.GetSmallCookie(httpResult4.Cookie);
                        httpItem4 = new HttpItem
                        {
                            URL = relaUrl,
                            Method = "POST",
                            ContentType = "application/x-www-form-urlencoded",
                            Cookie = cookies,
                            UserAgent = "Mozilla/5.0 (Windows NT 10.0; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/63.0.3239.132 Safari/537.36",
                            Accept = "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,image/apng,*/*;q=0.8"
                        };
                        httpResult4 = httpHelper.GetHtml(httpItem4);
                        string returl = "https://login.weixin.qq.com/cgi-bin/mmwebwx-bin/login?uuid=" + qrcode.uuid + "&r=" + (DateTime.Now.ToUniversalTime().Ticks - 621355968000000000L) / 10000000 + "&t=simple_auth/w_qrcode_show&&ticket=" + qrcode.ticket + "&wechat_real_lang=zh_CN&idc=2&qrcliticket=" + qrcode.qrcliticket;
                        httpItem4 = new HttpItem
                        {
                            URL = returl,
                            UserAgent = "Mozilla/5.0 (Windows NT 10.0; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/63.0.3239.132 Safari/537.36",
                            Accept = "*/*"
                        };
                        httpResult4 = httpHelper.GetHtml(httpItem4);
                        string redirect_uri = HttpHelper.GetBetweenHtml(httpResult4.Html, "window.redirect_uri=", ";").Replace("\"", "").Trim();
                        httpItem4 = new HttpItem
                        {
                            URL = redirect_uri,
                            UserAgent = "Mozilla/5.0 (Windows NT 10.0; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/63.0.3239.132 Safari/537.36",
                            Accept = "*/*"
                        };
                        httpResult4 = httpHelper.GetHtml(httpItem4);

                        var datalogin = wechat.UserLogin(dataA16Login.UserName, dataA16Login.Password, wxnew62);
                        if (datalogin.baseResponse.ret == RetConst.MM_OK)
                        {
                            response.Message = "转换62成功";

                            dataA16To62Response.Data62 = wxnew62;
                            dataA16To62Response.ManualAuthResponse = datalogin;
                            response.Data = dataA16To62Response;
                            return response.ToHttpResponseAsync();
                        }

                    }
                }
            }
            response.Data = dataA16To62Response;
            response.Message = "转换62失败";
            response.Success = false;
            return response.ToHttpResponseAsync();
        }
        /// <summary>
        /// 二次登陆
        /// </summary>
        /// <param name="wxId"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("api/Login/TwiceLogin/{wxId}")]
        public Task<HttpResponseMessage> TwiceLogin(string wxId)
        {

            ResponseBase<ManualAuthResponse> response = new ResponseBase<ManualAuthResponse>();
            var result = wechat.TwiceLogin(wxId);
            if (result == null || result.baseResponse.ret != (int)MMPro.MM.RetConst.MM_OK)
            {
                response.Success = false;
                response.Code = "501";
                response.Message = result.baseResponse.errMsg.@string ?? "登陆失败";
            }
            else
            {
                response.Data = result;
                response.Message = "登陆成功";
            }

            return response.ToHttpResponseAsync();
        }



        /// <summary>
        /// 二维码唤醒登录
        /// </summary>
        /// <param name="wxId"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("api/Login/TwiceQrCodeLogin/{wxId}")]
        public Task<HttpResponseMessage> TwiceQrCodeLogin(string wxId)
        {

            ResponseBase<PushLoginURLResponse> response = new ResponseBase<PushLoginURLResponse>();
            var result = wechat.TwiceQrCodeLogin(wxId);
            if (result == null || result.baseResponse.ret != (int)MMPro.MM.RetConst.MM_OK)
            {
                response.Success = false;
                response.Code = "501";
                response.Message = result.baseResponse.errMsg.@string ?? "登陆失败";
            }
            else
            {
                response.Data = result;
                response.Message = "登陆成功";
            }

            return response.ToHttpResponseAsync();
        }

        /// <summary>
        /// 退出登录
        /// </summary>
        /// <param name="wxId">微信Id</param>
        /// <returns></returns>
        [HttpPost]
        [Route("api/Login/Logout/{wxId}")]
        public Task<HttpResponseMessage> Logout(string wxId)
        {
            ResponseBase<InitResponse> response = new ResponseBase<InitResponse>();

            var result = wechat.logOut(wxId);
            if (result == null || result.BaseResponse.Ret != (int)MMPro.MM.RetConst.MM_OK)
            {
                response.Success = false;
                response.Code = "501";
                response.Message = result.BaseResponse.ErrMsg.String ?? "退出失败";
            }
            else
            {
                response.Message = "退出成功";
            }

            return response.ToHttpResponseAsync();
        }



        /// <summary>
        /// 扫码登录其他设备（A16或者62登录的账号可用）
        /// </summary>
        /// <param name="extDeviceLoginConfirmOK"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("api/Login/ExtDeviceLoginConfirmGet")]
        public Task<HttpResponseMessage> ExtDeviceLoginConfirmGet(ExtDeviceLoginConfirmOK extDeviceLoginConfirmOK)
        {
            ResponseBase<micromsg.ExtDeviceLoginConfirmGetResponse> response = new ResponseBase<micromsg.ExtDeviceLoginConfirmGetResponse>();
            var result = wechat.ExtDeviceLoginConfirmGet(extDeviceLoginConfirmOK.WxId, extDeviceLoginConfirmOK.LoginUrl);
            response.Data = result;
            return response.ToHttpResponseAsync();
        }

        /// <summary>
        /// 确认登录其他设备
        /// </summary>
        /// <param name="extDeviceLoginConfirmOK"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("api/Login/ExtDeviceLoginConfirmOK")]
        public Task<HttpResponseMessage> ExtDeviceLoginConfirmOK(ExtDeviceLoginConfirmOK extDeviceLoginConfirmOK)
        {
            ResponseBase<micromsg.ExtDeviceLoginConfirmOKResponse> response = new ResponseBase<micromsg.ExtDeviceLoginConfirmOKResponse>();
            var result = wechat.ExtDeviceLoginConfirmOK(extDeviceLoginConfirmOK.WxId, extDeviceLoginConfirmOK.LoginUrl);
            response.Data = result;
            return response.ToHttpResponseAsync();
        }

        /// <summary>
        /// 获取登陆Url
        /// </summary>
        /// <param name="getLoginUrl"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("api/Login/GetLoginUrl")]
        public Task<HttpResponseMessage> GetLoginUrl(GetLoginUrl getLoginUrl)
        {
            ResponseBase<micromsg.GetLoginURLResponse> response = new ResponseBase<micromsg.GetLoginURLResponse>();
            var result = wechat.GetLoginURL(getLoginUrl.WxId, getLoginUrl.Uuid);
            response.Data = result;
            return response.ToHttpResponseAsync();
        }

        /// <summary>
        /// 获取62数据
        /// </summary>
        /// <param name="wxId"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("api/Login/Get62Data/{wxId}")]
        public Task<HttpResponseMessage> Get62Data(string wxId)
        {
            ResponseBase<string> response = new ResponseBase<string>();
            var result = wechat.Get62Data(wxId);
            response.Data = result;
            return response.ToHttpResponseAsync();
        }
        /// <summary>
        /// 辅助登录新手机设备
        /// </summary>
        /// <param name="phoneLogin"></param>
        /// <returns></returns>
        [HttpPost, Route("api/Login/PhoneDeviceLogin")]
        public Task<HttpResponseMessage> PhoneDeviceLogin(PhoneLogin phoneLogin)
        {
            ResponseBase responseBase = new ResponseBase();
            MMPro.MM.GetA8KeyResponse a8Key = wechat.GetA8Key(phoneLogin.WxId, "", phoneLogin.Url, 2, null);
            bool flag = a8Key.fullURL.Contains("https://login.weixin.qq.com");
            if (flag)
            {
                SeleniumHelper seleniumHelper = new SeleniumHelper(Browsers.Chrome);
                try
                {
                    seleniumHelper.GoToUrl(a8Key.fullURL);
                    seleniumHelper.ClickElement(seleniumHelper.FindElementByXPath("/html/body/form/div[3]/p/button"));
                    responseBase.Message = "辅助成功，请在手机再次登录";
                }
                catch (Exception ex)
                {
                    responseBase.Success = false;
                    responseBase.Code = "501";
                    responseBase.Message = "登录失败,二维码已过期-" + ex.Message;
                }
                seleniumHelper.Cleanup();
            }
            else
            {
                responseBase.Success = false;
                responseBase.Code = "501";
                responseBase.Message = "登录失败";
            }
            return responseBase.ToHttpResponseAsync();
        }
        /// <summary>
        /// 辅助登录其他应用(https://open.weixin.qq.com/)
        /// </summary>
        /// <param name="phoneLogin"></param>
        /// <returns></returns>
        [HttpPost, Route("api/Login/OtherDeviceLogin")]
        public Task<HttpResponseMessage> OtherDeviceLogin(PhoneLogin phoneLogin)
        {
            ResponseBase responseBase = new ResponseBase();
            MMPro.MM.GetA8KeyResponse a8Key = wechat.GetA8Key(phoneLogin.WxId, "", phoneLogin.Url, 2, null);
            bool flag = a8Key.fullURL.Contains("https://open.weixin.qq.com/");
            if (flag)
            {
                SeleniumHelper seleniumHelper = new SeleniumHelper(Browsers.Chrome);
                try
                {
                    seleniumHelper.GoToUrl(a8Key.fullURL);
                    seleniumHelper.ClickElement(seleniumHelper.FindElementByXPath("//*[@id=\"js_allow\"]"));
                    responseBase.Message = "登录成功";
                }
                catch (Exception ex)
                {
                    responseBase.Success = false;
                    responseBase.Code = "501";
                    responseBase.Message = "登录失败,二维码已过期-" + ex.Message;
                }
                seleniumHelper.Cleanup();
            }
            else
            {
                responseBase.Success = false;
                responseBase.Code = "501";
                responseBase.Message = "登录失败";
            }
            return responseBase.ToHttpResponseAsync();
        }

        ///// <summary>
        ///// 获取短信验证码
        ///// </summary>
        ///// <param name="bindMobile"></param>
        ///// <returns></returns>
        //[HttpPost]
        //[Route("api/user/GetVerifycode")]
        //public Task<HttpResponseMessage> GetVerifycode(BindMobile bindMobile)
        //{
        //    ResponseBase<micromsg.BindOpMobileResponse> response = new ResponseBase<micromsg.BindOpMobileResponse>();

        //    var result = wechat.BindMobile(bindMobile.WxId, bindMobile.phone, bindMobile.code, 1);

        //    if (result == null || result.BaseResponse.Ret != (int)MMPro.MM.RetConst.MM_OK)
        //    {
        //        response.Success = false;
        //        response.Code = "501";
        //        response.Message = result?.BaseResponse?.ErrMsg?.String ?? "获取失败";
        //        return response.ToHttpResponseAsync();
        //    }
        //    else
        //    {
        //        response.Message = "获取成功";
        //        response.Data = result;
        //    }

        //    return response.ToHttpResponseAsync();
        //}

        ///// <summary>
        ///// 绑定手机号
        ///// </summary>
        ///// <param name="bindMobile"></param>
        ///// <returns></returns>
        //[HttpPost]
        //[Route("api/user/BindMobile")]
        //public Task<HttpResponseMessage> BindMobile(BindMobile bindMobile)
        //{
        //    ResponseBase<micromsg.BindOpMobileResponse> response = new ResponseBase<micromsg.BindOpMobileResponse>();

        //    var result = wechat.BindMobile(bindMobile.WxId, bindMobile.phone, bindMobile.code, 2);

        //    if (result == null || result.BaseResponse.Ret != (int)MMPro.MM.RetConst.MM_OK)
        //    {
        //        response.Success = false;
        //        response.Code = "501";
        //        response.Message = result?.BaseResponse?.ErrMsg?.String ?? "绑定失败";
        //        return response.ToHttpResponseAsync();
        //    }
        //    else
        //    {
        //        response.Message = "绑定成功";
        //        response.Data = result;
        //    }

        //    return response.ToHttpResponseAsync();
        //}

        ///// <summary>
        ///// 解绑手机号
        ///// </summary>
        ///// <param name="bindMobile"></param>
        ///// <returns></returns>
        //[HttpPost]
        //[Route("api/user/UnBindMobile")]
        //public Task<HttpResponseMessage> UnBindMobile(BindMobile bindMobile)
        //{
        //    ResponseBase<micromsg.BindOpMobileResponse> response = new ResponseBase<micromsg.BindOpMobileResponse>();

        //    var result = wechat.BindMobile(bindMobile.WxId, bindMobile.phone, bindMobile.code, 3);

        //    if (result == null || result.BaseResponse.Ret != (int)MMPro.MM.RetConst.MM_OK)
        //    {
        //        response.Success = false;
        //        response.Code = "501";
        //        response.Message = result?.BaseResponse?.ErrMsg?.String ?? "解绑失败";
        //        return response.ToHttpResponseAsync();
        //    }
        //    else
        //    {
        //        response.Message = "解绑成功";
        //        response.Data = result;
        //    }

        //    return response.ToHttpResponseAsync();
        //}

        /// <summary>
        /// 验证身份证
        /// </summary>
        /// <param name="verifyIdCard"></param>
        /// <returns></returns>
        [Route("api/user/VerifyIdCard")]
        public Task<HttpResponseMessage> VerifyIdCard(VerifyIdCard verifyIdCard)
        {
            ResponseBase<micromsg.VerifyPersonalInfoResp> response = new ResponseBase<micromsg.VerifyPersonalInfoResp>();
            var result = wechat.VerifyPersonalInfo(verifyIdCard.WxId, verifyIdCard.RealName, verifyIdCard.IdCardType, verifyIdCard.IDCardNumber);
            response.Data = result;
            return response.ToHttpResponseAsync();
        }
    }
}