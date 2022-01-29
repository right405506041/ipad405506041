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
    /// 新增标签
    /// </summary>
    public class AddLabelNameListener : MessageListenerConcurrentlyBase<AddLabel>
    {
        private DefaultMQProducer producer = RocketMqHelper.CreateDefaultMQProducer("WECHAT_ADD_LABEL_NAME_WATCH_PG");
        protected override void Invoke(org.apache.rocketmq.common.message.MessageClientExt messageClientExt,AddLabel obj)
        {
            ResponseBase<IList<micromsg.LabelPair>> response = new ResponseBase<IList<micromsg.LabelPair>>(obj.MqId);
            try
            {
                var result = wechat.AddContactLabel(obj.WxId, obj.LabelName);
                if (result == null || result.BaseResponse.Ret != (int)MMPro.MM.RetConst.MM_OK)
                {
                    response.Success = false;
                    response.Message = result.BaseResponse.ErrMsg.String ?? "添加失败";
                }
                else
                {
                    response.Data = result.LabelPairList;
                    response.Message = "添加成功";
                }
                var buffer = Encoding.UTF8.GetBytes(response.ToJson());
                Message message = new Message("WECHAT_ADD_LABEL_NAME_WATCH_TOPIC", buffer);
                producer.SendMessage(message);
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = ex.Message;

                var buffer = Encoding.UTF8.GetBytes(response.ToJson());
                Message message = new Message("WECHAT_ADD_LABEL_NAME_WATCH_TOPIC", buffer);
                producer.SendMessage(message);
            }
        }
    }
}
