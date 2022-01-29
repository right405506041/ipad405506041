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
using Wechat.Util.Extensions;
using Wechat.Util.Mq;

namespace Wechat.Task.App.MessageListeners
{
    /// <summary>
    /// 发送文本消息
    /// </summary>
    public class SendTxtMessageListener : MessageListenerConcurrentlyBase<TxtMessage>
    {
        private DefaultMQProducer producer = RocketMqHelper.CreateDefaultMQProducer("WECHAT_SEND_TXT_MESSAGE_WATCH_PG");
        protected override void Invoke(org.apache.rocketmq.common.message.MessageClientExt messageClientExt, TxtMessage obj)
        {
            ResponseBase<IList<MMPro.MM.NewSendMsgRespone.NewMsgResponeNew>> response = new ResponseBase<IList<MMPro.MM.NewSendMsgRespone.NewMsgResponeNew>>(obj.MqId);
            try
            {
                IList<MMPro.MM.NewSendMsgRespone.NewMsgResponeNew> list = new List<MMPro.MM.NewSendMsgRespone.NewMsgResponeNew>();
                foreach (var item in obj.ToWxIds)
                {
                    Thread.Sleep(50);
                    var result = wechat.SendNewMsg(obj.WxId, item, obj.Content);
                    list.Add(result.List.FirstOrDefault());
                }
                response.Data = list;


                var buffer = Encoding.UTF8.GetBytes(response.ToJson());
                Message message = new Message("WECHAT_SEND_TXT_MESSAGE_WATCH_TOPIC", buffer);
                producer.SendMessage(message);
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = ex.Message;

                var buffer = Encoding.UTF8.GetBytes(response.ToJson());
                Message message = new Message("WECHAT_SEND_TXT_MESSAGE_WATCH_TOPIC", buffer);
                producer.SendMessage(message);
            }
        }
    }
}
