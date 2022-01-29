using org.apache.rocketmq.client.producer;
using org.apache.rocketmq.common.message;
using Quartz;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using Wechat.Protocol;
using Wechat.Util.Cache;
using Wechat.Util.Exceptions;
using Wechat.Util.Extensions;
using Wechat.Util.Mq;

namespace Wechat.Api.Helper
{
    /// <summary>
    /// 同步消息作业
    /// </summary>
    public class SyncMessageJob : IJob
    {
        static WechatHelper wechatHelper = new WechatHelper();
        static RedisCache cache = RedisCache.CreateInstance();
        static DefaultMQProducer producer = RocketMqHelper.CreateDefaultMQProducer(MqConst.SyncMessageProducerGroup);

        static object lockObj = new object();

        private static IList<string> LogoutWxIds = new List<string>();

        public Task Execute(IJobExecutionContext context)
        {
            
            var customerInfoCaches = cache.HashGetAll<CustomerInfoCache>(ConstCacheKey.GetWxIdKey());
            foreach (var item in customerInfoCaches)
            {
                try
                {
                    if (LogoutWxIds.Contains(item.WxId))
                    {
                        continue;
                    }

                    var result = wechatHelper.SyncInit(item.WxId);

                    if (result.AddMsgs != null && result.AddMsgs.Count > 0)
                    {
                        SyncMessageResult syncMessageResult = new SyncMessageResult();
                        syncMessageResult.WxId = item.WxId;
                        syncMessageResult.Data = result;
                        var resultJson = syncMessageResult.ToJson();
                        var buffer = Encoding.UTF8.GetBytes(resultJson);
                        Message message = new Message(MqConst.SyncMessageTopic, buffer);
                        var sendResult = producer.SendMessage(message);
                    }

                }
                catch (ExpiredException)
                {
                    lock (lockObj)
                    {
                        if (!LogoutWxIds.Contains(item.WxId))
                        {
                            LogoutWxIds.Add(item.WxId);
                        }
                    }                   
           
                }
                catch (Exception ex)
                {
                    Util.Log.Logger.GetLog<SyncMessageJob>().Error(ex);
                }


            }

            return Task.CompletedTask;
        }

        /// <summary>
        /// 推送消息
        /// </summary>
        public class SyncMessageResult
        {
            public string WxId { get; set; }

            public InitResponse Data { get; set; }
        }
    }
}