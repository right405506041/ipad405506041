using org.apache.rocketmq.client.producer;
using org.apache.rocketmq.common.message;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wechat.Task.App.MessageListeners.Models;
using Wechat.Task.App.Models.Request.Friend;
using Wechat.Util.Extensions;
using Wechat.Util.Mq;

namespace Wechat.Task.App.MessageListeners
{
    /// <summary>
    /// 搜索微信用户信息
    /// </summary>
    public class SearchContractListener : MessageListenerConcurrentlyBase<SearchContract>
    {
        private DefaultMQProducer producer = RocketMqHelper.CreateDefaultMQProducer("WECHAT_SEARCH_CONTRACT_WATCH_PG");
        protected override void Invoke(org.apache.rocketmq.common.message.MessageClientExt messageClientExt,SearchContract obj)
        {
            ResponseBase<MMPro.MM.SearchContactResponse> response = new ResponseBase<MMPro.MM.SearchContactResponse>(obj.MqId);

            try
            {
                var result = wechat.SearchContact(obj.WxId, obj.SearchWxName);
                if (result == null || result.baseResponse.ret != (int)MMPro.MM.RetConst.MM_OK)
                {
                    response.Success = false;
                    response.Message = result.baseResponse.errMsg.@string ?? "获取失败";
                }
                else
                {
                    response.Data = result;
                }


                var buffer = Encoding.UTF8.GetBytes(response.ToJson());
                Message message = new Message("WECHAT_SEARCH_CONTRACT_WATCH_TOPIC", buffer);
                producer.SendMessage(message);
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = ex.Message;

                var buffer = Encoding.UTF8.GetBytes(response.ToJson());
                Message message = new Message("WECHAT_SEARCH_CONTRACT_WATCH_TOPIC", buffer);
                producer.SendMessage(message);
            }
        }
    }
}
