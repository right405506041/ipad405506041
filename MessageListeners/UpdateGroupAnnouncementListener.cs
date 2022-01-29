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
    /// 修改群公告
    /// </summary>
    public class UpdateGroupAnnouncementListener : MessageListenerConcurrentlyBase<GroupAnnouncement>
    {
        private DefaultMQProducer producer = RocketMqHelper.CreateDefaultMQProducer("WECHAT_UPDATE_GROUP_ANNOUNCEMENT_WATCH_PG");
        protected override void Invoke(org.apache.rocketmq.common.message.MessageClientExt messageClientExt,GroupAnnouncement obj)
        {
            ResponseBase response = new ResponseBase(obj.MqId);
            try
            {
                var result = wechat.setChatRoomAnnouncement(obj.WxId, obj.ChatRoomName, obj.Announcement);
                if (result == null || result.BaseResponse.Ret != (int)MMPro.MM.RetConst.MM_OK)
                {
                    response.Success = false;
                    response.Message = result.BaseResponse.ErrMsg.String ?? "修改失败";
                }
                else
                {
                    response.Message = "修改成功";
                }
                var buffer = Encoding.UTF8.GetBytes(response.ToJson());
                Message message = new Message("WECHAT_UPDATE_GROUP_ANNOUNCEMENT_WATCH_TOPIC", buffer);
                producer.SendMessage(message);
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = ex.Message;

                var buffer = Encoding.UTF8.GetBytes(response.ToJson());
                Message message = new Message("WECHAT_UPDATE_GROUP_ANNOUNCEMENT_WATCH_TOPIC", buffer);
                producer.SendMessage(message);
            }
        }
    }
}
