using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using Wechat.Api.Abstracts;
using Wechat.Api.Extensions;
using Wechat.Api.Filters;
using Wechat.Api.Helper;
using Wechat.Api.Request.Common;
using Wechat.Api.Request.Friend;
using Wechat.Api.Request.FriendCircle;
using Wechat.Api.Response.FriendCircle;
using Wechat.Protocol;
using Wechat.Util.Exceptions;
using Wechat.Util.Extensions;
using Wechat.Util.FileStore;

namespace Wechat.Api.Controllers
{
    /// <summary>
    /// 朋友圈
    /// </summary>
    public class FriendCircleController : WebchatControllerBase
    {

        /// <summary>
        /// 获取特定人朋友圈
        /// </summary>
        /// <param name="friendCircle"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("api/FriendCircle/GetFriendCircleDetail")]
        public Task<HttpResponseMessage> GetFriendCircleDetail(FriendCircle friendCircle)
        {
            ResponseBase<FriendCircleResponse> response = new ResponseBase<FriendCircleResponse>();

            var result = wechat.SnsUserPage(friendCircle.FristPageMd5, friendCircle.WxId, friendCircle.ToWxId, friendCircle.Id);
            if (result == null || result.baseResponse.ret != (int)MMPro.MM.RetConst.MM_OK)
            {
                response.Success = false;
                response.Code = "402";
                response.Message = result.baseResponse.errMsg.@string ?? "获取失败";
            }
            else
            {
                response.Data = new FriendCircleResponse()
                {
                    FristPageMd5 = result.fristPageMd5,
                    ObjectList = result.objectList
                };
            }
            return response.ToHttpResponseAsync();
        }

        /// <summary>
        /// 获取自己朋友圈列表
        /// </summary>
        /// <param name="friendCircleList"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("api/FriendCircle/GetFriendCircleList")]
        public Task<HttpResponseMessage> GetFriendCircleList(FriendCircleList friendCircleList)
        {
            ResponseBase<FriendCircleResponse> response = new ResponseBase<FriendCircleResponse>();

            var result = wechat.SnsTimeLine(friendCircleList.WxId, friendCircleList.FristPageMd5, friendCircleList.Id);
            if (result == null || result.baseResponse.ret != (int)MMPro.MM.RetConst.MM_OK)
            {
                response.Success = false;
                response.Code = "402";
                response.Message = result.baseResponse.errMsg.@string ?? "获取失败";
            }
            else
            {
                response.Data = new FriendCircleResponse()
                {
                    FristPageMd5 = result.fristPageMd5,
                    ObjectList = result.objectList
                };
            }


            return response.ToHttpResponseAsync();
        }

        /// <summary>
        /// 操作朋友圈 1删除朋友圈2设为隐私3设为公开4删除评论5取消点赞
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [Route("api/FriendCircle/SetFriendCircle")]
        public Task<HttpResponseMessage> SetFriendCircle(SetFriendCircle setFriendCircle)
        {
            ResponseBase response = new ResponseBase();
            var result = wechat.GetSnsObjectOp(setFriendCircle.Id, setFriendCircle.WxId, setFriendCircle.Type);
            if (result == null || result.baseResponse.ret != (int)MMPro.MM.RetConst.MM_OK)
            {
                response.Success = false;
                response.Code = "402";
                response.Message = result.baseResponse.errMsg.@string ?? "操作失败";
            }
            else
            {
                response.Message = "操作成功";
            }


            return response.ToHttpResponseAsync();
        }

        /// <summary>
        /// 发送朋友圈
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [Route("api/FriendCircle/SendFriendCircle")]
        public Task<HttpResponseMessage> SendFriendCircle(SendFriendCircle sendFriendCircle)
        {

            ResponseBase<MMPro.MM.SnsObject> response = new ResponseBase<MMPro.MM.SnsObject>();
            string content = null;
            switch (sendFriendCircle.Type)
            {
                case 0: content = SendSnsConst.GetContentTemplate(sendFriendCircle.WxId, wechat, sendFriendCircle.Content, sendFriendCircle.Title, sendFriendCircle.ContentUrl, sendFriendCircle.Description); break;
                case 1: content = SendSnsConst.GetImageTemplate(sendFriendCircle.WxId, wechat, sendFriendCircle.Content, sendFriendCircle.MediaInfos, sendFriendCircle.Title, sendFriendCircle.ContentUrl, sendFriendCircle.Description); break;
                case 2: content = SendSnsConst.GetVideoTemplate(sendFriendCircle.WxId, wechat, sendFriendCircle.Content, sendFriendCircle.MediaInfos, sendFriendCircle.Title, sendFriendCircle.ContentUrl, sendFriendCircle.Description); break;
                case 3: content = SendSnsConst.GetLinkTemplate(sendFriendCircle.WxId, wechat, sendFriendCircle.Content, sendFriendCircle.MediaInfos, sendFriendCircle.Title, sendFriendCircle.ContentUrl, sendFriendCircle.Description); break;
                case 4: content = SendSnsConst.GetImageTemplate3(sendFriendCircle.WxId, wechat, sendFriendCircle.Content, sendFriendCircle.MediaInfos, sendFriendCircle.Title, sendFriendCircle.ContentUrl, sendFriendCircle.Description); break;
                case 5: content = SendSnsConst.GetImageTemplate4(sendFriendCircle.WxId, wechat, sendFriendCircle.Content, sendFriendCircle.MediaInfos, sendFriendCircle.Title, sendFriendCircle.ContentUrl, sendFriendCircle.Description); break;
                case 6: content = SendSnsConst.GetImageTemplate5(sendFriendCircle.WxId, wechat, sendFriendCircle.Content, sendFriendCircle.MediaInfos, sendFriendCircle.Title, sendFriendCircle.ContentUrl, sendFriendCircle.Description); break;
                case 7: content = sendFriendCircle.Content; break;

            }

            var result = wechat.SnsPost(sendFriendCircle.WxId, content, sendFriendCircle.BlackList, sendFriendCircle.WithUserList);
            int count = 5;
            int index = 0;
            while (result?.snsObject?.id == null || result?.snsObject?.id == 0)
            {
                index++;
                result = wechat.SnsPost(sendFriendCircle.WxId, content, sendFriendCircle.BlackList, sendFriendCircle.WithUserList);
                if (index > count)
                {
                    throw new Exception("发送失败");
                }
                Thread.Sleep(index * 1000);
            }

            if (result == null || result.baseResponse.ret != (int)MMPro.MM.RetConst.MM_OK)
            {
                response.Success = false;
                response.Code = "402";
                response.Message = result.baseResponse.errMsg.@string ?? "发送失败";
            }
            else
            {

                response.Message = "发送成功";
                response.Data = result.snsObject;
            }
            return response.ToHttpResponseAsync();
        }

        /// <summary>
        /// 修改背景图
        /// </summary>
        /// <param name="setBackgroundImage"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("api/FriendCircle/SetBackgroundImage")]
        public Task<HttpResponseMessage> SetBackgroundImage(SetBackgroundImage setBackgroundImage)
        {

            ResponseBase<MMPro.MM.SnsObject> response = new ResponseBase<MMPro.MM.SnsObject>();
            string content = $"<TimelineObject><id><![CDATA[0]]></id><username><![CDATA[{setBackgroundImage.WxId}]]></username><createTime><![CDATA[0]]></createTime><contentDescShowType>0</contentDescShowType><contentDescScene>0</contentDescScene><private><![CDATA[0]]></private><contentDesc></contentDesc><contentattr><![CDATA[0]]></contentattr><sourceUserName></sourceUserName><sourceNickName></sourceNickName><statisticsData></statisticsData><weappInfo><appUserName></appUserName><pagePath></pagePath></weappInfo><canvasInfoXml></canvasInfoXml><location poiClickableStatus=\"0\"  poiClassifyId=\"\"  poiScale=\"0\"  longitude=\"0.0\"  city=\"\"  poiName=\"\"  latitude=\"0.0\"  poiClassifyType=\"0\"  poiAddress=\"\" ></location><ContentObject><contentStyle><![CDATA[7]]></contentStyle><contentSubStyle><![CDATA[0]]></contentSubStyle><title></title><description></description><contentUrl></contentUrl><mediaList><media><id><![CDATA[0]]></id><type><![CDATA[2]]></type><title></title><description></description><private><![CDATA[0]]></private><url type=\"1\" ><![CDATA[{setBackgroundImage.Url}]]></url><thumb type=\"1\" ><![CDATA[{setBackgroundImage.Url}]]></thumb>";
 
            var result = wechat.SnsPost(setBackgroundImage.WxId, content, null,null);
            int count = 5;
            int index = 0;
            while (result?.snsObject?.id == null || result?.snsObject?.id == 0)
            {
                index++;
                result = wechat.SnsPost(setBackgroundImage.WxId, content, null, null);
                if (index > count)
                {
                    throw new Exception("发送失败");
                }
                Thread.Sleep(index * 1000);
            }

            if (result == null || result.baseResponse.ret != (int)MMPro.MM.RetConst.MM_OK)
            {
                response.Success = false;
                response.Code = "402";
                response.Message = result.baseResponse.errMsg.@string ?? "发送失败";
            }
            else
            {

                response.Message = "发送成功";
                response.Data = result.snsObject;
            }
            return response.ToHttpResponseAsync();
        }

        /// <summary>
        /// 同步朋友圈
        /// </summary>
        /// <param name="wxId">微信Id</param>
        /// <returns></returns>
        [HttpPost]
        [Route("api/FriendCircle/SyncFriendCircle/{wxId}")]
        public Task<HttpResponseMessage> SyncFriendCircle(string wxId)
        {

            ResponseBase response = new ResponseBase();
            var result = wechat.SnsSync(wxId);
            if (result == null || result.baseResponse.ret != (int)MMPro.MM.RetConst.MM_OK)
            {
                response.Success = false;
                response.Code = "402";
                response.Message = result.baseResponse.errMsg.@string ?? "同步失败";
            }
            else
            {
                response.Message = "同步成功";
            }

            return response.ToHttpResponseAsync();
        }

        /// <summary>
        /// 上传朋友圈图片
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [NoRequestLog]
        [Route("api/FriendCircle/SendFriendCircleImage")]
        public async Task<HttpResponseMessage> SendFriendCircleImage(SendFriendCircleImage sendFriendCircleImage)
        {
            ResponseBase<IList<micromsg.SnsUploadResponse>> response = new ResponseBase<IList<micromsg.SnsUploadResponse>>();

            IList<micromsg.SnsUploadResponse> list = new List<micromsg.SnsUploadResponse>();
            int count = 5;
            int index = 0;
            foreach (var item in sendFriendCircleImage.Base64s)
            {
                byte[] buffer = null;
                var arr = item.Split(',');
                if (arr.Count() == 2)
                {
                    buffer = Convert.FromBase64String(arr[1]);

                }
                else
                {
                    buffer = Convert.FromBase64String(item);
                }
                var result = wechat.SnsUpload(sendFriendCircleImage.WxId, new MemoryStream(buffer));
                while (result == null)
                {
                    index++;
                    result = wechat.SnsUpload(sendFriendCircleImage.WxId, new MemoryStream(buffer));
                    if (index > count)
                    {
                        throw new Exception("上传失败");
                    }
                    Thread.Sleep(index * 1000);
                }
                list.Add(result);
            }
            response.Data = list;
            response.Message = "上传成功";

            return await response.ToHttpResponseAsync();
        }

        ///// <summary>
        ///// 上传朋友圈视频
        ///// </summary>
        ///// <param name="sendFriendCircleVideo"></param>
        ///// <returns></returns>
        //[HttpPost]
        //[NoRequestLog]
        //[Route("api/FriendCircle/SendFriendCircleVideo")]
        //public async Task<HttpResponseMessage> SendFriendCircleVideo(SendFriendCircleVideo sendFriendCircleVideo)
        //{
        //    ResponseBase<IList<micromsg.SnsUploadResponse>> response = new ResponseBase<IList<micromsg.SnsUploadResponse>>();

        //    IList<micromsg.SnsUploadResponse> list = new List<micromsg.SnsUploadResponse>();
        //    int count = 5;
        //    int index = 0;
        //    foreach (var item in sendFriendCircleVideo.Base64s)
        //    {
        //        byte[] buffer = null;
        //        var arr = item.Split(',');
        //        if (arr.Count() == 2)
        //        {
        //            buffer = Convert.FromBase64String(arr[1]);

        //        }
        //        else
        //        {
        //            buffer = Convert.FromBase64String(item);
        //        }
        //        var result = wechat.SnsUpload(sendFriendCircleVideo.WxId, new MemoryStream(buffer),5);
        //        while (result == null)
        //        {
        //            index++;
        //            result = wechat.SnsUpload(sendFriendCircleVideo.WxId, new MemoryStream(buffer),5);
        //            if (index > count)
        //            {
        //                throw new Exception("上传失败");
        //            }
        //            Thread.Sleep(index * 1000);
        //        }
        //        list.Add(result);
        //    }
        //    response.Data = list;
        //    response.Message = "上传成功";

        //    return await response.ToHttpResponseAsync();
        //}

        /// <summary>
        /// 上传朋友圈图片(表单)
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [NoRequestLog]
        [Route("api/FriendCircle/SendFriendCircleImageForm")]
        public async Task<HttpResponseMessage> SendFriendCircleImageForm()
        {
            ResponseBase<IList<micromsg.SnsUploadResponse>> response = new ResponseBase<IList<micromsg.SnsUploadResponse>>();
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

            var wxId = HttpContext.Current.Request["WxId"];
            if (string.IsNullOrEmpty(wxId))
            {
                response.Success = false;
                response.Code = "400";
                response.Message = "WxId不能为空";
                return await response.ToHttpResponseAsync();
            }
            IList<micromsg.SnsUploadResponse> list = new List<micromsg.SnsUploadResponse>();
            for (int i = 0; i < fileCount; i++)
            {
                var file = HttpContext.Current.Request.Files[i];
                var result = wechat.SnsUpload(wxId, file.InputStream);
                if (result == null)
                {
                    throw new Exception("上传失败");
                }
                list.Add(result);
            }
            response.Data = list;
            response.Message = "上传成功";

            return await response.ToHttpResponseAsync();
        }


        ///// <summary>
        ///// 上传朋友圈视频(表单)
        ///// </summary>
        ///// <returns></returns>
        [HttpPost]
        [NoRequestLog]
        [Route("api/FriendCircle/SendFriendCircleVideoForm")]
        public async Task<HttpResponseMessage> SendFriendCircleVideoForm()
        {
            ResponseBase<IList<micromsg.SnsUploadResponse>> response = new ResponseBase<IList<micromsg.SnsUploadResponse>>();
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

            var wxId = HttpContext.Current.Request["WxId"];
            if (string.IsNullOrEmpty(wxId))
            {
                response.Success = false;
                response.Code = "400";
                response.Message = "WxId不能为空";
                return await response.ToHttpResponseAsync();
            }
            IList<micromsg.SnsUploadResponse> list = new List<micromsg.SnsUploadResponse>();
            for (int i = 0; i < fileCount; i++)
            {
                var file = HttpContext.Current.Request.Files[i];
                var result = wechat.SnsUpload(wxId, file.InputStream, 7);
                if (result == null)
                {
                    throw new Exception("上传失败");
                }
                list.Add(result);
            }
            response.Data = list;
            response.Message = "上传成功";

            return await response.ToHttpResponseAsync();
        }
        /// <summary>
        /// 发送评论 点赞
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [Route("api/FriendCircle/SendFriendCircleComment")]
        public async Task<HttpResponseMessage> SendFriendCircleComment(FriendCircleComment friendCircleComment)
        {
            ResponseBase<micromsg.SnsObject> response = new ResponseBase<micromsg.SnsObject>();
            var result = wechat.SnsComment(Convert.ToUInt64(friendCircleComment.Id), friendCircleComment.WxId, friendCircleComment.WxId, friendCircleComment.ReplyCommnetId, friendCircleComment.Content, (MMPro.MM.SnsObjectType)friendCircleComment.Type);
            if (result == null || result.BaseResponse.Ret != (int)MMPro.MM.RetConst.MM_OK)
            {
                response.Success = false;
                response.Code = "402";
                response.Message = result.BaseResponse.ErrMsg.String ?? "发送失败";
            }
            else
            {
                response.Data = result.SnsObject;
                response.Message = "发送成功";
            }

            return await response.ToHttpResponseAsync();
        }

        /// <summary>
        /// 设置朋友圈可见天数
        /// </summary>
        /// <param name="setFriendCircleDays"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("api/FriendCircle/SetFriendCircleDays")]
        public Task<HttpResponseMessage> SetFriendCircleDays(SetFriendCircleDays setFriendCircleDays)
        {
            ResponseBase response = new ResponseBase();

            MMPro.MM.SnsUserInfo snsUser = new MMPro.MM.SnsUserInfo()
            {
                snsBGImgID = setFriendCircleDays.SnsBGImgID,
                snsBGObjectID = setFriendCircleDays.SnsBGObjectID,
                snsFlag = 1,
                snsFlagEx = (uint)setFriendCircleDays.SnsFlagEx
            };

            var result = wechat.OpLog(setFriendCircleDays.WxId, 51, snsUser);
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
        /// 获取朋友圈视频
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [Route("api/FriendCircle/GetFriendCircleVideo")]
        public Task<HttpResponseMessage> GetFriendCircleVideo(GetFriendCircleVideo getFriendCircleVideo)
        {

            ResponseBase response = new ResponseBase();
            HttpHelper http = new HttpHelper();
            HttpItem item = new HttpItem()
            {
                URL = getFriendCircleVideo.Url,
                Method = "GET",
                PostDataType = PostDataType.Byte,
                UserAgent = "MicroMessenger Client",
                Accept = "*/*",
                ContentType = "application/octet-stream",
                ResultType = ResultType.Byte,
                ProxyIp = ""
            };

            HttpResult g = http.GetHtml(item);
            // Console.WriteLine(ret.ResultByte.ToString(16, 2));
            string enclen = g.Header.Get("X-enclen");
            int elen = int.Parse(enclen);
            var buffer = g.ResultByte;
            decryptData(buffer, (uint)elen, (ulong)getFriendCircleVideo.Key);



            if (!(buffer?.LongLength > 0))
            {
                response.Success = false;
                response.Code = "400";
                return response.ToHttpResponseAsync();
            }
            else
            {
                return buffer.ToHttpVideoResponseAsync();

            }

        }


        [DllImport(@"SDll.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern Int64 decryptData(byte[] srcData, uint srcDataLen, UInt64 key);

    }
}