using org.apache.rocketmq.client.producer;
using org.apache.rocketmq.common.message;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wechat.Task.App.MessageListeners.Models;
using Wechat.Task.App.MessageListeners.Models.Request.Friend;
using Wechat.Util.Extensions;
using Wechat.Util.Mq;

namespace Wechat.Task.App.MessageListeners
{
    /// <summary>
    /// 操作朋友圈 1删除朋友圈2设为隐私3设为公开4删除评论5取消点赞
    /// </summary>
    public class SetFriendCircleListener : MessageListenerConcurrentlyBase<SetFriendCircle>
    {
        private DefaultMQProducer producer = RocketMqHelper.CreateDefaultMQProducer("WECHAT_SET_FRIEND_CIRCLE_WATCH_PG");
        protected override void Invoke(org.apache.rocketmq.common.message.MessageClientExt messageClientExt,SetFriendCircle obj)
        {
            ResponseBase response = new ResponseBase(obj.MqId);
            try
            {
                var result = wechat.GetSnsObjectOp(obj.Id, obj.WxId, obj.Type);
                if (result == null || result.baseResponse.ret != (int)MMPro.MM.RetConst.MM_OK)
                {
                    response.Success = false;
                    response.Message = result.baseResponse.errMsg.@string ?? "操作失败";
                }
                else
                {
                    response.Message = "操作成功";
                }
                var buffer = Encoding.UTF8.GetBytes(response.ToJson());
                Message message = new Message("WECHAT_SET_FRIEND_CIRCLE_WATCH_TOPIC", buffer);
                producer.SendMessage(message);
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = ex.Message;

                var buffer = Encoding.UTF8.GetBytes(response.ToJson());
                Message message = new Message("WECHAT_SET_FRIEND_CIRCLE_WATCH_TOPIC", buffer);
                producer.SendMessage(message);
            }
        }
    }
}
