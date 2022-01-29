using java.util;
using org.apache.rocketmq.client.consumer.listener;
using org.apache.rocketmq.client.producer;
using org.apache.rocketmq.common.message;
using System;
using System.IO;
using System.Text;
using System.Threading;
using Wechat.Protocol;
using Wechat.Task.App.MessageListeners.Models;
using Wechat.Util;
using Wechat.Util.Cache;
using Wechat.Util.Extensions;
using Wechat.Util.FileStore;
using Wechat.Util.Mq;
using Wechat.Util.ProcessInfos;

namespace Wechat.Task.App.MessageListeners
{
    /// <summary>
    /// 上传oss
    /// </summary>
    public class UploadOssMessageListener : MessageListenerConcurrentlyBase<UploadFileObj>
    {
        private static string tempPath = ".temp";

        private static string ffmpegPath = "tools/ffmpeg.exe";
        private static string silk_v3_decoderPath = "tools/silk_v3_decoder.exe";
        private static string silk_v3_encoderPath = "tools/silk_v3_encoder.exe";

        private DefaultMQProducer producer = RocketMqHelper.CreateDefaultMQProducer("WECHAT_UPLOAD_FILE_TO_OSS_WATCH_PG");
        protected override void Invoke(org.apache.rocketmq.common.message.MessageClientExt messageClientExt, UploadFileObj obj)
        {
            var uploadFileObj = obj;
            string objName = null;
            string mchId = RedisCache.CreateInstance().Get(ConstCacheKey.GetMchIdKey(uploadFileObj.WxId));
            if (string.IsNullOrEmpty(mchId))
            {
                Util.Log.Logger.GetLog(this.GetType()).Error($"{uploadFileObj.WxId}未初始化商户号");
                invokeWatch(false, uploadFileObj, $"{uploadFileObj.WxId}未初始化商户号");
                return;
            }
            WechatHelper wechatHelper = new WechatHelper();
            //图片
            if (uploadFileObj.MsgType == 3)
            {
                var length = uploadFileObj.LongDataLength == 0 ? uploadFileObj.Length : uploadFileObj.LongDataLength;

                byte[] buffer = wechatHelper.GetMsgBigImg(length, uploadFileObj.MsgId, uploadFileObj.WxId, uploadFileObj.ToWxId, 0, (int)length);

                if (buffer != null)
                {                   
                    objName = FileStorageHelper.GetObjectName(mchId);
                    FileStorageHelper.Upload(buffer, $"{objName}{uploadFileObj.MsgId}_{uploadFileObj.NewMsgId}.png");
                  
                }
                else
                {
                    objName = FileStorageHelper.GetObjectName(mchId);
                    FileStorageHelper.Upload(uploadFileObj.Buffer, $"{objName}{uploadFileObj.MsgId}_{uploadFileObj.NewMsgId}.png");                 
                }
            }
            //语音
            else if (uploadFileObj.MsgType == 34)
            {
                if (uploadFileObj.Buffer != null)
                {

                    objName = FileStorageHelper.GetObjectName(mchId);
                    string fileName = $"{uploadFileObj.MsgId}_{uploadFileObj.NewMsgId}";
                    var tmp_silk_path = Path.Combine(tempPath, $"{fileName}.silk");
                    var tmp_pcm_path = Path.Combine(tempPath, $"{fileName}.pcm");
                    var tmp_mp3_path = Path.Combine(tempPath, $"{fileName}.mp3");
                    if (!Directory.Exists(tempPath))
                    {
                        Directory.CreateDirectory(tempPath);
                    }
                    using (var fs = File.Create(tmp_silk_path, uploadFileObj.Buffer.Length))
                    {
                        fs.Write(uploadFileObj.Buffer, 0, uploadFileObj.Buffer.Length);
                    }

                    if (File.Exists(tmp_silk_path))
                    {
                        var decoderResult = ProcessHelper.Excute(silk_v3_decoderPath, $"\"{tmp_silk_path}\" \"{tmp_pcm_path}\" -quiet");

                        if (string.IsNullOrEmpty(decoderResult.Item2))
                        {
                            Util.Log.Logger.GetLog<UploadOssMessageListener>().Error($"{fileName}文件解码失败");
                        }

                        File.Delete(tmp_silk_path);

                        if (File.Exists(tmp_pcm_path))
                        {
                            if (!File.Exists(tmp_mp3_path))
                            {
                                var ffmpegReuslt = ProcessHelper.Excute(ffmpegPath, $"-y -f s16le -ar 24000 -ac 1 -i \"{tmp_pcm_path}\" \"{tmp_mp3_path}\"");

                                if (string.IsNullOrEmpty(decoderResult.Item2))
                                {
                                    Util.Log.Logger.GetLog<UploadOssMessageListener>().Error($"{fileName}文件转换失败");
                                }

                                File.Delete(tmp_pcm_path);
                            }

                            if (File.Exists(tmp_mp3_path))
                            {
                                using (var fs = File.OpenRead(tmp_mp3_path))
                                {
                                    FileStorageHelper.Upload(fs, $"{objName}{uploadFileObj.MsgId}_{uploadFileObj.NewMsgId}.mp3");
                                }

                                File.Delete(tmp_mp3_path);
                            }
                        }
                        else
                        {
                            Util.Log.Logger.GetLog(this.GetType()).Error($"{uploadFileObj.ToJson()}转换pcm文件不存在");
                            invokeWatch(false, uploadFileObj, "转换pcm文件不存在");
                            return;
                        }
                    }
                    else
                    {
                        Util.Log.Logger.GetLog(this.GetType()).Error($"{uploadFileObj.ToJson()}silk文件不存在");
                        invokeWatch(false, uploadFileObj, "silk文件不存在");
                        return;
                    }



                }
                else
                {
                    Util.Log.Logger.GetLog(this.GetType()).Error($"{uploadFileObj.ToJson()}语音不存在");
                    invokeWatch(false, uploadFileObj, "语音不存在");
                    return;
                }


            }
            //视频
            else if (uploadFileObj.MsgType == 43)
            {
                byte[] buffer = wechatHelper.GetVideo(uploadFileObj.WxId, uploadFileObj.ToWxId, uploadFileObj.MsgId, uploadFileObj.LongDataLength, 0, (int)uploadFileObj.LongDataLength);

                if (buffer != null)
                {
                    objName = FileStorageHelper.GetObjectName(mchId);
                    FileStorageHelper.Upload(buffer, $"{objName}{uploadFileObj.MsgId}_{uploadFileObj.NewMsgId}.mp4");
                }
                else
                {
                    buffer = wechatHelper.GetVideo(uploadFileObj.WxId, uploadFileObj.ToWxId, uploadFileObj.MsgId, uploadFileObj.LongDataLength, 0, (int)uploadFileObj.LongDataLength);

                    if (buffer != null)
                    {
                        objName = FileStorageHelper.GetObjectName(mchId);
                        FileStorageHelper.Upload(buffer, $"{objName}{uploadFileObj.MsgId}_{uploadFileObj.NewMsgId}.mp4");
                    }
                    else
                    {
                        Util.Log.Logger.GetLog(this.GetType()).Error($"{uploadFileObj.ToJson()}上传失败");
                        invokeWatch(false, uploadFileObj, "上传失败");
                        return;
                    }
                }


            }
            invokeWatch(true, uploadFileObj);
        }


        protected override void ExceptionInvoke(UploadFileObj uploadFileObj, Exception ex)
        {
            invokeWatch(false, uploadFileObj, ex.Message);
        }

        private void invokeWatch(bool sucess, UploadFileObj uploadFileObj, string msg = null)
        {
            if (uploadFileObj != null)
            {
                ResponseBase<UploadOssMessageWatch> response = new ResponseBase<UploadOssMessageWatch>(null);
                response.Success = sucess;
                response.Message = msg;
                UploadOssMessageWatch uploadOssMessageWatch = new UploadOssMessageWatch();
                uploadOssMessageWatch.WxId = uploadFileObj.WxId;
                uploadOssMessageWatch.ToWxId = uploadFileObj.ToWxId;
                uploadOssMessageWatch.MsgId = uploadFileObj.MsgId;
                uploadOssMessageWatch.NewMsgId = uploadFileObj.NewMsgId;
                response.Data = uploadOssMessageWatch;
                var buffer = Encoding.UTF8.GetBytes(response.ToJson());
                Message message = new Message("WECHAT_UPLOAD_FILE_TO_OSS_WATCH_TOPIC", buffer);
                producer.SendMessage(message);

            }
        }

        public class UploadOssMessageWatch
        {
            public string WxId { get; set; }

            public string ToWxId { get; set; }

            public int MsgId { get; set; }

            public long NewMsgId { get; set; }

        }
    }
}
