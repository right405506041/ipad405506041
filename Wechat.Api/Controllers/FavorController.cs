using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using Wechat.Api.Abstracts;
using Wechat.Api.Extensions;
using Wechat.Api.Request.Common;
using Wechat.Api.Request.Favor;
using Wechat.Protocol;
using Wechat.Util.Exceptions;

namespace Wechat.Api.Controllers
{
    /// <summary>
    /// 收藏
    /// </summary>
    public class FavorController : WebchatControllerBase
    {
        /// <summary>
        /// 同步收藏
        /// </summary>
        /// <param name="favSync"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("api/Favor/FavSync")]
        public async Task<HttpResponseMessage> FavSync(FavSync favSync)
        {
            ResponseBase<micromsg.FavSyncResponse, IList<micromsg.AddFavItem>> response = new ResponseBase<micromsg.FavSyncResponse, IList<micromsg.AddFavItem>>();


            var result = wechat.FavSync(favSync.WxId, favSync.KeyBuf);
            response.Data = result;
            response.Result = wechat.ToAddFavItem(result);

            return await response.ToHttpResponseAsync();
        }

        /// <summary>
        /// 获取单条收藏
        /// </summary>
        /// <param name="wxId">微信Id</param>
        /// <param name="favId">收藏Id</param>
        /// <returns></returns>
        [HttpPost]
        [Route("api/Favor/GetFavItem/{wxId}/{favId}")]
        public async Task<HttpResponseMessage> GetFavItem(string wxId, int favId)
        {
            ResponseBase<IList<micromsg.FavObject>> response = new ResponseBase<IList<micromsg.FavObject>>();

            var result = wechat.GetFavItem(wxId, favId);
            if (result == null || result.BaseResponse.Ret != (int)MMPro.MM.RetConst.MM_OK)
            {
                response.Success = false;
                response.Code = "402";
                response.Message = result.BaseResponse.ErrMsg.String ?? "未找到";
            }
            else
            {
                response.Data = result.ObjectList;

            }

            return await response.ToHttpResponseAsync();
        }

        /// <summary>
        /// 删除收藏
        /// </summary>
        /// <param name="delFavItem"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("api/Favor/DelFavItem")]
        public async Task<HttpResponseMessage> DelFavItem(DelFavItem delFavItem)
        {
            ResponseBase<IList<micromsg.DelFavItemRsp>> response = new ResponseBase<IList<micromsg.DelFavItemRsp>>();

            var result = wechat.DelFavItem(delFavItem.WxId, delFavItem.favIds);
            if (result == null || result.BaseResponse.Ret != (int)MMPro.MM.RetConst.MM_OK)
            {
                response.Success = false;
                response.Code = "402";
                response.Message = result.BaseResponse.ErrMsg.String ?? "删除失败";
            }
            else
            {
                response.Data = result.List;

            }

            return await response.ToHttpResponseAsync();
        }

        /// <summary>
        /// 添加收藏
        /// </summary>
        /// <param name="addFavItem"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("api/Favor/AddFavItem")]
        public async Task<HttpResponseMessage> AddFavItem(AddFavItem addFavItem)
        {
            ResponseBase<uint> response = new ResponseBase<uint>();

            var result = wechat.addFavItem(addFavItem.WxId, addFavItem.Object, addFavItem.SourceId);
            if (result == null || result.BaseResponse.Ret != (int)MMPro.MM.RetConst.MM_OK)
            {
                response.Success = false;
                response.Code = "402";
                response.Message = result.BaseResponse.ErrMsg.String ?? "添加失败";
            }
            else
            {
                response.Data = result.FavId;

            }

            return await response.ToHttpResponseAsync();
        }
    }
}