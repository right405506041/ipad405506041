using System.Web.Mvc;

namespace Wechat.Api.Areas.ZombieFan
{
    public class ZombieFanAreaRegistration : AreaRegistration 
    {
        public override string AreaName 
        {
            get 
            {
                return "ZombieFan";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context) 
        {
            context.MapRoute(
                "ZombieFan_default",
                "ZombieFan/{controller}/{action}/{id}",
                new { action = "Index", id = UrlParameter.Optional }
            );
        }
    }
}