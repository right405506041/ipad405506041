using Wechat.Api.Abstracts;

namespace Wechat.Api.Controllers
{
    /// <summary>
    /// 首页
    /// </summary>
    public class IndexController : WebchatControllerBase
    {
        /// <summary>
        /// 首页
        /// </summary>
        /// <returns></returns>
        public string Get()
        { 
            return $"微信接口7.0.14";
        }

    }
}
