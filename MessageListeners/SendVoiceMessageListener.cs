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
    /// 发送声音消息
    /// </summary>
    public class SendVoiceMessageListener : MessageListenerConcurrentlyBase<VoiceMessage>
    {
        private DefaultMQProducer producer = RocketMqHelper.CreateDefaultMQProducer("WECHAT_SEND_VOICE_MESSAGE_WATCH_PG");
        protected override void Invoke(org.apache.rocketmq.common.message.MessageClientExt messageClientExt,VoiceMessage obj)
        {
            ResponseBase<IList<MMPro.MM.UploadVoiceResponse>> response = new ResponseBase<IList<MMPro.MM.UploadVoiceResponse>>(obj.MqId);
            try
            {
                IList<MMPro.MM.UploadVoiceResponse> list = new List<MMPro.MM.UploadVoiceResponse>();
                byte[] voiceBuffer = FileStorageHelper.DownloadToBuffer(obj.ObjectName);
                foreach (var item in obj.ToWxIds)
                {
                    var result = wechat.SendVoiceMessage(obj.WxId, item, voiceBuffer, obj.FileName.GetVoiceType(), obj.VoiceSecond * 100);
                    list.Add(result);
                }
                response.Data = list;
                var buffer = Encoding.UTF8.GetBytes(response.ToJson());
                Message message = new Message("WECHAT_SEND_VOICE_MESSAGE_WATCH_TOPIC", buffer);
                producer.SendMessage(message);
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = ex.Message;

                var buffer = Encoding.UTF8.GetBytes(response.ToJson());
                Message message = new Message("WECHAT_SEND_VOICE_MESSAGE_WATCH_TOPIC", buffer);
                producer.SendMessage(message);
            }
        }
    }
}
