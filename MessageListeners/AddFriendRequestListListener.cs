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
    /// 批量申请添加好友请求
    /// </summary>
    public class AddFriendRequestListListener : MessageListenerConcurrentlyBase<AddFriendList>
    {
        private DefaultMQProducer producer = RocketMqHelper.CreateDefaultMQProducer("WECHAT_ADD_FRIEND_REQUEST_LIST_WATCH_PG");
        protected override void Invoke(org.apache.rocketmq.common.message.MessageClientExt messageClientExt,AddFriendList obj)
        {
            ResponseBase<string> response = new ResponseBase<string>(obj.MqId);
            try
            {
                MMPro.MM.VerifyUser[] verifyUser_ = new MMPro.MM.VerifyUser[obj.Friends.Count];
                for (int i = 0; i < obj.Friends.Count; i++)
                {
                    MMPro.MM.VerifyUser user = new MMPro.MM.VerifyUser();
                    user.value = obj.Friends[i].UserNameV1;
                    user.antispamTicket = obj.Friends[i].AntispamTicket;
                    user.friendFlag = 0;
                    user.scanQrcodeFromScene = 0;
                    verifyUser_[i] = user;
                }
                byte[] Origin = new byte[4];
                // 由高位到低位  
                Origin[0] = (byte)((obj.Origin >> 24) & 0xFF);
                Origin[1] = (byte)((obj.Origin >> 16) & 0xFF);
                Origin[2] = (byte)((obj.Origin >> 8) & 0xFF);
                Origin[3] = (byte)(obj.Origin & 0xFF);
               

                var result = wechat.VerifyUserList(obj.WxId, MMPro.MM.VerifyUserOpCode.MM_VERIFYUSER_SENDREQUEST, obj.Content, verifyUser_, Origin);
                //      var result = wechat.VerifyUser(obj.WxId, MMPro.MM.VerifyUserOpCode.MM_VERIFYUSER_SENDREQUEST, obj.Content, obj.AntispamTicket, obj.UserNameV1, (byte)obj.Origin);
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
                Message message = new Message("WECHAT_ADD_FRIEND_REQUEST_LIST_WATCH_TOPIC", buffer);
                producer.SendMessage(message);
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = ex.Message;

                var buffer = Encoding.UTF8.GetBytes(response.ToJson());
                Message message = new Message("WECHAT_ADD_FRIEND_REQUEST_LIST_WATCH_TOPIC", buffer);
                producer.SendMessage(message);
            }
        }
    }
}
