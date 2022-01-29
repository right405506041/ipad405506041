using org.apache.rocketmq.client.producer;
using org.apache.rocketmq.common.message;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wechat.Task.App.MessageListeners.Models;
using Wechat.Task.App.MessageListeners.Models.Request.Message;
using Wechat.Util.Extensions;
using Wechat.Util.FileStore;
using Wechat.Util.Mq;

namespace Wechat.Task.App.MessageListeners
{
    /// <summary>
    /// 发送名片消息
    /// </summary>
    public class SendCardMessageListener : MessageListenerConcurrentlyBase<CardMessage>
    {
        private DefaultMQProducer producer = RocketMqHelper.CreateDefaultMQProducer("WECHAT_SEND_CARD_MESSAGE_WATCH_PG");
        protected override void Invoke(org.apache.rocketmq.common.message.MessageClientExt messageClientExt,CardMessage obj)
        {
            ResponseBase<IList<MMPro.MM.NewSendMsgRespone>> response = new ResponseBase<IList<MMPro.MM.NewSendMsgRespone>>(obj.MqId);
            try
            {
                IList<MMPro.MM.NewSendMsgRespone> list = new List<MMPro.MM.NewSendMsgRespone>();
                obj.CardNickName = string.IsNullOrEmpty(obj.CardNickName) ? obj.CardWxId : obj.CardNickName;
                string appMessageFormat = $"<?xml version=\"1.0\"?>\n<msg bigheadimgurl=\"\" smallheadimgurl=\"\" username=\"{obj.CardWxId}\" nickname=\"{obj.CardNickName}\" fullpy=\"\" shortpy=\"\" alias=\"{obj.CardAlias}\" imagestatus=\"0\" scene=\"17\" province=\"\" city=\"\" sign=\"\" sex=\"2\" certflag=\"0\" certinfo=\"\" brandIconUrl=\"\" brandHomeUrl=\"\" brandSubscriptConfigUrl=\"\" brandFlags=\"0\" regionCode=\"CN\" />\n";
                foreach (var item in obj.ToWxIds)
                {
                    var result = wechat.SendNewMsg(obj.WxId, item, appMessageFormat, 42);
                    list.Add(result);
                }
                response.Data = list;
                var buffer = Encoding.UTF8.GetBytes(response.ToJson());
                Message message = new Message("WECHAT_SEND_CARD_MESSAGE_WATCH_TOPIC", buffer);
                producer.SendMessage(message);
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = ex.Message;

                var buffer = Encoding.UTF8.GetBytes(response.ToJson());
                Message message = new Message("WECHAT_SEND_CARD_MESSAGE_WATCH_TOPIC", buffer);
                producer.SendMessage(message);
            }
        }
    }
}
