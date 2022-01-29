using org.apache.rocketmq.client.producer;
using org.apache.rocketmq.common.message;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Wechat.Task.App.MessageListeners.Models;
using Wechat.Task.App.MessageListeners.Models.Request.Friend;
using Wechat.Task.App.MessageListeners.Models.Request.Group;
using Wechat.Util.Extensions;
using Wechat.Util.Mq;
using System;
namespace Wechat.Task.App.MessageListeners
{
    /// <summary>
    /// 获取群成员信息
    /// </summary>
    public class GetGroupMembersListener : MessageListenerConcurrentlyBase<GetGroupMember>
    {
        private DefaultMQProducer producer = RocketMqHelper.CreateDefaultMQProducer("WECHAT_GET_GROUP_MEMBERS_WATCH_PG");
        protected override void Invoke(org.apache.rocketmq.common.message.MessageClientExt messageClientExt,GetGroupMember obj)
        {
            ResponseBase<MMPro.MM.ChatRoomMemberData> response = new ResponseBase<MMPro.MM.ChatRoomMemberData>(obj.MqId);
            try
            {
                var result = wechat.GetChatroomMemberDetail(obj.WxId, obj.ChatRoomName);
                if (result == null || result.baseResponse.ret != (int)MMPro.MM.RetConst.MM_OK)
                {
                    response.Success = false;
                    response.Message = result.baseResponse.errMsg.@string ?? "获取失败";
                }
                else
                {
                    response.Data = result.newChatroomData;
                }

                var buffer = Encoding.UTF8.GetBytes(response.ToJson());
                Message message = new Message("WECHAT_GET_GROUP_MEMBERS_WATCH_TOPIC", buffer);
                producer.SendMessage(message);
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = ex.Message;

                var buffer = Encoding.UTF8.GetBytes(response.ToJson());
                Message message = new Message("WECHAT_GET_GROUP_MEMBERS_WATCH_TOPIC", buffer);
                producer.SendMessage(message);
            }
        }
    }
}
