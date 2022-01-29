using org.apache.rocketmq.client.producer;
using org.apache.rocketmq.common.message;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wechat.Task.App.Helper;
using Wechat.Task.App.MessageListeners.Models;
using Wechat.Task.App.MessageListeners.Models.Request.Message;
using Wechat.Util.Extensions;
using Wechat.Util.FileStore;
using Wechat.Util.Mq;

namespace Wechat.Task.App.MessageListeners
{
    /// <summary>
    /// 发送朋友圈
    /// </summary>
    public class SendFriendCircleListener : MessageListenerConcurrentlyBase<SendFriendCircle>
    {
        private DefaultMQProducer producer = RocketMqHelper.CreateDefaultMQProducer("WECHAT_SEND_FRIEND_CIRCLE_WATCH_PG");
        protected override void Invoke(org.apache.rocketmq.common.message.MessageClientExt messageClientExt, SendFriendCircle obj)
        {
            ResponseBase<MMPro.MM.SnsObject> response = new ResponseBase<MMPro.MM.SnsObject>(obj.MqId);
            try
            {
                string content = null;
                switch (obj.Type)
                {
                    case 0: content = SendSnsConst.GetContentTemplate(obj.WxId, wechat, obj.Content, obj.Title, obj.ContentUrl, obj.Description); break;
                    case 1: content = SendSnsConst.GetImageTemplate(obj.WxId, wechat, obj.Content, obj.MediaInfos, obj.Title, obj.ContentUrl, obj.Description); break;
                    case 2: content = SendSnsConst.GetVideoTemplate(obj.WxId, wechat, obj.Content, obj.MediaInfos, obj.Title, obj.ContentUrl, obj.Description); break;
                    case 3: content = SendSnsConst.GetLinkTemplate(obj.WxId, wechat, obj.Content, obj.MediaInfos, obj.Title, obj.ContentUrl, obj.Description); break;
                    case 4: content = SendSnsConst.GetImageTemplate3(obj.WxId, wechat, obj.Content, obj.MediaInfos, obj.Title, obj.ContentUrl, obj.Description); break;
                    case 5: content = SendSnsConst.GetImageTemplate4(obj.WxId, wechat, obj.Content, obj.MediaInfos, obj.Title, obj.ContentUrl, obj.Description); break;
                    case 6: content = SendSnsConst.GetImageTemplate5(obj.WxId, wechat, obj.Content, obj.MediaInfos, obj.Title, obj.ContentUrl, obj.Description); break;

                }

                var result = wechat.SnsPost(obj.WxId, content, obj.BlackList, obj.WithUserList);
                if (result == null || result.baseResponse.ret != (int)MMPro.MM.RetConst.MM_OK)
                {
                    response.Success = false;

                    response.Message = result.baseResponse.errMsg.@string ?? "发送失败";
                }
                else
                {
                    response.Message = "发送成功";
                    response.Data = result.snsObject;
                }
                var buffer = Encoding.UTF8.GetBytes(response.ToJson());
                Message message = new Message("WECHAT_SEND_FRIEND_CIRCLE_WATCH_TOPIC", buffer);
                producer.SendMessage(message);
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = ex.Message;

                var buffer = Encoding.UTF8.GetBytes(response.ToJson());
                Message message = new Message("WECHAT_SEND_FRIEND_CIRCLE_WATCH_TOPIC", buffer);
                producer.SendMessage(message);
            }
        }
    }
}
