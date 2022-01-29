using java.util;
using org.apache.rocketmq.client.consumer.listener;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wechat.Protocol;
using Wechat.Task.App.MessageListeners.Models.Request;
using Wechat.Util;
using Wechat.Util.Exceptions;
using Wechat.Util.Extensions;

namespace Wechat.Task.App.MessageListeners
{
    public abstract class MessageListenerConcurrentlyBase<T> : MessageListenerConcurrently where T : RequestBase
    {
        protected WechatHelper wechat = new WechatHelper();

        public ConsumeConcurrentlyStatus consumeMessage(List list, ConsumeConcurrentlyContext ccc)
        {
            Iterator iterator = list.iterator();
            BeforeInvoke();
            org.apache.rocketmq.common.message.MessageClientExt messageClientExt = null;
            T uploadFileObj = default(T);
            while (iterator.hasNext())
            {
                try
                {
                    messageClientExt = iterator.next() as org.apache.rocketmq.common.message.MessageClientExt;

                    var content = Encoding.UTF8.GetString(messageClientExt.getBody());             
                    uploadFileObj = content.ToObj<T>();
                    Invoke(messageClientExt, uploadFileObj);
                    AfterInvoke(uploadFileObj);
                    Util.Log.Logger.GetLog(this.GetType()).Info(content);
                }
                catch (Exception ex)
                {
                    ExceptionInvoke(uploadFileObj, ex);
                    Util.Log.Logger.GetLog(this.GetType()).Error($"{uploadFileObj}", ex);
                }
            }
            return ConsumeConcurrentlyStatus.CONSUME_SUCCESS;
        }

        protected abstract void Invoke(org.apache.rocketmq.common.message.MessageClientExt messageClientExt, T obj);
        protected virtual void BeforeInvoke()
        {

        }
        protected virtual void AfterInvoke(T uploadFileObj)
        {

        }

        protected virtual void ExceptionInvoke(T uploadFileObj, Exception ex)
        {

        }
    }

    public abstract class MessageListenerConcurrentlyBase : MessageListenerConcurrently
    {
        protected WechatHelper wechatHelper = new WechatHelper();

        public ConsumeConcurrentlyStatus consumeMessage(List list, ConsumeConcurrentlyContext ccc)
        {
            Iterator iterator = list.iterator();
            BeforeInvoke();
            string content = null;
            while (iterator.hasNext())
            {
                try
                {
                    var messageClientExt = iterator.next() as org.apache.rocketmq.common.message.MessageClientExt;

                    content = Encoding.UTF8.GetString(messageClientExt.getBody());                  
                    Invoke(messageClientExt, content);
                    Util.Log.Logger.GetLog(this.GetType()).Info(content);
                }
                catch (Exception ex)
                {
                    ExceptionInvoke(content, ex);
                    Util.Log.Logger.GetLog(this.GetType()).Error(ex);
                    return ConsumeConcurrentlyStatus.RECONSUME_LATER;
                }
            }

            return ConsumeConcurrentlyStatus.CONSUME_SUCCESS;
        }


        protected virtual void BeforeInvoke()
        {

        }
         
        protected virtual void ExceptionInvoke(string content, Exception ex)
        {

        }
        protected abstract void Invoke(org.apache.rocketmq.common.message.MessageClientExt messageClientExt, string str);

    }

}
