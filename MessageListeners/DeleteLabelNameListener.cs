using org.apache.rocketmq.client.producer;
using org.apache.rocketmq.common.message;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Wechat.Task.App.MessageListeners.Models;
using Wechat.Task.App.MessageListeners.Models.Request.Friend;
using Wechat.Task.App.MessageListeners.Models.Request.Group;
using Wechat.Task.App.MessageListeners.Models.Request.Label;
using Wechat.Util.Extensions;
using Wechat.Util.Mq;

namespace Wechat.Task.App.MessageListeners
{
    /// <summary>
    /// 删除标签
    /// </summary>
    public class DeleteLabelNameListener : MessageListenerConcurrentlyBase<DeleteLabel>
    {
        private DefaultMQProducer producer = RocketMqHelper.CreateDefaultMQProducer("WECHAT_DELETE_LABEL_NAME_WATCH_PG");
        protected override void Invoke(org.apache.rocketmq.common.message.MessageClientExt messageClientExt,DeleteLabel obj)
        {
            ResponseBase response = new ResponseBase(obj.MqId);
            try
            {
                var result = wechat.DelContactLabel(obj.WxId, obj.LabelIDList);
                if (result == null || result.BaseResponse.Ret != (int)MMPro.MM.RetConst.MM_OK)
                {
                    response.Success = false;
                    response.Message = result.BaseResponse.ErrMsg.String ?? "删除失败";
                }
                else
                {
                    response.Message = "删除成功";
                }
                var buffer = Encoding.UTF8.GetBytes(response.ToJson());
                Message message = new Message("WECHAT_DELETE_LABEL_NAME_WATCH_TOPIC", buffer);
                producer.SendMessage(message);
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = ex.Message;

                var buffer = Encoding.UTF8.GetBytes(response.ToJson());
                Message message = new Message("WECHAT_DELETE_LABEL_NAME_WATCH_TOPIC", buffer);
                producer.SendMessage(message);
            }
        }
    }
}
