using org.apache.rocketmq.client.producer;
using org.apache.rocketmq.common.message;
using System;
using System.Linq;
using System.Text;
using Wechat.Task.App.MessageListeners.Models;
using Wechat.Task.App.MessageListeners.Models.Request;
using Wechat.Task.App.Models.Request.Friend;
using Wechat.Util;
using Wechat.Util.Extensions;
using Wechat.Util.Mq;


namespace Wechat.Task.App.MessageListeners
{
    /// <summary>
    /// 同步朋友圈
    /// </summary>
    public class SyncFriendCircleListener : MessageListenerConcurrentlyBase<RequestBase>
    {
        private DefaultMQProducer producer = RocketMqHelper.CreateDefaultMQProducer("WECHAT_SYNC_FRIEND_CIRCLE_WATCH_PG");
        protected override void Invoke(org.apache.rocketmq.common.message.MessageClientExt messageClientExt,RequestBase obj)
        {
            ResponseBase response = new ResponseBase(obj.MqId);
            try
            {
                var result = wechat.SnsSync(obj.WxId);
                if (result == null || result.baseResponse.ret != (int)MMPro.MM.RetConst.MM_OK)
                {
                    response.Success = false;
                    response.Message = result.baseResponse.errMsg.@string ?? "同步失败";
                }
                else
                {
                    response.Message = "同步成功";
                }

                var buffer = Encoding.UTF8.GetBytes(response.ToJson());
                Message message = new Message("WECHAT_SYNC_FRIEND_CIRCLE_WATCH_TOPIC", buffer);
                producer.SendMessage(message);
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = ex.Message;

                var buffer = Encoding.UTF8.GetBytes(response.ToJson());
                Message message = new Message("WECHAT_SYNC_FRIEND_CIRCLE_WATCH_TOPIC", buffer);
                producer.SendMessage(message);
            }
        }
    }
}
