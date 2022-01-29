using org.apache.rocketmq.client.producer;
using org.apache.rocketmq.common.message;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wechat.Task.App.Extensions;
using Wechat.Task.App.MessageListeners.Models;
using Wechat.Task.App.MessageListeners.Models.Request.Message;
using Wechat.Util.Extensions;
using Wechat.Util.FileStore;
using Wechat.Util.Mq;

namespace Wechat.Task.App.MessageListeners
{
    /// <summary>
    /// 发送视频消息
    /// </summary>
    public class SendVideoMessageListener : MessageListenerConcurrentlyBase<VideoMessage>
    {
        private DefaultMQProducer producer = RocketMqHelper.CreateDefaultMQProducer("WECHAT_SEND_VIDEO_MESSAGE_WATCH_PG");
        protected override void Invoke(org.apache.rocketmq.common.message.MessageClientExt messageClientExt,VideoMessage obj)
        {
            ResponseBase<IList<MMPro.MM.UploadVideoResponse>> response = new ResponseBase<IList<MMPro.MM.UploadVideoResponse>>(obj.MqId);
            try
            {
                IList<MMPro.MM.UploadVideoResponse> list = new List<MMPro.MM.UploadVideoResponse>();
                byte[] videoBuffer = FileStorageHelper.DownloadToBuffer(obj.ObjectName);

                byte[] imageBuffer = FileStorageHelper.DownloadToBuffer(obj.ImageObjectName);
                foreach (var item in obj.ToWxIds)
                {
                    var result = wechat.SendVideoMessage(obj.WxId, item, obj.PlayLength, videoBuffer, imageBuffer);
                    list.Add(result);
                }
                response.Data = list;
                var buffer = Encoding.UTF8.GetBytes(response.ToJson());
                Message message = new Message("WECHAT_SEND_VIDEO_MESSAGE_WATCH_TOPIC", buffer);
                producer.SendMessage(message);
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = ex.Message;

                var buffer = Encoding.UTF8.GetBytes(response.ToJson());
                Message message = new Message("WECHAT_SEND_VIDEO_MESSAGE_WATCH_TOPIC", buffer);
                producer.SendMessage(message);
            }
        }


    }
}
