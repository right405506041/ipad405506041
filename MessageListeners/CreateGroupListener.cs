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
    /// 创建群
    /// </summary>
    public class CreateGroupListener : MessageListenerConcurrentlyBase<CreateGroup>
    {
        private DefaultMQProducer producer = RocketMqHelper.CreateDefaultMQProducer("WECHAT_CREATE_GROUP_WATCH_PG");
        protected override void Invoke(org.apache.rocketmq.common.message.MessageClientExt messageClientExt,CreateGroup obj)
        {
            ResponseBase<string> response = new ResponseBase<string>(obj.MqId);

            try
            {
                IList<MMPro.MM.MemberReq> list = new List<MMPro.MM.MemberReq>();
                var memberReqCurrent = new MMPro.MM.MemberReq();
                memberReqCurrent.member = new MMPro.MM.SKBuiltinString();
                memberReqCurrent.member.@string = obj.WxId;
                list.Add(memberReqCurrent);
                foreach (var item in obj.ToWxIds)
                {
                    var memberReq = new MMPro.MM.MemberReq();
                    memberReq.member = new MMPro.MM.SKBuiltinString();
                    memberReq.member.@string = item;
                    list.Add(memberReq);
                }
                var result = wechat.CreateChatRoom(obj.WxId, list.ToArray(), obj.GroupName);
                if (result == null || result.baseResponse.ret != (int)MMPro.MM.RetConst.MM_OK)
                {
                    response.Success = false;
                    response.Message = result.baseResponse.errMsg.@string ?? "创建失败";
                }
                else
                {
                    response.Data = result.chatRoomName.@string;
                    response.Message = "创建成功";
                }
                var buffer = Encoding.UTF8.GetBytes(response.ToJson());
                Message message = new Message("WECHAT_CREATE_GROUP_WATCH_TOPIC", buffer);
                producer.SendMessage(message);
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = ex.Message;

                var buffer = Encoding.UTF8.GetBytes(response.ToJson());
                Message message = new Message("WECHAT_CREATE_GROUP_WATCH_TOPIC", buffer);
                producer.SendMessage(message);
            }
        }
    }
}
