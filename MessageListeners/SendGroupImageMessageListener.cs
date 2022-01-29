using org.apache.rocketmq.client.producer;
using org.apache.rocketmq.common.message;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Wechat.Task.App.MessageListeners.Models;
using Wechat.Task.App.MessageListeners.Models.Request.Message;
using Wechat.Util.Exceptions;
using Wechat.Util.Extensions;
using Wechat.Util.FileStore;
using Wechat.Util.Mq;

namespace Wechat.Task.App.MessageListeners
{

    public class SendGroupImageMessageListener : MessageListenerConcurrentlyBase<ImageMessage>
    {
        private DefaultMQProducer producer = RocketMqHelper.CreateDefaultMQProducer("WECHAT_SEND_GROUP_IMAGE_MESSAGE_WATCH_PG");
        protected override void Invoke(org.apache.rocketmq.common.message.MessageClientExt messageClientExt, ImageMessage obj)
        {
            ResponseBase<IList<MMPro.MM.UploadMsgImgResponse>> response = new ResponseBase<IList<MMPro.MM.UploadMsgImgResponse>>(obj.MqId);
            try
            {
                IList<MMPro.MM.UploadMsgImgResponse> list = new List<MMPro.MM.UploadMsgImgResponse>();
                byte[] imageBuffer = FileStorageHelper.DownloadToBuffer(obj.ObjectName);
                int time = 5000;
                foreach (var item in obj.ToWxIds)
                {
                    try
                    {
                        Thread.Sleep(time);
                        var result = wechat.SendImageMessage(obj.WxId, item, imageBuffer);
                        list.Add(result);
                    }
                    catch (ExpiredException)
                    {
                        break;
                    }
                    catch (Exception ex)
                    {
                        MMPro.MM.UploadMsgImgResponse ss = new MMPro.MM.UploadMsgImgResponse();
                        ss.baseResponse = new MMPro.MM.BaseResponse()
                        {
                            ret = MMPro.MM.RetConst.MMSNS_RET_SPAM
                        };
                        list.Add(ss);
                        Util.Log.Logger.GetLog(this.GetType()).Error(ex);
                    }
                }
                response.Data = list;

                var buffer = Encoding.UTF8.GetBytes(response.ToJson());
                Message message = new Message("WECHAT_SEND_GROUP_IMAGE_MESSAGE_WATCH_TOPIC", buffer);
                producer.SendMessage(message);
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = ex.Message;

                var buffer = Encoding.UTF8.GetBytes(response.ToJson());
                Message message = new Message("WECHAT_SEND_GROUP_IMAGE_MESSAGE_WATCH_TOPIC", buffer);
                producer.SendMessage(message);
            }
        }
    }
}
