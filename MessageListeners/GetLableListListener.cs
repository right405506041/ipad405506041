using org.apache.rocketmq.client.producer;
using org.apache.rocketmq.common.message;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wechat.Task.App.MessageListeners.Models;
using Wechat.Task.App.MessageListeners.Models.Request;
using Wechat.Task.App.MessageListeners.Models.Request.Friend;
using Wechat.Util;
using Wechat.Util.Extensions;
using Wechat.Util.Mq;
namespace Wechat.Task.App.MessageListeners
{

    /// <summary>
    /// 获取标签列表
    /// </summary>
    public class GetLableListListener : MessageListenerConcurrentlyBase<RequestBase>
    {
        private DefaultMQProducer producer = RocketMqHelper.CreateDefaultMQProducer("WECHAT_GET_LABLE_LIST_WATCH_PG");
        protected override void Invoke(org.apache.rocketmq.common.message.MessageClientExt messageClientExt,RequestBase obj)
        {
            ResponseBase<MMPro.MM.LabelPair[]> response = new ResponseBase<MMPro.MM.LabelPair[]>(obj.MqId);
            try
            {
                var result = wechat.GetContactLabelList(obj.WxId);
                if (result == null || result.baseResponse.ret != (int)MMPro.MM.RetConst.MM_OK)
                {
                    response.Success = false;
                    response.Message = result.baseResponse.errMsg.@string ?? "获取失败";
                }
                else
                {
                    response.Data = result.labelPairList;
                }
                var buffer = Encoding.UTF8.GetBytes(response.ToJson());
                Message message = new Message("WECHAT_GET_LABLE_LIST_WATCH_TOPIC", buffer);
                producer.SendMessage(message);
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = ex.Message;

                var buffer = Encoding.UTF8.GetBytes(response.ToJson());
                Message message = new Message("WECHAT_GET_LABLE_LIST_WATCH_TOPIC", buffer);
                producer.SendMessage(message);
            }
        }
    }
}
