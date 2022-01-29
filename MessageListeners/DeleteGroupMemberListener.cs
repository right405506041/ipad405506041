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
    /// 删除群成员
    /// </summary>
    public class DeleteGroupMemberListener : MessageListenerConcurrentlyBase<GroupMember>
    {
        private DefaultMQProducer producer = RocketMqHelper.CreateDefaultMQProducer("WECHAT_DELETE_GROUP_MEMBER_WATCH_PG");
        protected override void Invoke(org.apache.rocketmq.common.message.MessageClientExt messageClientExt,GroupMember obj)
        {
            ResponseBase response = new ResponseBase(obj.MqId);
            try
            {
                IList<MMPro.MM.DelMemberReq> list = new List<MMPro.MM.DelMemberReq>();
                foreach (var item in obj.ToWxIds)
                {
                    var memberReq = new MMPro.MM.DelMemberReq();
                    memberReq.memberName = new MMPro.MM.SKBuiltinString();
                    memberReq.memberName.@string = item;
                    list.Add(memberReq);
                }
                var result = wechat.DelChatRoomMember(obj.WxId, obj.ChatRoomName, list.ToArray());
                if (result == null || result.baseResponse.ret != (int)MMPro.MM.RetConst.MM_OK)
                {
                    response.Success = false;
                    response.Message = result.baseResponse.errMsg.@string ?? "删除失败";
                }
                else
                {
                    response.Message = "删除成功";
                }
                var buffer = Encoding.UTF8.GetBytes(response.ToJson());
                Message message = new Message("WECHAT_DELETE_GROUP_MEMBER_WATCH_TOPIC", buffer);
                producer.SendMessage(message);
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = ex.Message;

                var buffer = Encoding.UTF8.GetBytes(response.ToJson());
                Message message = new Message("WECHAT_DELETE_GROUP_MEMBER_WATCH_TOPIC", buffer);
                producer.SendMessage(message);
            }
        }
    }
}
