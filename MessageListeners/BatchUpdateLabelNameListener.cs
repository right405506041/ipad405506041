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
    /// 批量修改标签
    /// </summary>
    public class BatchUpdateLabelNameListener : MessageListenerConcurrentlyBase<BatchUpdateLabel>
    {
        private DefaultMQProducer producer = RocketMqHelper.CreateDefaultMQProducer("WECHAT_BATCH_UPDATE_LABEL_NAME_WATCH_PG");
        protected override void Invoke(org.apache.rocketmq.common.message.MessageClientExt messageClientExt,BatchUpdateLabel obj)
        {
            ResponseBase response = new ResponseBase(obj.MqId);
            try
            {

                micromsg.UserLabelInfo[] userLabels = new micromsg.UserLabelInfo[obj.LabelInfos.Count];

                for (int i = 0; i < obj.LabelInfos.Count; i++)
                {
                    userLabels[i] = new micromsg.UserLabelInfo();
                    userLabels[i].LabelIDList = obj.LabelInfos[i].LabelIdList;
                    userLabels[i].UserName = obj.LabelInfos[i].ToWxId;

                }
                var result = wechat.ModifyContactLabelList(obj.WxId, userLabels);
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
                Message message = new Message("WECHAT_BATCH_UPDATE_LABEL_NAME_WATCH_TOPIC", buffer);
                producer.SendMessage(message);
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = ex.Message;

                var buffer = Encoding.UTF8.GetBytes(response.ToJson());
                Message message = new Message("WECHAT_BATCH_UPDATE_LABEL_NAME_WATCH_TOPIC", buffer);
                producer.SendMessage(message);
            }
        }
    }
}
