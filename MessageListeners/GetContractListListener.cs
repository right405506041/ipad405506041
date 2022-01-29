using org.apache.rocketmq.client.producer;
using org.apache.rocketmq.common.message;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wechat.Task.App.MessageListeners.Models;
using Wechat.Task.App.MessageListeners.Models.Response.Friend;
using Wechat.Task.App.Models;
using Wechat.Task.App.Models.Request.Friend;
using Wechat.Util.Extensions;
using Wechat.Util.Mq;

namespace Wechat.Task.App.MessageListeners
{

    /// <summary>
    /// 获取联系人列表
    /// </summary>
    public class GetContractListListener : MessageListenerConcurrentlyBase<GetContractList>
    {
        private DefaultMQProducer producer = RocketMqHelper.CreateDefaultMQProducer("WECHAT_GET_CONTRACT_LIST_WATCH_PG");
        protected override void Invoke(org.apache.rocketmq.common.message.MessageClientExt messageClientExt,GetContractList obj)
        {
            ResponseBase<ContractListResponse> response = new ResponseBase<ContractListResponse>(obj.MqId);

            try
            {
                var result = wechat.InitContact(obj.WxId, obj.CurrentWxcontactSeq);
                if (result == null || result.baseResponse.ret != (int)MMPro.MM.RetConst.MM_OK)
                {
                    response.Success = false;
                    response.Message = result.baseResponse.errMsg.@string ?? "获取失败";

                }
                else
                {
                    ContractListResponse contractResponse = new ContractListResponse();
                    contractResponse.Contracts = result.contactUsernameList;
                    contractResponse.CurrentWxcontactSeq = result.currentWxcontactSeq;
                    response.Data = contractResponse;
                }
                var buffer = Encoding.UTF8.GetBytes(response.ToJson());
                Message message = new Message("WECHAT_GET_CONTRACT_LIST_WATCH_TOPIC", buffer);
                producer.SendMessage(message);
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = ex.Message;

                var buffer = Encoding.UTF8.GetBytes(response.ToJson());
                Message message = new Message("WECHAT_GET_CONTRACT_LIST_WATCH_TOPIC", buffer);
                producer.SendMessage(message);
            }
        }
    }


}
