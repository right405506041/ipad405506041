using org.apache.rocketmq.client.producer;
using org.apache.rocketmq.common.message;
using System.Linq;
using System.Text;
using Wechat.Task.App.MessageListeners.Models;
using Wechat.Task.App.Models.Request.Friend;
using Wechat.Util.Extensions;
using Wechat.Util.Mq;
using System;
using System.Collections.Generic;

namespace Wechat.Task.App.MessageListeners
{
    /// <summary>
    /// 获取好友详情
    /// </summary>
    public class GetContractDetailListener : MessageListenerConcurrentlyBase<GetContractDetail>
    {
        private DefaultMQProducer producer = RocketMqHelper.CreateDefaultMQProducer("WECHAT_GET_CONTRACT_DETAIL_WATCH_PG");
        protected override void Invoke(org.apache.rocketmq.common.message.MessageClientExt messageClientExt,GetContractDetail obj)
        {
            ResponseBase<IList<micromsg.ModContact>> response = new ResponseBase<IList<micromsg.ModContact>>(obj.MqId);

            try
            {
                var result = wechat.GetContactDetail(obj.WxId, obj.SearchWxIds);
                if (result == null || result.BaseResponse.Ret != (int)MMPro.MM.RetConst.MM_OK)
                {
                    response.Success = false;
                    response.Message = result.BaseResponse.ErrMsg.String ?? "获取失败";
                }
                else
                {
                    response.Data = result.ContactList;
                }
                var buffer = Encoding.UTF8.GetBytes(response.ToJson());
                Message message = new Message("WECHAT_GET_CONTRACT_DETAIL_WATCH_TOPIC", buffer);
                producer.SendMessage(message);
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = ex.Message;

                var buffer = Encoding.UTF8.GetBytes(response.ToJson());
                Message message = new Message("WECHAT_GET_CONTRACT_DETAIL_WATCH_TOPIC", buffer);
                producer.SendMessage(message);
            }
        }
    }
}
