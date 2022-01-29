using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using System.Xml;
using Wechat.Api.Abstracts;
using Wechat.Api.Extensions;
using Wechat.Api.Filters;
using Wechat.Api.Helper;
using Wechat.Api.Request;
using Wechat.Api.Request.Pay;
using Wechat.Util.Cache;
using Wechat.Util.Exceptions;
using Wechat.Util.Extensions;
using Wechat.Util.Mq;
using static MMPro.MM;

namespace Wechat.Api.Controllers
{
    /// <summary>
    /// 支付
    /// </summary>
    public class PayController : WebchatControllerBase
    {
        /// <summary>
        /// 获取银行卡信息
        /// </summary>
        /// <param name="wxId">微信Id</param>
        /// <returns></returns>
        [HttpPost]
        [Route("api/Pay/GetBandCardList/{wxId}")]
        public async Task<HttpResponseMessage> GetBandCardList(string wxId)
        {
            ResponseBase<MMPro.MM.TenPayResponse> response = new ResponseBase<MMPro.MM.TenPayResponse>();

            var result = wechat.TenPay(wxId, MMPro.MM.enMMTenPayCgiCmd.MMTENPAY_CGICMD_BIND_QUERY_NEW);
            if (result == null || result.baseResponse.ret != (int)MMPro.MM.RetConst.MM_OK)
            {
                response.Success = false;
                response.Code = "501";
                response.Message = result.baseResponse.errMsg.@string ?? "获取失败";
            }
            else
            {
                response.Data = result;
            }


            return await response.ToHttpResponseAsync();
        }

        /// <summary>
        /// 创建转账
        /// </summary>
        /// <param name="createPreTransfer"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("api/Pay/CreatePreTransfer")]
        public async Task<HttpResponseMessage> CreatePreTransfer(CreatePreTransfer createPreTransfer)
        {
            ResponseBase<MMPro.MM.TenPayResponse, string> response = new ResponseBase<MMPro.MM.TenPayResponse, string>();

            string tenpayUrl = $"delay_confirm_flag=0&desc={createPreTransfer.Name}&fee={(int)(createPreTransfer.Money * 100) }&fee_type=CNY&pay_scene=31&receiver_name={createPreTransfer.ToWxId}&scene=31&transfer_scene=2";
            var sign = Wechat.Protocol.Util.WCPaySignDES3Encode(tenpayUrl, Guid.NewGuid().ToString().Replace("-", "").ToUpper());
            tenpayUrl += $"&WCPaySign={sign}";
            var result = wechat.TenPay(createPreTransfer.WxId, MMPro.MM.enMMTenPayCgiCmd.MMTENPAY_CGICMD_GEN_PRE_TRANSFER, tenpayUrl);

            if (result == null || result.baseResponse.ret != (int)MMPro.MM.RetConst.MM_OK)
            {
                response.Success = false;
                response.Code = "501";
                response.Message = result.baseResponse.errMsg.@string ?? "创建失败";
            }
            else
            {
                response.Data = result;
                response.Result = sign;
            }


            return await response.ToHttpResponseAsync();
        }

        /// <summary>
        /// 确认转账
        /// </summary>
        /// <param name="confirmTransfer"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("api/Pay/ConfirmTransfer")]
        public async Task<HttpResponseMessage> ConfirmTransfer(ConfirmTransfer confirmTransfer)
        {
            ResponseBase<MMPro.MM.TenPayResponse, string> response = new ResponseBase<MMPro.MM.TenPayResponse, string>();

            string tenpayUrl = $"auto_deduct_flag=0&bank_type={confirmTransfer.BankType}&bind_serial={confirmTransfer.BindSerial}&busi_sms_flag=0&flag=3&passwd={confirmTransfer.PayPassword}&pay_scene=37&req_key={confirmTransfer.ReqKey}&use_touch=0";

            var sign = Wechat.Protocol.Util.WCPaySignDES3Encode(tenpayUrl, Guid.NewGuid().ToString().Replace("-", "").ToUpper());
            tenpayUrl += $"&WCPaySign={sign}";
            var result = wechat.TenPay(confirmTransfer.WxId, MMPro.MM.enMMTenPayCgiCmd.MMTENPAY_CGICMD_AUTHEN, tenpayUrl);

            if (result == null || result.baseResponse.ret != (int)MMPro.MM.RetConst.MM_OK)
            {
                response.Success = false;
                response.Code = "501";
                response.Message = result.baseResponse.errMsg.@string ?? "确认失败";
            }
            else
            {
                response.Data = result;
                response.Result = sign;
            }


            return await response.ToHttpResponseAsync();
        }


        /// <summary>
        /// 生成自定义支付二维码
        /// </summary>
        /// <param name="generatePayQCode"></param>
        /// <returns></returns>
        [HttpPost]
        [NoResponseLog]
        [Route("api/Pay/GeneratePayQCode")]
        public async Task<HttpResponseMessage> GeneratePayQCode(GeneratePayQCode generatePayQCode)
        {
            ResponseBase response = new ResponseBase();

            //string payloadJson = "{\"CgiCmd\":0,\"ReqKey\":\"" + ReqKey + "\",\"PassWord\":\"123456\"}";
            string tenpayUrl = $"delay_confirm_flag=0&desc={generatePayQCode.Name}&fee={(int)(generatePayQCode.Money * 100)}&fee_type=CNY&pay_scene=31&receiver_name={generatePayQCode.WxId}&scene=31&transfer_scene=2";
            var sign = Wechat.Protocol.Util.WCPaySignDES3Encode(tenpayUrl, Guid.NewGuid().ToString().Replace("-", "").ToUpper());
            tenpayUrl += $"&WCPaySign={sign}";
            var result = wechat.TenPay(generatePayQCode.WxId, MMPro.MM.enMMTenPayCgiCmd.MMTENPAY_CGICMD_GET_FIXED_AMOUNT_QRCODE, tenpayUrl);

            if (result == null || result.baseResponse.ret != (int)MMPro.MM.RetConst.MM_OK)
            {
                response.Success = false;
                response.Code = "501";
                response.Message = result.baseResponse.errMsg.@string ?? "生成失败";
            }
            else
            {
                var reqText = result.reqText.buffer.ToObj<GeneratePayQCodeReqTest>();
                if (reqText.retcode == 0)
                {
                    string url = reqText.pay_url;
                    var image = url.CreateQRCode();
                    return await image.ToHttpImageResponseAsync();
                }
                else
                {
                    response.Success = false;
                    response.Code = "501";
                    response.Message = reqText.retmsg ?? "生成失败";
                }

            }

            return await response.ToHttpResponseAsync();
        }

        /// <summary>
        /// 生成支付二维码
        /// </summary>
        /// <param name="wxId"></param>
        /// <returns></returns>
        [HttpPost]
        [NoResponseLog]
        [Route("api/Pay/GeneratePayQCode/{wxId}")]
        public async Task<HttpResponseMessage> GetQrCodeWithStream(string wxId)
        {
            ResponseBase response = new ResponseBase();
            var result = wechat.F2FQrcode(wxId);
            if (result != null && result.baseResponse.ret == MMPro.MM.RetConst.MM_OK)
            {
                string url = result.url;
                var image = url.CreateQRCode();
                return await image.ToHttpImageResponseAsync();
            }
            else
            {
                response.Success = false;
                response.Code = "501";
                response.Message = "获取二维码失败";
            }

            return await response.ToHttpResponseAsync();
        }

        /// <summary>
        /// 确认收款
        /// </summary>
        /// <param name="collectmoney"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("api/Pay/Collectmoney")]
        public async Task<HttpResponseMessage> TransferConfirm(Collectmoney collectmoney)
        {
            ResponseBase<MMPro.MM.TenPayResponse, string> response = new ResponseBase<MMPro.MM.TenPayResponse, string>();

            string tenpayUrl = $"invalid_time={collectmoney.InvalidTime}&op=confirm&total_fee=0&trans_id={collectmoney.Transferid}&transaction_id={collectmoney.TransactionId}&username={collectmoney.ToWxid}";
            var sign = Wechat.Protocol.Util.WCPaySignDES3Encode(tenpayUrl, Guid.NewGuid().ToString().Replace("-", "").ToUpper());
            tenpayUrl += $"&WCPaySign={sign}";
            var result = wechat.TenPay(collectmoney.WxId, MMPro.MM.enMMTenPayCgiCmd.MMTENPAY_CGICMD_TRANSFER_CONFIRM, tenpayUrl);

            if (result == null || result.baseResponse.ret != (int)MMPro.MM.RetConst.MM_OK)
            {
                response.Success = false;
                response.Code = "501";
                response.Message = result.baseResponse.errMsg.@string ?? "确认失败";
            }
            else
            {
                response.Data = result;
                response.Result = sign;
            }

            return await response.ToHttpResponseAsync();
        }

        /// <summary>
        /// 拆红包
        /// </summary>
        /// <param name="openRedEnvelopes"></param>
        /// <returns></returns>
        [HttpPost()]
        [Route("api/Pay/OpenRedEnvelopes")]
        public Task<HttpResponseMessage> OpenRedEnvelopes(OpenRedEnvelopesRequest openRedEnvelopes)
        {
            ResponseBase<string> response = new ResponseBase<string>();
            bool openSucc = false;
            string nativeUrl = openRedEnvelopes.NativeUrl;

            string timingIdentifier = string.Empty;
            {
                var urlParams = UrlHelper.ExtractQueryParams(nativeUrl);
                int channelId = Convert.ToInt32(urlParams["channelid"]);
                int msgType = Convert.ToInt32(urlParams["msgtype"]);
                string sendId = urlParams["sendid"];
                string sessionUserName = urlParams["sendusername"];

                string reqText = $"agreeDuty={1}&inWay={1}&channelId={channelId}&msgType={msgType}&nativeUrl={System.Web.HttpUtility.UrlEncode(nativeUrl)}&sendId={sendId}&sessionUserName={sessionUserName}";

                var receiveWxHB = wechat.HongBaoRequest(openRedEnvelopes.WxId, reqText, 5181, "/cgi-bin/mmpay-bin/receivewxhb");
                if (receiveWxHB?.BaseResponse.ret == 0 && receiveWxHB?.platRet == 0 && receiveWxHB?.errorType == 0)
                {
                    string retText = receiveWxHB.retText?.buffer;

                    JObject json = JObject.Parse(retText);
                    if (Convert.ToInt32(json["retcode"]) != 0)
                    {
                        throw new Exception(json["retmsg"].ToString());
                    }
                    timingIdentifier = Convert.ToString(json["timingIdentifier"]);
                }
            }


            if (!string.IsNullOrEmpty(timingIdentifier))
            {
                var urlParams = UrlHelper.ExtractQueryParams(nativeUrl);

                int channelId = Convert.ToInt32(urlParams["channelid"]);
                int msgType = Convert.ToInt32(urlParams["msgtype"]);
                string sendId = urlParams["sendid"];
                string sessionUserName = urlParams["sendusername"];

                string reqText = $"channelId={channelId}&msgType={msgType}&nativeUrl={System.Web.HttpUtility.UrlEncode(nativeUrl)}&sendId={sendId}&sessionUserName={sessionUserName}&timingIdentifier={timingIdentifier}";

                var openWxHB = wechat.HongBaoRequest(openRedEnvelopes.WxId, reqText, 1685, "/cgi-bin/mmpay-bin/openwxhb");
                if (openWxHB?.BaseResponse.ret == 0 && openWxHB?.platRet == 0 && openWxHB?.errorType == 0)
                {
                    response.Data = openWxHB.retText?.buffer;

                    openSucc = true;
                }
            }
            if (openSucc)
            {
                response.Message = "打开成功";
            }
            else
            {
                response.Success = false;
                response.Code = "501";
                response.Message = "打开失败";

            }

            return response.ToHttpResponseAsync();
        }

        /// <summary>
        /// 查看红包详情
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [Route("api/Pay/QueryRedEnvelopesDetail")]
        public Task<HttpResponseMessage> QueryRedEnvelopesDetail(OpenWxHB model)
        {
            ResponseBase<string> response = new ResponseBase<string>();
            try
            {
                
                string nativeUrl = model.NativeUrl;
                string timingIdentifier = string.Empty;
                NameValueCollection urlParams = UrlHelper.ExtractQueryParams(nativeUrl);
                int channelId = Convert.ToInt32(urlParams["channelid"]);
                int msgType = Convert.ToInt32(urlParams["msgtype"]);
                string sendId = urlParams["sendid"];
                string sessionUserName = urlParams["sendusername"];
                string reqText = string.Format("agreeDuty={0}&inWay={1}&channelId={2}&msgType={3}&nativeUrl={4}&sendId={5}&sessionUserName={6}", new object[]
                {
                    1,
                    1,
                    channelId,
                    msgType,
                    HttpUtility.UrlEncode(nativeUrl),
                    sendId,
                    sessionUserName
                });
                MMPro.MM.HongBaoResponse receiveWxHB = this.wechat.HongBaoRequest(model.WxId, reqText, 5181, "/cgi-bin/mmpay-bin/receivewxhb");
                bool flag = receiveWxHB != null && receiveWxHB.BaseResponse.ret == MMPro.MM.RetConst.MM_OK && receiveWxHB != null && receiveWxHB.platRet == 0 && receiveWxHB != null && receiveWxHB.errorType == 0;
                if (flag)
                {
                    MMPro.MM.SKBuiltinString_S retText2 = receiveWxHB.retText;
                    string retText = (retText2 != null) ? retText2.buffer : null;
                    response.Data = retText;
                }
            }
            catch
            {
            }
            bool flag2 = !string.IsNullOrEmpty(response.Data);
            if (flag2)
            {
                response.Message = "查看成功";
            }
            else
            {
                response.Success = false;
                response.Code = "501";
                response.Message = "查看失败";
            }
            return response.ToHttpResponseAsync();
        }
        /// <summary>
        /// 开启抢红包
        /// </summary>
        /// <param name="StartOpenRedEnvelopes"></param>
        /// <returns></returns>
        [HttpPost()]
        [Route("api/Pay/StartOpenRedEnvelopes")]
        public Task<HttpResponseMessage> StartOpenRedEnvelopes(StartOpenRedEnvelopesRequest StartOpenRedEnvelopes)
        {
            ResponseBase response = new ResponseBase();
            var cache = RedisCache.CreateInstance();
            string key = ConstCacheKey.GetHongbaoKey(StartOpenRedEnvelopes.WxId);

            cache.Add(key, StartOpenRedEnvelopes.WxId, DateTime.Now.AddMonths(1));
            Task.Run(() =>
            {
                while (true)
                {
                    try
                    {
                        var hongBaoCache = cache.Get(key);
                        if (!string.IsNullOrEmpty(hongBaoCache))
                        {
                            var syncInitRes = wechat.SyncInit(StartOpenRedEnvelopes.WxId);
                            var msgs = syncInitRes.AddMsgs?.Where(o => o.MsgType == 49)?.ToList();
                            if (msgs != null)
                            {
                                foreach (var msg in msgs)
                                {
                                    string msgXml = null;
                                    if (msg.FromUserName.String.EndsWith("@chatroom"))
                                    {
                                        int index = msg.Content.String.IndexOf(':');
                                        msgXml = msg.Content.String.Substring(index + 1);
                                    }
                                    else
                                    {
                                        msgXml = msg.Content.String;
                                    }
                                    if (!msgXml.Contains("nativeurl"))
                                    {
                                        continue;
                                    }
                                    XmlDocument xml = new XmlDocument();
                                    xml.LoadXml(msgXml);
                                    string nativeurl = xml.SelectSingleNode("//nativeurl").InnerText;
                                    OpenRedEnvelopesRequest request = new OpenRedEnvelopesRequest()
                                    {
                                        WxId = StartOpenRedEnvelopes.WxId,
                                        NativeUrl = nativeurl
                                    };
                                    OpenRedEnvelopes(request);
                                }
                            }
                        }
                        else
                        {
                            break;
                        }
                        Thread.Sleep(StartOpenRedEnvelopes.Second * 1000);
                    }
                    catch (Exception ex)
                    {
                        Util.Log.Logger.GetLog<PayController>().Error(ex);
                        break;
                    }
                }




            });

            return response.ToHttpResponseAsync();
        }



        /// <summary>
        /// 关闭抢红包
        /// </summary>
        /// <param name="wxId"></param>
        /// <returns></returns>
        [HttpPost()]
        [Route("api/Pay/StopOpenRedEnvelopes/{wxId}")]
        public Task<HttpResponseMessage> StopOpenRedEnvelopes(string wxId)
        {
            ResponseBase response = new ResponseBase();

            string key = ConstCacheKey.GetHongbaoKey(wxId);
            RedisCache.CreateInstance().Remove(key);
            return response.ToHttpResponseAsync();
        }

    }
}