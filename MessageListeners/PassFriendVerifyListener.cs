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
    /// 通过好友验证
    /// </summary>
    public class PassFriendVerifyListener : MessageListenerConcurrentlyBase<FriendVerify>
    {
        private DefaultMQProducer producer = RocketMqHelper.CreateDefaultMQProducer("WECHAT_PASS_FRIEND_VERIFY_WATCH_PG");
        protected override void Invoke(org.apache.rocketmq.common.message.MessageClientExt messageClientExt,FriendVerify obj)
        {
            ResponseBase<string> response = new ResponseBase<string>(obj.MqId);
            try
            {
                var result = wechat.VerifyUser(obj.WxId, MMPro.MM.VerifyUserOpCode.MM_VERIFYUSER_VERIFYOK, obj.Content, obj.AntispamTicket, obj.UserNameV1, (byte)obj.Origin);
                if (result == null || result.baseResponse.ret != (int)MMPro.MM.RetConst.MM_OK)
                {
                    response.Success = false;
                    response.Message = result?.baseResponse?.errMsg?.@string;
                }
                else
                {
                    response.Data = result.userName;
                }
                var buffer = Encoding.UTF8.GetBytes(response.ToJson());
                Message message = new Message("WECHAT_PASS_FRIEND_VERIFY_WATCH_TOPIC", buffer);
                producer.SendMessage(message);
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = ex.Message;

                var buffer = Encoding.UTF8.GetBytes(response.ToJson());
                Message message = new Message("WECHAT_PASS_FRIEND_VERIFY_WATCH_TOPIC", buffer);
                producer.SendMessage(message);
            }
        }
    }
}
