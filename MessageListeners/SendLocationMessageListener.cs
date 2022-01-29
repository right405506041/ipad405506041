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
    /// 发送位置消息
    /// </summary>
    public class SendLocationMessageListener : MessageListenerConcurrentlyBase<LocationMessage>
    {
        private DefaultMQProducer producer = RocketMqHelper.CreateDefaultMQProducer("WECHAT_Send_Location_Message_WATCH_PG");
        protected override void Invoke(org.apache.rocketmq.common.message.MessageClientExt messageClientExt,LocationMessage obj)
        {
            ResponseBase<IList<MMPro.MM.NewSendMsgRespone>> response = new ResponseBase<IList<MMPro.MM.NewSendMsgRespone>>(obj.MqId);
            try
            {
                IList<MMPro.MM.NewSendMsgRespone> list = new List<MMPro.MM.NewSendMsgRespone>();
                string appMessageFormat = $"<?xml version=\"1.0\"?>\n<msg>\n\t<location x=\"{obj.Latitude}\" y=\"{obj.Longitude}\" scale=\"16\" label=\"{obj.Name}\" maptype=\"0\" poiname=\"[位置]{obj.Name}\" poiid=\"\" />\n</msg>";
                foreach (var item in obj.ToWxIds)
                {
                    var result = wechat.SendNewMsg(obj.WxId, item, appMessageFormat, 48);
                    list.Add(result);
                }
                response.Data = list;
                var buffer = Encoding.UTF8.GetBytes(response.ToJson());
                Message message = new Message("WECHAT_Send_Location_Message_WATCH_TOPIC", buffer);
                producer.SendMessage(message);
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = ex.Message;

                var buffer = Encoding.UTF8.GetBytes(response.ToJson());
                Message message = new Message("WECHAT_Send_Location_Message_WATCH_TOPIC", buffer);
                producer.SendMessage(message);
            }
        }
    }
}
