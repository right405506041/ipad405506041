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
using Wechat.Util.Mq;
using Wechat.Util.Times;

namespace Wechat.Task.App.MessageListeners
{

    public class SendGroupTxtMessageListener : MessageListenerConcurrentlyBase<TxtMessage>
    {
        private DefaultMQProducer producer = RocketMqHelper.CreateDefaultMQProducer("WECHAT_SEND_GROUP_TXT_MESSAGE_WATCH_PG");
        protected override void Invoke(org.apache.rocketmq.common.message.MessageClientExt messageClientExt, TxtMessage obj)
        {
            ResponseBase<IList<MMPro.MM.NewSendMsgRespone.NewMsgResponeNew>> response = new ResponseBase<IList<MMPro.MM.NewSendMsgRespone.NewMsgResponeNew>>(obj.MqId);
            try
            {
                int time = 5000;
                foreach (var item in obj.ToWxIds)
                {
                    IList<MMPro.MM.NewSendMsgRespone.NewMsgResponeNew> list = new List<MMPro.MM.NewSendMsgRespone.NewMsgResponeNew>();

                    //time = new Random(DateTime.Now.Millisecond).Next(3, 10) * 1000;
                    //Thread.Sleep(time);
                    try
                    {
                        var result = wechat.SendNewMsg(obj.WxId, item, $"{DateTime.Now.ToString("yyyy月MM月dd日 HH:mm:ss:fff")} {obj.Content}");
                        if (result == null || result.List == null)
                        {
                            MMPro.MM.NewSendMsgRespone.NewMsgResponeNew ss = new MMPro.MM.NewSendMsgRespone.NewMsgResponeNew();
                            ss.Ret = 1;
                            list.Add(ss);
                        }
                        else
                        {
                            list.Add(result.List.FirstOrDefault());
                        }
                    }
                    catch (ExpiredException)
                    {
                        break;
                    }
                    catch (Exception ex)
                    {
                        MMPro.MM.NewSendMsgRespone.NewMsgResponeNew ss = new MMPro.MM.NewSendMsgRespone.NewMsgResponeNew();
                        ss.Ret = 1;
                        list.Add(ss);
                        Util.Log.Logger.GetLog(this.GetType()).Error(ex);
                    }
                    response.Data = list;

                    var buffer = Encoding.UTF8.GetBytes(response.ToJson());
                    Message message = new Message("WECHAT_SEND_GROUP_TXT_MESSAGE_WATCH_TOPIC", buffer);
                    producer.SendMessage(message);

                }
        
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = ex.Message;

                var buffer = Encoding.UTF8.GetBytes(response.ToJson());
                Message message = new Message("WECHAT_SEND_GROUP_TXT_MESSAGE_WATCH_TOPIC", buffer);
                producer.SendMessage(message);
            }
        }
    }
}
