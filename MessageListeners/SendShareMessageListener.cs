using org.apache.rocketmq.client.producer;
using org.apache.rocketmq.common.message;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wechat.Task.App.Extensions;
using Wechat.Task.App.MessageListeners.Models;
using Wechat.Task.App.MessageListeners.Models.Request.Message;
using Wechat.Util.Extensions;
using Wechat.Util.FileStore;
using Wechat.Util.Mq;


namespace Wechat.Task.App.MessageListeners
{
    /// <summary>
    /// 发送分享消息
    /// </summary>
    public class SendShareMessageListener : MessageListenerConcurrentlyBase<ShareMessage>
    {
        private DefaultMQProducer producer = RocketMqHelper.CreateDefaultMQProducer("WECHAT_SEND_SHARE_MESSAGE_WATCH_PG");
        protected override void Invoke(org.apache.rocketmq.common.message.MessageClientExt messageClientExt,ShareMessage obj)
        {
            ResponseBase<IList<micromsg.SendAppMsgResponse>> response = new ResponseBase<IList<micromsg.SendAppMsgResponse>>(obj.MqId);
            try
            {
                IList<micromsg.SendAppMsgResponse> list = new List<micromsg.SendAppMsgResponse>();
                string dataUrl = string.IsNullOrEmpty(obj.DataUrl) ? obj.Url : obj.DataUrl;
                string appMessageFormat = $"<appmsg  sdkver=\"0\"><title>{obj.Title}</title><des>{obj.Desc}</des><type>{obj.Type}</type><showtype>0</showtype><soundtype>0</soundtype><contentattr>0</contentattr><url>{obj.Url}</url><lowurl>{obj.Url}</lowurl><dataurl>{dataUrl}</dataurl><lowdataurl>{dataUrl}</lowdataurl> <thumburl>{obj.ThumbUrl}</thumburl></appmsg>";
                foreach (var item in obj.ToWxIds)
                {
                    var result = wechat.SendAppMsg(appMessageFormat, item, obj.WxId);
                    list.Add(result);
                }
                response.Data = list;
                var buffer = Encoding.UTF8.GetBytes(response.ToJson());
                Message message = new Message("WECHAT_SEND_SHARE_MESSAGE_WATCH_TOPIC", buffer);
                producer.SendMessage(message);
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = ex.Message;

                var buffer = Encoding.UTF8.GetBytes(response.ToJson());
                Message message = new Message("WECHAT_SEND_SHARE_MESSAGE_WATCH_TOPIC", buffer);
                producer.SendMessage(message);
            }
        }
    }
}
