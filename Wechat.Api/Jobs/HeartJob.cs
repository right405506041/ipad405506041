using Quartz;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using Wechat.Protocol;
using Wechat.Util.Exceptions;
using Wechat.Util.Extensions;

namespace Wechat.Api.Jobs
{
    public class HeartJob : IJob
    {
        public Task Execute(IJobExecutionContext context)
        {
            try
            {
                var wxId = context.MergedJobDataMap["wxId"];
                WechatHelper wechatHelper = new WechatHelper();
                var result = wechatHelper.HeartBeat(wxId.ToString());

                Util.Log.Logger.GetLog<HeartJob>().Info($"心跳检测---{result.ToJson()}");

            }
            catch (ExpiredException ex)
            {
               
                Util.Log.Logger.GetLog<HeartJob>().Error($"心跳检测 ", ex);
            }
            catch (Exception ex)
            {
                Util.Log.Logger.GetLog<HeartJob>().Error($"心跳检测 ", ex);
            }
            return Task.CompletedTask;
        }
    }
}