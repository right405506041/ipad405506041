using org.apache.rocketmq.client.producer;
using org.apache.rocketmq.common.message;
using System;
using System.Linq;
using System.Text;
using Wechat.Task.App.MessageListeners.Models;
using Wechat.Task.App.MessageListeners.Models.Response.FriendCircle;
using Wechat.Task.App.Models.Request.Friend;
using Wechat.Util.Extensions;
using Wechat.Util.Mq;

namespace Wechat.Task.App.MessageListeners
{
    /// <summary>
    /// 获取自己的朋友圈列表
    /// </summary>
    public class GetFriendCircleListListener : MessageListenerConcurrentlyBase<FriendCircle>
    {
        private DefaultMQProducer producer = RocketMqHelper.CreateDefaultMQProducer("WECHAT_GET_FRIEND_CIRCLE_LIST_WATCH_PG");
        protected override void Invoke(org.apache.rocketmq.common.message.MessageClientExt messageClientExt,FriendCircle obj)
        {
            ResponseBase<FriendCircleResponse> response = new ResponseBase<FriendCircleResponse>(obj.MqId);
            try
            {
                var result = wechat.SnsTimeLine(obj.WxId, obj.FristPageMd5);
                if (result == null || result.baseResponse.ret != (int)MMPro.MM.RetConst.MM_OK)
                {
                    response.Success = false;
                    response.Message = result.baseResponse.errMsg.@string ?? "获取失败";
                }
                else
                {
                    response.Data = new FriendCircleResponse()
                    {
                        FristPageMd5 = result.fristPageMd5,
                        ObjectList = result.objectList
                    };
                }
                var buffer = Encoding.UTF8.GetBytes(response.ToJson());
                Message message = new Message("WECHAT_GET_FRIEND_CIRCLE_LIST_WATCH_TOPIC", buffer);
                producer.SendMessage(message);
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = ex.Message;

                var buffer = Encoding.UTF8.GetBytes(response.ToJson());
                Message message = new Message("WECHAT_GET_FRIEND_CIRCLE_LIST_WATCH_TOPIC", buffer);
                producer.SendMessage(message);
            }
        }
    }
}
