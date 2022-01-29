using org.apache.rocketmq.client.producer;
using org.apache.rocketmq.common.message;
using System;
using System.Text;
using Wechat.Task.App.MessageListeners.Models;
using Wechat.Task.App.MessageListeners.Models.Request.Friend;
using Wechat.Util.Extensions;
using Wechat.Util.Mq;
using System;

namespace Wechat.Task.App.MessageListeners
{
    /// <summary>
    /// 朋友圈点赞发评论
    /// </summary>
    public class SendFriendCircleCommentListener : MessageListenerConcurrentlyBase<FriendCircleComment>
    {
        private DefaultMQProducer producer = RocketMqHelper.CreateDefaultMQProducer("WECHAT_SEND_FRIEND_CIRCLE_COMMENT_WATCH_PG");
        protected override void Invoke(org.apache.rocketmq.common.message.MessageClientExt messageClientExt,FriendCircleComment obj)
        {
            ResponseBase<micromsg.SnsObject> response = new ResponseBase<micromsg.SnsObject>(obj.MqId);
            try
            {
                var result = wechat.SnsComment(Convert.ToUInt64(obj.Id), obj.WxId, obj.WxId, obj.ReplyCommnetId, obj.Content, (MMPro.MM.SnsObjectType)obj.Type);
                if (result == null || result.BaseResponse.Ret != (int)MMPro.MM.RetConst.MM_OK)
                {
                    response.Success = false;
                    response.Message = result.BaseResponse.ErrMsg.String ?? "发送失败";
                }
                else
                {
                    response.Data = result.SnsObject;
                    response.Message = "发送成功";
                }
                var buffer = Encoding.UTF8.GetBytes(response.ToJson());
                Message message = new Message("WECHAT_SEND_FRIEND_CIRCLE_COMMENT_WATCH_TOPIC", buffer);
                producer.SendMessage(message);
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = ex.Message;

                var buffer = Encoding.UTF8.GetBytes(response.ToJson());
                Message message = new Message("WECHAT_SEND_FRIEND_CIRCLE_COMMENT_WATCH_TOPIC", buffer);
                producer.SendMessage(message);
            }
        }
    }
}
