//using System;
//using System.Collections.Generic;
//using System.IO;
//using System.Linq;
//using System.Net.Http;
//using System.Text;
//using System.Threading.Tasks;
//using System.Web;
//using System.Web.Http;
//using Wechat.Api.Extensions;

//namespace Wechat.Api.Controllers
//{
//    /// <summary>
//    /// 日志
//    /// </summary>
//    public class LogController : ApiController
//    {
//        /// <summary>
//        /// 获取日志
//        /// </summary>
//        /// <returns></returns>
//        [HttpGet()]
//        [Route("api/log/logs")]
//        public Task<HttpResponseMessage> Logs()
//        {

//            string path = Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, "logs");
//            var logFiles = Directory.GetFiles(path, "*.log");
//            StringBuilder sb = new StringBuilder();
//            foreach (var item in logFiles.Reverse())
//            {
//                sb.Append($"{Path.GetFileName(item)}\r\n");

//            }
//            Dictionary<string, string> header = new Dictionary<string, string>();
//            header.Add("Title", $"Port {Request.RequestUri.Port}");
//            return sb.ToString().ToHttpResponseAsync(header);

//        }

//        /// <summary>
//        /// 获取日志
//        /// </summary>
//        /// <param name="fileName"></param>
//        /// <returns></returns>
//        [HttpGet()]
//        [Route("api/log/logs/{fileName}")]
//        public Task<HttpResponseMessage> Log(string fileName)
//        {

//            StringBuilder result = new StringBuilder();

//            string path = Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, "logs");
//            var files = Directory.GetFiles(path, $"{fileName}.log*");
//            if (files != null && files.Count() > 0)
//            {
//                foreach (var item in files)
//                {
//                    result.Append(File.ReadAllText(item, Encoding.Default));
//                }
//            }
//            else
//            {
//                result.Append("未生成日志");
//            }
//            Dictionary<string, string> header = new Dictionary<string, string>();
//            header.Add("Title", $"Port {Request.RequestUri.Port}");
//            return result.ToString().ToHttpResponseAsync(header);
//        }
//    }
//}