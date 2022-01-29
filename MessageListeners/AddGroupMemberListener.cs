using org.apache.rocketmq.client.producer;
using org.apache.rocketmq.common.message;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Wechat.Task.App.MessageListeners.Models;
using Wechat.Task.App.MessageListeners.Models.Request.Friend;
using Wechat.Task.App.MessageListeners.Models.Request.Group;
using Wechat.Util.Extensions;
using Wechat.Util.Mq;

namespace Wechat.Task.App.MessageListeners
{
    /// <summary>
    /// 添加群成员
    /// </summary>
    public class AddGroupMemberListener : MessageListenerConcurrentlyBase<GroupMember>
    {
        private DefaultMQProducer producer = RocketMqHelper.CreateDefaultMQProducer("WECHAT_ADD_GROUP_MEMBER_WATCH_PG");
        protected override void Invoke(org.apache.rocketmq.common.message.MessageClientExt messageClientExt,GroupMember obj)
        {
            ResponseBase response = new ResponseBase(obj.MqId);
            try
            {
                IList<MMPro.MM.MemberReq> list = new List<MMPro.MM.MemberReq>();
                foreach (var item in obj.ToWxIds)
                {
                    var memberReq = new MMPro.MM.MemberReq();
                    memberReq.member = new MMPro.MM.SKBuiltinString();
                    memberReq.member.@string = item;
                    list.Add(memberReq);
                }
                var result = wechat.AddChatRoomMember(obj.WxId, obj.ChatRoomName, list.ToArray());
                if (result == null || result.baseResponse.ret != (int)MMPro.MM.RetConst.MM_OK)
                {
                    response.Success = false;
                    response.Message = result.baseResponse.errMsg.@string ?? "添加失败";
                }
                else
                {
                    response.Message = "添加成功";
                }
                var buffer = Encoding.UTF8.GetBytes(response.ToJson());
                Message message = new Message("WECHAT_ADD_GROUP_MEMBER_WATCH_TOPIC", buffer);
                producer.SendMessage(message);
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = ex.Message;

                var buffer = Encoding.UTF8.GetBytes(response.ToJson());
                Message message = new Message("WECHAT_ADD_GROUP_MEMBER_WATCH_TOPIC", buffer);
                producer.SendMessage(message);
            }
        }
    }
}
