using System;
using System.Collections.Generic;
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
using Wechat.Api.Helper;
using Wechat.Api.Request.Common;
using Wechat.Api.Request.Message;
using Wechat.Protocol;
using Wechat.Util.Exceptions;
using Wechat.Util.Extensions;
using Wechat.Util.FileStore;
using static MMPro.MM;

namespace Wechat.Api.Controllers
{
    /// <summary>
    /// 消息服务
    /// </summary>
    public class MessageController : WebchatControllerBase
    {
        /// <summary>
        /// 同步微信消息
        /// </summary>
        /// <param name="wxId">微信Id</param>
        /// <returns></returns>
        [HttpPost]
        [Route("api/Message/SyncMessage/{wxId}")]
        public Task<HttpResponseMessage> SyncMessage(string wxId)
        {
            ResponseBase<InitResponse> response = new ResponseBase<InitResponse>();
            var result = wechat.SyncInit(wxId);
            response.Data = result;
            return response.ToHttpResponseAsync();
        }
        /// <summary>
        /// 发送文本信息
        /// </summary>
        /// <param name="txtMessage"></param>
        /// <returns></returns>
        [HttpPost()]
        [Route("api/Message/SendTxtMessage")]
        public Task<HttpResponseMessage> SendTxtMessage(TxtMessage txtMessage)
        {
            ResponseBase<IList<MMPro.MM.NewSendMsgRespone.NewMsgResponeNew>> response = new ResponseBase<IList<MMPro.MM.NewSendMsgRespone.NewMsgResponeNew>>();
           

            IList<MMPro.MM.NewSendMsgRespone.NewMsgResponeNew> list = new List<MMPro.MM.NewSendMsgRespone.NewMsgResponeNew>();
            foreach (var item in txtMessage.ToWxIds)
            {
                var result = wechat.SendNewMsg(txtMessage.WxId, item, txtMessage.Content, 1, txtMessage.AtWxIds);
                list.Add(result?.List?.FirstOrDefault());
            }

            response.Data = list;

            return response.ToHttpResponseAsync();
        }

        /// <summary>
        /// 发送Emoji表情消息
        /// </summary>
        /// <param name="sendEmojiMessase"></param>
        /// <returns></returns>
        [HttpPost()]
        [Route("api/Message/SendEmojiMessage")]
        public Task<HttpResponseMessage> SendEmojiMessage(SendEmojiMessase sendEmojiMessase)
        {
            ResponseBase<IList<micromsg.UploadEmojiResponse>> response = new ResponseBase<IList<micromsg.UploadEmojiResponse>>();

            IList<micromsg.UploadEmojiResponse> list = new List<micromsg.UploadEmojiResponse>();
            //if (sendEmojiMessase.Type == 1)
            //{              
            //    sendEmojiMessase.Md5 = "da1c289d4e363f3ce1ff36538903b92f";
            //}
            //else if (sendEmojiMessase.Type == 2)
            //{
            //    sendEmojiMessase.Md5 = "9e3f303561566dc9342a3ea41e6552a6";
            //}
            foreach (var item in sendEmojiMessase.ToWxIds)
            {
                var result = wechat.EmojiUploadInfo(sendEmojiMessase.WxId, item, sendEmojiMessase.Md5, sendEmojiMessase.Type, sendEmojiMessase.Content);



                list.Add(result);
            }

            response.Data = list;

            return response.ToHttpResponseAsync();
        }

        /// <summary>
        /// 发送声音消息
        /// </summary> 
        /// <returns></returns>
        [HttpPost()]
        [NoRequestLog]
        [Route("api/Message/SendVoiceMessage")]
        public async Task<HttpResponseMessage> SendVoiceMessage(VoiceMessage voiceMessage)
        {
            ResponseBase<IList<MMPro.MM.UploadVoiceResponse>> response = new ResponseBase<IList<MMPro.MM.UploadVoiceResponse>>();

            IList<MMPro.MM.UploadVoiceResponse> list = new List<MMPro.MM.UploadVoiceResponse>();
            var buffer = Convert.FromBase64String(voiceMessage.Base64.Replace("data:audio/amr;base64,", ""));
            foreach (var item in voiceMessage.ToWxIds)
            {
                var result = wechat.SendVoiceMessage(voiceMessage.WxId, item, buffer, voiceMessage.FileName.GetVoiceType(), voiceMessage.VoiceSecond * 100);
                list.Add(result);
            }
            response.Data = list;

            return await response.ToHttpResponseAsync();
        }

        /// <summary>
        /// 发送图片消息
        /// </summary> 
        /// <returns></returns>
        [HttpPost()]
        [NoRequestLog]
        [Route("api/Message/SendImageMessage")]
        public async Task<HttpResponseMessage> SendImageMessage(ImageMessage imageMessage)
        {
            ResponseBase<IList<MMPro.MM.UploadMsgImgResponse>> response = new ResponseBase<IList<MMPro.MM.UploadMsgImgResponse>>();

            IList<MMPro.MM.UploadMsgImgResponse> list = new List<MMPro.MM.UploadMsgImgResponse>();
            byte[] buffer = null;
            var arr = imageMessage.Base64.Split(',');
            if (arr.Count() == 2)
            {
                buffer = Convert.FromBase64String(arr[1]);

            }
            else
            {
                buffer = Convert.FromBase64String(imageMessage.Base64);
            }

            foreach (var item in imageMessage.ToWxIds)
            {
                var result = wechat.SendImageMessage(imageMessage.WxId, item, buffer);
                list.Add(result);
            }
            response.Data = list;

            return await response.ToHttpResponseAsync();
        }



        /// <summary>
        /// 发送视频消息
        /// </summary>       
        /// <returns></returns>
        [HttpPost()]
        [NoRequestLog]
        [Route("api/Message/SendVideoMessage")]
        public async Task<HttpResponseMessage> SendVideoMessage(VideoMessage videoMessage)
        {
            ResponseBase<IList<MMPro.MM.UploadVideoResponse>> response = new ResponseBase<IList<MMPro.MM.UploadVideoResponse>>();

            IList<MMPro.MM.UploadVideoResponse> list = new List<MMPro.MM.UploadVideoResponse>();
            byte[] buffer = null;
            var arr = videoMessage.Base64.Split(',');
            if (arr.Count() == 2)
            {
                buffer = Convert.FromBase64String(arr[1]);

            }
            else
            {
                buffer = Convert.FromBase64String(videoMessage.Base64);
            }
            byte[] imageBuffer = null;
            var arrimageBuffer = videoMessage.ImageBase64.Split(',');
            if (arrimageBuffer.Count() == 2)
            {
                imageBuffer = Convert.FromBase64String(arrimageBuffer[1]);

            }
            else
            {
                imageBuffer = Convert.FromBase64String(videoMessage.ImageBase64);
            }
            foreach (var item in videoMessage.ToWxIds)
            {
                var result = wechat.SendVideoMessage(videoMessage.WxId, item, videoMessage.PlayLength, buffer, imageBuffer);
                list.Add(result);
            }
            response.Data = list;

            return await response.ToHttpResponseAsync();
        }
        /// <summary>
        /// 发送CDN图片
        /// </summary> 
        /// <returns></returns>
        [HttpPost()]
        [Route("api/Message/SendCDNImageMessage")]
        public async Task<HttpResponseMessage> SendCDNImageMessage(CDNImageMessage imageMessage)
        {
            ResponseBase<IList<MMPro.MM.UploadMsgImgResponse>> response = new ResponseBase<IList<MMPro.MM.UploadMsgImgResponse>>();

            IList<MMPro.MM.UploadMsgImgResponse> list = new List<MMPro.MM.UploadMsgImgResponse>();

            foreach (var item in imageMessage.ToWxIds)
            {
                var result = wechat.SendMsgImgCDN(imageMessage.WxId, item, imageMessage.AESKey, imageMessage.Length, imageMessage.CDNThumbLength, imageMessage.Url);
                list.Add(result);
            }
            response.Data = list;

            return await response.ToHttpResponseAsync();
        }

        /// <summary>
        /// 发送CDN高清图片
        /// </summary> 
        /// <returns></returns>
        [HttpPost()]
        [Route("api/Message/SendCDNBigImageMessage")]
        public async Task<HttpResponseMessage> SendCDNBigImageMessage(CDNBigImageMessage imageMessage)
        {
            ResponseBase<IList<MMPro.MM.UploadMsgImgResponse>> response = new ResponseBase<IList<MMPro.MM.UploadMsgImgResponse>>();

            IList<MMPro.MM.UploadMsgImgResponse> list = new List<MMPro.MM.UploadMsgImgResponse>();

            foreach (var item in imageMessage.ToWxIds)
            {
                var result = wechat.SendMsgBigImgCDN(imageMessage.WxId, item, imageMessage.AESKey, imageMessage.Length, imageMessage.HDLength, imageMessage.CDNThumbLength, imageMessage.Url);
                list.Add(result);
            }
            response.Data = list;

            return await response.ToHttpResponseAsync();
        }
        /// <summary>
        /// 发送CDN视频
        /// </summary> 
        /// <returns></returns>
        [HttpPost()]
        [Route("api/Message/SendCDNVideoMessage")]
        public async Task<HttpResponseMessage> SendCDNVideoMessage(CDNVideoMessage videoMessage)
        {
            ResponseBase<IList<MMPro.MM.UploadVideoResponse>> response = new ResponseBase<IList<MMPro.MM.UploadVideoResponse>>();

            IList<MMPro.MM.UploadVideoResponse> list = new List<MMPro.MM.UploadVideoResponse>();

            foreach (var item in videoMessage.ToWxIds)
            {
                var result = wechat.SendCDNVideoMessage(videoMessage.WxId, item, videoMessage.AESKey, videoMessage.PlayLength, videoMessage.Length, videoMessage.CDNVideoUrl, videoMessage.CDNThumbLength);
                list.Add(result);
            }
            response.Data = list;

            return await response.ToHttpResponseAsync();
        }


        /// <summary>
        /// 发送声音消息(表单)
        /// </summary>
        /// <returns></returns>
        [HttpPost()]
        [NoRequestLog]
        [Route("api/Message/SendVoiceMessageForm")]
        public async Task<HttpResponseMessage> SendVoiceMessageForm()
        {
            ResponseBase<MMPro.MM.UploadVoiceResponse> response = new ResponseBase<MMPro.MM.UploadVoiceResponse>();
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
            if (file.FileName.IsVoice())
            {
                response.Success = false;
                response.Code = "400";
                response.Message = "请上传声音文件";
                return await response.ToHttpResponseAsync();
            }
            var wxId = HttpContext.Current.Request["WxId"];
            var toWxId = HttpContext.Current.Request["ToWxId"];
            var voiceSecond = HttpContext.Current.Request["VoiceSecond"];
            if (string.IsNullOrEmpty(wxId))
            {
                response.Success = false;
                response.Code = "400";
                response.Message = "WxId不能为空";
                return await response.ToHttpResponseAsync();
            }
            if (string.IsNullOrEmpty(toWxId))
            {
                response.Success = false;
                response.Code = "400";
                response.Message = "ToWxId不能为空";
                return await response.ToHttpResponseAsync();
            }
            if (string.IsNullOrEmpty(voiceSecond))
            {
                response.Success = false;
                response.Code = "400";
                response.Message = "VoiceSecond不能为空";
                return await response.ToHttpResponseAsync();
            }

            byte[] buffer = await file.InputStream.ToBufferAsync();

            var result = wechat.SendVoiceMessage(wxId, toWxId, buffer, file.FileName.GetVoiceType(), Convert.ToInt32(string.IsNullOrEmpty(voiceSecond) ? "1" : voiceSecond) * 100);


            if (result == null || result.baseResponse.ret != (int)MMPro.MM.RetConst.MM_OK)
            {
                response.Success = false;
                response.Code = "501";
                response.Message = result.baseResponse.errMsg.@string ?? "发送失败";
            }
            else
            {
                response.Data = result;
            }

            return await response.ToHttpResponseAsync();
        }

        /// <summary>
        /// 发送图片消息(表单)
        /// </summary> 
        /// <returns></returns>
        [HttpPost()]
        [NoRequestLog]
        [Route("api/Message/SendImageMessageForm")]
        public async Task<HttpResponseMessage> SendImageMessageForm()
        {
            ResponseBase<MMPro.MM.UploadMsgImgResponse> response = new ResponseBase<MMPro.MM.UploadMsgImgResponse>();
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
            }
            var file = HttpContext.Current.Request.Files[0];
            if (file.FileName.IsImage())
            {
                response.Success = false;
                response.Code = "400";
                response.Message = "请上传图片文件";
                return await response.ToHttpResponseAsync();
            }
            var wxId = HttpContext.Current.Request["WxId"];
            var toWxId = HttpContext.Current.Request["ToWxId"];
            if (string.IsNullOrEmpty(wxId))
            {
                response.Success = false;
                response.Code = "400";
                response.Message = "WxId不能为空";
                return await response.ToHttpResponseAsync();
            }
            if (string.IsNullOrEmpty(toWxId))
            {
                response.Success = false;
                response.Code = "400";
                response.Message = "ToWxId不能为空";
                return await response.ToHttpResponseAsync();
            }

            byte[] buffer = await file.InputStream.ToBufferAsync();

            var result = wechat.SendImageMessage(wxId, toWxId, buffer);
            if (result == null || result.baseResponse.ret != (int)MMPro.MM.RetConst.MM_OK)
            {
                response.Success = false;
                response.Code = "501";
                response.Message = result.baseResponse.errMsg.@string;
            }
            else
            {
                response.Data = result;
            }

            return await response.ToHttpResponseAsync();
        }



        /// <summary>
        /// 发送视频消息(表单)
        /// </summary>       
        /// <returns></returns>
        [HttpPost()]
        [NoRequestLog]
        [Route("api/Message/SendVideoMessageForm")]
        public async Task<HttpResponseMessage> SendVideoMessageForm()
        {

            ResponseBase<MMPro.MM.UploadVideoResponse> response = new ResponseBase<MMPro.MM.UploadVideoResponse>();
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
            }
            var file = HttpContext.Current.Request.Files.Get("videofile");
            if (file.FileName.IsVideo())
            {
                response.Success = false;
                response.Code = "400";
                response.Message = "请上传视频文件";
                return await response.ToHttpResponseAsync();
            }
            var imageFfile = HttpContext.Current.Request.Files.Get("imagefile");
            if (file.FileName.IsImage())
            {
                response.Success = false;
                response.Code = "400";
                response.Message = "请上传封面文件";
                return await response.ToHttpResponseAsync();
            }
            var wxId = HttpContext.Current.Request["WxId"];
            var toWxId = HttpContext.Current.Request["ToWxId"];
            var playLength = HttpContext.Current.Request["PlayLength"];
            if (!(int.TryParse(playLength, out int playLengthInt)))
            {
                response.Success = false;
                response.Code = "400";
                response.Message = "PlayLength格式不正确";
                return await response.ToHttpResponseAsync();
            }
            if (string.IsNullOrEmpty(wxId))
            {
                response.Success = false;
                response.Code = "400";
                response.Message = "WxId不能为空";
                return await response.ToHttpResponseAsync();
            }
            if (string.IsNullOrEmpty(toWxId))
            {
                response.Success = false;
                response.Code = "400";
                response.Message = "ToWxId不能为空";
                return await response.ToHttpResponseAsync();
            }

            byte[] buffer = await file.InputStream.ToBufferAsync();
            byte[] imageBuffer = await imageFfile.InputStream.ToBufferAsync();
            var result = wechat.SendVideoMessage(wxId, toWxId, playLengthInt, buffer, imageBuffer);
            if (result == null || result.baseResponse.ret != (int)MMPro.MM.RetConst.MM_OK)
            {
                response.Success = false;
                response.Code = "501";
                response.Message = result.baseResponse.errMsg.@string ?? "发送失败";
            }
            else
            {
                response.Data = result;
            }

            return await response.ToHttpResponseAsync();
        }


        /// <summary>
        /// 发送App消息
        /// </summary>
        /// <returns></returns>
        [HttpPost()]
        [Route("api/Message/SendAppMessage")]
        public async Task<HttpResponseMessage> SendAppMessage(AppMessage appMessage)
        {
            ResponseBase<IList<micromsg.SendAppMsgResponse>> response = new ResponseBase<IList<micromsg.SendAppMsgResponse>>();

            IList<micromsg.SendAppMsgResponse> list = new List<micromsg.SendAppMsgResponse>();
            string dataUrl = string.IsNullOrEmpty(appMessage.DataUrl) ? appMessage.Url : appMessage.DataUrl;
            string appMessageFormat = $"<appmsg appid=\"{appMessage.AppId}\" sdkver=\"0\"><title>{appMessage.Title}</title><des>{appMessage.Desc}</des><type>{appMessage.Type}</type><showtype>0</showtype><soundtype>0</soundtype><contentattr>0</contentattr><url>{appMessage.Url}</url><lowurl>{appMessage.Url}</lowurl><dataurl>{dataUrl}</dataurl><lowdataurl>{dataUrl}</lowdataurl> <thumburl>{appMessage.ThumbUrl}</thumburl></appmsg>";
            foreach (var item in appMessage.ToWxIds)
            {
                var result = wechat.SendAppMsg(appMessageFormat, item, appMessage.WxId);
                list.Add(result);
            }
            response.Data = list;

            return await response.ToHttpResponseAsync();
        }



        /// <summary>
        /// 发送分享消息
        /// </summary>
        /// <param name="appMessage"></param>
        /// <returns></returns>
        [HttpPost()]
        [Route("api/Message/SendShareMessage")]
        public async Task<HttpResponseMessage> SendShareMessage(ShareMessage appMessage)
        {
            ResponseBase<IList<micromsg.SendAppMsgResponse>> response = new ResponseBase<IList<micromsg.SendAppMsgResponse>>();

            IList<micromsg.SendAppMsgResponse> list = new List<micromsg.SendAppMsgResponse>();
            string dataUrl = string.IsNullOrEmpty(appMessage.DataUrl) ? appMessage.Url : appMessage.DataUrl;
            string appMessageFormat = $"<appmsg  sdkver=\"0\"><title>{appMessage.Title}</title><des>{appMessage.Desc}</des><type>{appMessage.Type}</type><showtype>0</showtype><soundtype>0</soundtype><contentattr>0</contentattr><url>{appMessage.Url}</url><lowurl>{appMessage.Url}</lowurl><dataurl>{dataUrl}</dataurl><lowdataurl>{dataUrl}</lowdataurl> <thumburl>{appMessage.ThumbUrl}</thumburl></appmsg>";
            foreach (var item in appMessage.ToWxIds)
            {
                var result = wechat.SendAppMsg(appMessageFormat, item, appMessage.WxId);
                list.Add(result);
            }
            response.Data = list;

            return await response.ToHttpResponseAsync();
        }



        /// <summary>
        /// 发送卡片消息
        /// </summary>
        /// <returns></returns>
        [HttpPost()]
        [Route("api/Message/SendCardMessage")]
        public async Task<HttpResponseMessage> SendCardMessage(CardMessage cardMessage)
        {
            ResponseBase<IList<MMPro.MM.NewSendMsgRespone>> response = new ResponseBase<IList<MMPro.MM.NewSendMsgRespone>>();

            IList<MMPro.MM.NewSendMsgRespone> list = new List<MMPro.MM.NewSendMsgRespone>();
            //cardMessage.CardNickName = string.IsNullOrEmpty(cardMessage.CardNickName) ? cardMessage.CardWxId : cardMessage.CardNickName;
            string appMessageFormat = $"<?xml version=\"1.0\"?>\n<msg bigheadimgurl=\"\" smallheadimgurl=\"\" username=\"{cardMessage.CardWxId}\" nickname=\"{cardMessage.CardNickName}\" fullpy=\"\" shortpy=\"\" alias=\"{cardMessage.CardAlias}\" imagestatus=\"0\" scene=\"17\" province=\"\" city=\"\" sign=\"\" sex=\"2\" certflag=\"0\" certinfo=\"\" brandIconUrl=\"\" brandHomeUrl=\"\" brandSubscriptConfigUrl=\"\" brandFlags=\"0\" regionCode=\"CN\" />\n";
            foreach (var item in cardMessage.ToWxIds)
            {
                var result = wechat.SendNewMsg(cardMessage.WxId, item, appMessageFormat, 42);
                list.Add(result);
            }
            response.Data = list;

            return await response.ToHttpResponseAsync();
        }


        /// <summary>
        /// 发送位置信息
        /// </summary>
        /// <param name="cardMessage"></param>
        /// <returns></returns>
        [HttpPost()]
        [Route("api/Message/SendLocationMessage")]
        public async Task<HttpResponseMessage> SendLocationMessage(LocationMessage cardMessage)
        {
            ResponseBase<IList<MMPro.MM.NewSendMsgRespone>> response = new ResponseBase<IList<MMPro.MM.NewSendMsgRespone>>();

            IList<MMPro.MM.NewSendMsgRespone> list = new List<MMPro.MM.NewSendMsgRespone>();
            string appMessageFormat = $"<?xml version=\"1.0\"?>\n<msg>\n\t<location x=\"{cardMessage.Latitude}\" y=\"{cardMessage.Longitude}\" scale=\"16\" label=\"{cardMessage.Name}\" maptype=\"0\" poiname=\"[位置]{cardMessage.Name}\" poiid=\"\" />\n</msg>";
            foreach (var item in cardMessage.ToWxIds)
            {
                var result = wechat.SendNewMsg(cardMessage.WxId, item, appMessageFormat, 48);
                list.Add(result);
            }
            response.Data = list;

            return await response.ToHttpResponseAsync();
        }


        /// <summary>
        /// 发送文件消息 
        /// </summary>
        /// <param name="mediaMessage"></param>
        /// <returns></returns>
        [HttpPost()]
        [Route("api/Message/SendMediaMessage")]
        public async Task<HttpResponseMessage> SendMediaMessage(MediaMessage mediaMessage)
        {
            ResponseBase<IList<micromsg.SendAppMsgResponse>> response = new ResponseBase<IList<micromsg.SendAppMsgResponse>>();

            IList<micromsg.SendAppMsgResponse> list = new List<micromsg.SendAppMsgResponse>();

            string appMessageFormat = $"<?xml version=\"1.0\"?>\n<appmsg appid='' sdkver=''><title>{mediaMessage.Title}</title><des></des><action></action><type>6</type><content></content><url></url><lowurl></lowurl><appattach><totallen>{mediaMessage.Length}</totallen><attachid>{mediaMessage.AttachId}</attachid><fileext>{mediaMessage.FileExt}</fileext></appattach><extinfo></extinfo></appmsg>";
            foreach (var item in mediaMessage.ToWxIds)
            {
                var result = wechat.SendAppMsg(appMessageFormat, item, mediaMessage.WxId, 6);
                list.Add(result);
            }
            response.Data = list;

            return await response.ToHttpResponseAsync();
        }


        /// <summary>
        /// 发送xml消息
        /// </summary>
        /// <param name="xmlMessage"></param>
        /// <returns></returns>
        [HttpPost()]
        [Route("api/Message/SendXmlMessage")]
        public async Task<HttpResponseMessage> SendXmlMessage(XmlMessage xmlMessage)
        {
            ResponseBase<IList<micromsg.SendAppMsgResponse>> response = new ResponseBase<IList<micromsg.SendAppMsgResponse>>();

            IList<micromsg.SendAppMsgResponse> list = new List<micromsg.SendAppMsgResponse>();
            foreach (var item in xmlMessage.ToWxIds)
            {
                var result = wechat.SendAppMsg(xmlMessage.Xml, item, xmlMessage.WxId);
                list.Add(result);
            }
            response.Data = list;

            return await response.ToHttpResponseAsync();
        }

        /// <summary>
        /// 上传文件
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [NoRequestLog]
        [Route("api/Message/UploadFile")]
        public async Task<HttpResponseMessage> UploadFile(UploadFile uploadFile)
        {
            ResponseBase<micromsg.UploadAppAttachResponse> response = new ResponseBase<micromsg.UploadAppAttachResponse>();
            var buffer = Convert.FromBase64String(uploadFile.Base64);
            var result = wechat.UploadFile(uploadFile.WxId, buffer, uploadFile.FileType);
            response.Data = result;
            return await response.ToHttpResponseAsync();
        }


        /// <summary>
        /// 上传文件 (表单)
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [NoRequestLog]
        [Route("api/Message/UploadFileForm")]
        public async Task<HttpResponseMessage> UploadFileForm()
        {
            ResponseBase<micromsg.UploadAppAttachResponse> response = new ResponseBase<micromsg.UploadAppAttachResponse>();
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

            var fileType = HttpContext.Current.Request["FileType"];
            if (string.IsNullOrEmpty(fileType))
            {
                response.Success = false;
                response.Code = "400";
                response.Message = "FileType不能为空";
                return await response.ToHttpResponseAsync();
            }
            if (!int.TryParse(fileType, out int fileTypeInt))
            {
                response.Success = false;
                response.Code = "400";
                response.Message = "FileType格式错误";
                return await response.ToHttpResponseAsync();
            }
            var result = wechat.UploadFile(wxId, file.InputStream.ToBuffer(), fileTypeInt);
            response.Data = result;
            return await response.ToHttpResponseAsync();
        }


        /// <summary>
        /// 获取大图片
        /// </summary>   
        /// <param name="bigImage"></param>
        /// <returns></returns>
        [HttpPost]
        [NoResponseLog]
        [Route("api/Message/GetBigImage")]
        public Task<HttpResponseMessage> GetBigImage(BigImage bigImage)
        {
            ResponseBase response = new ResponseBase();

            var buffer = wechat.GetMsgBigImg(bigImage.LongDataLength, bigImage.MsgId, bigImage.WxId, bigImage.ToWxId, 0, (int)bigImage.LongDataLength, (uint)bigImage.CompressType);
            if (buffer != null)
            {
                //return buffer.ToHttpImageResponseAsync();
                response.Message = Convert.ToBase64String(buffer);
            }
            else
            {
                response.Success = false;
                response.Code = "402";
                response.Message = "图片未找到";
            }


            return response.ToHttpResponseAsync();
        }
        /// <summary>
        /// 获取视频
        /// </summary>
        /// <param name="bigVideo"></param>
        /// <returns></returns>
        [HttpPost]
        [NoResponseLog]
        [Route("api/Message/GetBigVideo")]
        public Task<HttpResponseMessage> GetBigVideo(BigVideo bigVideo)
        {
            ResponseBase response = new ResponseBase();

            var buffer = wechat.GetVideo(bigVideo.WxId, bigVideo.ToWxId, bigVideo.MsgId, bigVideo.LongDataLength, (int)bigVideo.LongDataLength, 0, (uint)bigVideo.CompressType);
            if (buffer != null)
            {
                return buffer.ToHttpVideoResponseAsync();
            }
            else
            {
                response.Success = false;
                response.Code = "402";
                response.Message = "视频未找到";
            }
            return response.ToHttpResponseAsync();
        }
        /// <summary>
        /// 下载文件
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [Route("api/Message/DownloadFile")]
        public async Task<HttpResponseMessage> DownloadFile(DownloadFile DownloadFile)
        {
            ResponseBase<micromsg.DownloadAppAttachResponse> response = new ResponseBase<micromsg.DownloadAppAttachResponse>();

            var buffer = wechat.DownloadFile(DownloadFile.WxId, DownloadFile.AppId, DownloadFile.MediaId, DownloadFile.ToWxId, DownloadFile.TotalLen);
            if (buffer != null)
            {
                response.Data = buffer;
            }
            else
            {
                response.Success = false;
                response.Code = "402";
                response.Message = "文件未找到";
            }

            return await response.ToHttpResponseAsync();
        }
        /// <summary>
        /// 撤回消息
        /// </summary>
        /// <param name="revokeMessage"></param>
        /// <returns></returns>
        [HttpPost()]
        [Route("api/Message/RevokeMessage")]
        public Task<HttpResponseMessage> RevokeMessage(RevokeMessage revokeMessage)
        {
            ResponseBase<micromsg.RevokeMsgResponse> response = new ResponseBase<micromsg.RevokeMsgResponse>();

            var result = wechat.RevokeMsg(revokeMessage.WxId, revokeMessage.ToWxId, revokeMessage.MsgId, revokeMessage.NewMsgId);
            response.Data = result;

            return response.ToHttpResponseAsync();
        }


        ///// <summary>
        ///// 视频转发
        ///// </summary>
        ///// <param name="forwardVideo"></param>
        ///// <returns></returns>
        //[HttpPost()]
        //[Route("api/Message/ForwardVideo")]
        //public async Task<HttpResponseMessage> SendForwardVideo(ForwardVideo forwardVideo)
        //{
        //    ResponseBase<IList<MMPro.MM.UploadVideoResponse>> response = new ResponseBase<IList<MMPro.MM.UploadVideoResponse>>();
        //    IList<MMPro.MM.UploadVideoResponse> list = new List<MMPro.MM.UploadVideoResponse>();
        //    foreach (var item in forwardVideo.ToWxIds)
        //    {
        //        var result = wechat.ForwardVideo(forwardVideo.WxId, item, forwardVideo.Xml);
        //        list.Add(result);
        //    }
        //    response.Data = list;

        //    return await response.ToHttpResponseAsync();
        //}
        ///// <summary>
        ///// 图片转发
        ///// </summary>
        ///// <param name="forwardImg"></param>
        ///// <returns></returns>
        //[HttpPost()]
        //[Route("api/Message/ForwardImg")]
        //public async Task<HttpResponseMessage> SendForwardImg(ForwardImg forwardImg)
        //{
        //    ResponseBase<IList<MMPro.MM.UploadMsgImgResponse>> response = new ResponseBase<IList<MMPro.MM.UploadMsgImgResponse>>();

        //    IList<MMPro.MM.UploadMsgImgResponse> list = new List<MMPro.MM.UploadMsgImgResponse>();

        //    foreach (var item in forwardImg.ToWxIds)
        //    {
        //        var result = wechat.ForwardImg(forwardImg.WxId, item, forwardImg.Xml);
        //        list.Add(result);
        //    }
        //    response.Data = list;

        //    return await response.ToHttpResponseAsync();
        //}

        ///// <summary>
        ///// 分享转发
        ///// </summary>
        ///// <returns></returns>
        ///// [HttpPost()]
        //[Route("api/Message/ForwardShare")]
        //public async Task<HttpResponseMessage> SendForwardShare(ForwardShare forwardShare)
        //{
        //    ResponseBase<IList<micromsg.SendAppMsgResponse>> response = new ResponseBase<IList<micromsg.SendAppMsgResponse>>();

        //    IList<micromsg.SendAppMsgResponse> list = new List<micromsg.SendAppMsgResponse>();

        //    string appMessageFormat = $"{forwardShare.Xml}";
        //    foreach (var item in forwardShare.ToWxIds)
        //    {
        //        var result = wechat.SendAppMsg(appMessageFormat, item, forwardShare.WxId, forwardShare.Type);
        //        list.Add(result);
        //    }
        //    response.Data = list;

        //    return await response.ToHttpResponseAsync();
        //}

    }
}