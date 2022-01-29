using org.apache.rocketmq.client.producer;
using org.apache.rocketmq.common.message;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wechat.Task.App.MessageListeners.Models;
using Wechat.Task.App.Models.Request.Message;
using Wechat.Util.Extensions;
using Wechat.Util.Mq;

namespace Wechat.Task.App.MessageListeners
{
    /// <summary>
    /// 发送媒体消息
    /// </summary>
    public class SendMediaMessageListener : MessageListenerConcurrentlyBase<MediaMessage>
    {
        private DefaultMQProducer producer = RocketMqHelper.CreateDefaultMQProducer("WECHAT_SEND_MEDIA_MESSAGE_WATCH_PG");
        protected override void Invoke(org.apache.rocketmq.common.message.MessageClientExt messageClientExt, MediaMessage obj)
        {
            ResponseBase<IList<micromsg.SendAppMsgResponse>> response = new ResponseBase<IList<micromsg.SendAppMsgResponse>>(obj.MqId);
            try
            {
                IList<micromsg.SendAppMsgResponse> list = new List<micromsg.SendAppMsgResponse>();

                string appMessageFormat = $"<?xml version=\"1.0\"?>\n<appmsg appid='' sdkver=''><title>{obj.Title}</title><des></des><action></action><type>6</type><content></content><url></url><lowurl></lowurl><appattach><totallen>{obj.Length}</totallen><attachid>{obj.AttachId}</attachid><fileext>{obj.FileExt}</fileext></appattach><extinfo></extinfo></appmsg>";
                foreach (var item in obj.ToWxIds)
                {
                    var result = wechat.SendAppMsg(appMessageFormat, item, obj.WxId, 6);
                    list.Add(result);
                }
                response.Data = list;

                var buffer = Encoding.UTF8.GetBytes(response.ToJson());
                Message message = new Message("WECHAT_SEND_MEDIA_MESSAGE_WATCH_TOPIC", buffer);
                producer.SendMessage(message);
            }           
            catch (Exception ex)
            {
                response.Success = false; 
                response.Message = ex.Message;           

                var buffer = Encoding.UTF8.GetBytes(response.ToJson());
                Message message = new Message("WECHAT_SEND_MEDIA_MESSAGE_WATCH_TOPIC", buffer);
                producer.SendMessage(message);
            }
      

        
        }

    }
}
