using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Wechat.Api.Response.Common
{
    public class ReadInfoResponse
    {

        public string Article { get; set; }

        public string GhId { get; set; }


        public IList<string> advertisement_info { get; set; }

        public appmsgstat appmsgstat { get; set; }

        public int comment_enabled { get; set; }

        public IList<string> reward_head_imgs { get; set; }

        public bool only_fans_can_comment { get; set; }

        public int comment_count { get; set; }

        public int is_fans { get; set; }

        public string nick_name { get; set; }

        public string logo_url { get; set; }

        public int friend_comment_enabled { get; set; }

        public base_resp base_resp { get; set; }
    }

    public class base_resp
    {
        public int wxtoken { get; set; }

    }
    public class appmsgstat
    {
        public bool show { get; set; }
        public bool is_login { get; set; }

        public bool liked { get; set; }

        public int read_num { get; set; }

        public int like_num { get; set; }

        public int ret { get; set; }

        public int real_read_num { get; set; }

        public int version { get; set; }

        public int prompted { get; set; }

        public bool like_disabled { get; set; }

        public int style { get; set; }

    }
}