using java.util;
using org.apache.rocketmq.client.consumer.listener;
using org.apache.rocketmq.client.producer;
using org.apache.rocketmq.common.message;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using Wechat.Protocol;
using Wechat.Task.App.MessageListeners.Models;
using Wechat.Task.App.MessageListeners.Models.Response.Common;
using Wechat.Util.Exceptions;
using Wechat.Util.Extensions;
using Wechat.Util.Mq;

namespace Wechat.Task.App.MessageListeners
{
    /// <summary>
    /// 同步消息
    /// </summary>
    public class SyncMessageListener : MessageListenerConcurrentlyBase
    {

        private DefaultMQProducer producer = RocketMqHelper.CreateDefaultMQProducer(MqConst.SyncMessageProducerGroup);
        private DefaultMQProducer userproducer = RocketMqHelper.CreateDefaultMQProducer(MqConst.UserSyncMessageProducerGroup);
        private DefaultMQProducer offlineproducer = RocketMqHelper.CreateDefaultMQProducer(MqConst.UserOfflineStatusProducerGroup);

        private int maxCount = 3;
        private static ConcurrentDictionary<string, int> Dic = new ConcurrentDictionary<string, int>();

        private SendResult sendResult = null;

        private IList<string> wxList = new List<string>();

        protected override void BeforeInvoke()
        {
            sendResult = null;
            wxList.Clear();
        }

        protected override void Invoke(org.apache.rocketmq.common.message.MessageClientExt messageClientExt, string str)
        {
            string wxId = str;


            if (wxList.Contains(wxId))
            {
                Util.Log.Logger.GetLog<SyncMessageListener>().Info("移除一个同步消息，存在多个" + wxId);
                return;
            }
            wxList.Add(wxId);
            byte[] buffer = Encoding.UTF8.GetBytes(wxId);
            try
            {

                var result = wechatHelper.SyncInit(wxId);
                
                if (result.ModUserInfos?.Count > 0 || result.AddMsgs?.Count > 0 || result.DelContacts?.Count > 0 || result.AddMsgs?.Count > 0
                    || result.ModMsgStatuss?.Count > 0 || result.DelChatContacts?.Count > 0 || result.DelContactMsgs?.Count > 0 || result.DelMsgs?.Count > 0
                    || result.Reports?.Count > 0 || result.OpenQQMicroBlogs?.Count > 0 || result.CloseMicroBlogs?.Count > 0 || result.InviteFriendOpens?.Count > 0
                    || result.ModNotifyStatuss?.Count > 0 || result.ModChatRoomMembers?.Count > 0 || result.QuitChatRooms?.Count > 0 || result.ModUserDomainEmails?.Count > 0
                    || result.DelUserDomainEmails?.Count > 0 || result.ModChatRoomNotifys?.Count > 0 || result.PossibleFriends?.Count > 0 || result.FunctionSwitchs?.Count > 0
                    || result.QContacts?.Count > 0 || result.TContacts?.Count > 0 || result.PSMStats?.Count > 0 || result.ModChatRoomTopics?.Count > 0
                    || result.UpdateStatOpLogs?.Count > 0 || result.ModDisturbSettings?.Count > 0 || result.ModBottleContacts?.Count > 0 || result.DelBottleContacts?.Count > 0
                    || result.ModUserImgs?.Count > 0 || result.ModDisturbSetting?.Count > 0 || result.KVStatItems?.Count > 0 || result.ThemeOpLogs?.Count > 0
                    || result.UserInfoExts?.Count > 0 || result.SnsObjects?.Count > 0 || result.SnsActionGroups?.Count > 0 || result.ModBrandSettings?.Count > 0
                    || result.ModChatRoomMemberDisplayNames?.Count > 0 || result.ModChatRoomMemberFlags?.Count > 0 || result.WebWxFunctionSwitchs?.Count > 0 || result.ModSnsBlackLists?.Count > 0
                    || result.NewDelMsgs?.Count > 0 || result.ModDescriptions?.Count > 0 || result.KVCmds?.Count > 0 || result.DeleteSnsOldGroups?.Count > 0)
                {
                    //Util.Log.Logger.GetLog<SyncMessageListener>().Info($"同步消息-----{wxId}----{result.ToJson()}");
                    //if (result.ToJson().Length > 900)
                    //{
                    SyncMessageResponse syncMessageResult = new SyncMessageResponse();
                    syncMessageResult.WxId = wxId;
                    syncMessageResult.Data = result;
                    var resultJson = syncMessageResult.ToJson();
                    var dataBuffer = Encoding.UTF8.GetBytes(resultJson);
                    Message message = new Message(MqConst.SyncMessageTopic, dataBuffer);
                    var sendResult = producer.SendMessage(message);
                }
                OfflineStatus offlineStatus = new OfflineStatus()
                {
                    WxId = wxId,
                    Status = 0
                };
                offlineproducer.SendMessage(new Message(MqConst.UserOfflineStatusTopic, Encoding.UTF8.GetBytes(offlineStatus.ToJson())));

                var userMessage = new Message(MqConst.UserSyncMessageTopic, buffer);
                //2表示5秒
                sendResult = userproducer.SendMessage(userMessage, 2);

            }
            catch (ExpiredException ex)
            {
                if (!string.IsNullOrEmpty(wxId))
                {
                    OfflineStatus offlineStatus = new OfflineStatus()
                    {
                        WxId = wxId,
                        Status = 2
                    };
                    offlineproducer.SendMessage(new Message(MqConst.UserOfflineStatusTopic, Encoding.UTF8.GetBytes(offlineStatus.ToJson())));
                }
                if (Dic.ContainsKey(wxId))
                {
                    if (Dic[wxId] < maxCount)
                    {
                        Dic[wxId]++;
                        var userMessage = new Message(MqConst.UserSyncMessageTopic, buffer);
                        userproducer.SendMessage(userMessage, 2);
                        Util.Log.Logger.GetLog<SyncMessageListener>().Warn($"{wxId}---重试次数{ Dic[wxId] }", ex);
                    }
                    else
                    {
                        Dic[wxId] = 0;
                        Util.Log.Logger.GetLog<SyncMessageListener>().Error($"{wxId}---移除同步消息", ex);
                    }
                }
                else
                {
                    Dic.TryAdd(wxId, 1);
                    var userMessage = new Message(MqConst.UserSyncMessageTopic, buffer);
                    userproducer.SendMessage(userMessage, 2);
                    Util.Log.Logger.GetLog<SyncMessageListener>().Warn($"{wxId}---重试次数{ Dic[wxId] }", ex);

                    if (!string.IsNullOrEmpty(wxId))
                    {
                        OfflineStatus offlineStatus = new OfflineStatus()
                        {
                            WxId = wxId,
                            Status = 2
                        };
                        offlineproducer.SendMessage(new Message(MqConst.UserOfflineStatusTopic, Encoding.UTF8.GetBytes(offlineStatus.ToJson())));
                    }
                }




            }
            catch (Exception ex)
            {
                if (sendResult == null)
                {
                    var userMessage = new Message(MqConst.UserSyncMessageTopic, buffer);
                    userproducer.SendMessage(userMessage, 3);
                }
                Util.Log.Logger.GetLog<SyncMessageListener>().Error(wxId, ex);

            }
        }
    }

    public class OfflineStatus
    {
        /// <summary>
        /// 微信ID
        /// </summary>
        public string WxId { get; set; }

        /// <summary>
        /// 0 在线 2：下线
        /// </summary>
        public int Status { get; set; }
    }
}

