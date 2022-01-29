using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using Wechat.Api.Abstracts;
using Wechat.Api.Extensions;
using Wechat.Api.Filters;
using Wechat.Api.Request.Group;
using Wechat.Api.Request.Message;
using Wechat.Protocol;
using Wechat.Util.Exceptions;
using Wechat.Util.Extensions;
using Wechat.Util.QrCode;
using static MMPro.MM;

namespace Wechat.Api.Controllers
{
    /// <summary>
    /// 群
    /// </summary>
    public class GroupController : WebchatControllerBase
    {

        /// <summary>
        /// 扫码进群
        /// </summary>
        /// <param name="scanIntoGroup"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("api/Group/ScanIntoGroup")]
        public async Task<HttpResponseMessage> ScanIntoGroup(ScanIntoGroup scanIntoGroup)
        {
            ResponseBase<string> response = new ResponseBase<string>();

            var result = wechat.GetA8KeyGroup(scanIntoGroup.WxId, "", scanIntoGroup.Url);
            if (string.IsNullOrWhiteSpace(result))
            {
                response.Success = false;
                response.Code = "501";
                response.Message = "扫码进群失败";
                return await response.ToHttpResponseAsync();
            }
            else
            {
                response.Data = result;
                response.Message = "扫码进群成功";
            }

            return await response.ToHttpResponseAsync();
        }

        /// <summary>
        /// 群二维码转链接
        /// </summary>
        /// <param name="QrcodeToUrl"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("api/Group/QrcodeToUrl")]
        public async Task<HttpResponseMessage> QrcodeToUrl(QrcodeToUrl qrcodeBase64)
        {
            ResponseBase<string> response = new ResponseBase<string>();
            byte[] buffer = null;
            var arr = qrcodeBase64.Base64.Split(',');
            if (arr.Count() == 2)
            {
                buffer = Convert.FromBase64String(arr[1]);

            }
            else
            {
                buffer = Convert.FromBase64String(qrcodeBase64.Base64);
            }
            var url = QrCodeHelper.DecodeQrCode(new MemoryStream(buffer));
            response.Data = url;
            response.Message = "解码成功";
            return await response.ToHttpResponseAsync();
        }

        /// <summary>
        /// 扫码进群
        /// </summary>
        /// <param name="scanIntoGroupBase64"></param>
        /// <returns></returns>
        [HttpPost]
        [NoRequestLog]
        [Route("api/Group/ScanIntoGroupBase64")]
        public async Task<HttpResponseMessage> ScanIntoGroupBase64(ScanIntoGroupBase64 scanIntoGroupBase64)
        {
            ResponseBase<string> response = new ResponseBase<string>();
            byte[] buffer = null;
            var arr = scanIntoGroupBase64.Base64.Split(',');
            if (arr.Count() == 2)
            {
                buffer = Convert.FromBase64String(arr[1]);

            }
            else
            {
                buffer = Convert.FromBase64String(scanIntoGroupBase64.Base64);
            }
            var url = QrCodeHelper.DecodeQrCode(new MemoryStream(buffer));
            var result = wechat.GetA8KeyGroup(scanIntoGroupBase64.WxId, "", url);
            if (string.IsNullOrWhiteSpace(result))
            {
                response.Success = false;
                response.Code = "501";
                response.Message = "扫码进群失败";
                return await response.ToHttpResponseAsync();
            }
            else
            {
                response.Data = result;
                response.Message = "扫码进群成功";
            }

            return await response.ToHttpResponseAsync();
        }





        /// <summary>
        /// 获取群详情
        /// </summary>
        /// <param name="chatRoomInfoDetail"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("api/Group/GetChatRoomInfoDetail")]
        public async Task<HttpResponseMessage> GetChatRoomInfoDetail(ChatRoomInfoDetail chatRoomInfoDetail)
        {
            ResponseBase<MMPro.MM.GetChatRoomInfoDetailResponse> response = new ResponseBase<MMPro.MM.GetChatRoomInfoDetailResponse>();

            var result = wechat.GetChatRoomInfoDetail(chatRoomInfoDetail.WxId, chatRoomInfoDetail.ChatRoomName);
            if (result == null || result.baseResponse.ret != (int)MMPro.MM.RetConst.MM_OK)
            {
                response.Success = false;
                response.Code = "501";
                response.Message = result.baseResponse.errMsg.@string ?? "获取失败";
            }
            else
            {
                response.Data = result;
            }


            return await response.ToHttpResponseAsync();
        }


        /// <summary>
        /// 创建群
        /// </summary>
        /// <param name="greateGroup"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("api/Group/CreateGroup")]
        public async Task<HttpResponseMessage> CreateGroupAsync(CreateGroup greateGroup)
        {
            ResponseBase<string> response = new ResponseBase<string>();
            if (greateGroup.ToWxIds == null || greateGroup.ToWxIds.Count == 0)
            {
                response.Success = false;
                response.Code = "400";
                response.Message = "用户列表不能为空";
                return await response.ToHttpResponseAsync();
            }

            IList<MMPro.MM.MemberReq> list = new List<MMPro.MM.MemberReq>();
            var memberReqCurrent = new MMPro.MM.MemberReq();
            memberReqCurrent.member = new MMPro.MM.SKBuiltinString();
            memberReqCurrent.member.@string = greateGroup.WxId;
            list.Add(memberReqCurrent);
            foreach (var item in greateGroup.ToWxIds)
            {
                var memberReq = new MMPro.MM.MemberReq();
                memberReq.member = new MMPro.MM.SKBuiltinString();
                memberReq.member.@string = item;
                list.Add(memberReq);
            }
            var result = wechat.CreateChatRoom(greateGroup.WxId, list.ToArray(), greateGroup.GroupName);
            if (result == null || result.baseResponse.ret != (int)MMPro.MM.RetConst.MM_OK)
            {
                response.Success = false;
                response.Code = "501";
                response.Message = result.baseResponse.errMsg.@string ?? "创建失败";
            }
            else
            {
                response.Data = result.chatRoomName.@string;
                response.Message = "创建成功";
            }


            return await response.ToHttpResponseAsync();
        }



        /// <summary>
        /// 面对面建群
        /// </summary>
        /// <param name="facingCreateChatRoom"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("api/Group/FacingCreateChatRoom")]
        public async Task<HttpResponseMessage> FacingCreateChatRoom(FacingCreateChatRoom facingCreateChatRoom)
        {
            ResponseBase<micromsg.FacingCreateChatRoomResponse> response = new ResponseBase<micromsg.FacingCreateChatRoomResponse>();
            var result = wechat.FaceCreateRoom(facingCreateChatRoom.WxId, facingCreateChatRoom.Longitude, facingCreateChatRoom.Latitude, facingCreateChatRoom.PassWord);
            if (result == null || result.BaseResponse.Ret != (int)MMPro.MM.RetConst.MM_OK)
            {
                response.Success = false;
                response.Code = "501";
                response.Message = result.BaseResponse.ErrMsg.String ?? "创建失败";
            }
            else
            {
                response.Data = result;
                response.Message = "创建成功";
            }


            return await response.ToHttpResponseAsync();
        }

        /// <summary>
        /// 添加群成员
        /// </summary>
        /// <param name="addGroupMember"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("api/Group/AddGroupMember")]
        public async Task<HttpResponseMessage> AddGroupMember(GroupMember addGroupMember)
        {
            ResponseBase<MMPro.MM.AddChatRoomMemberResponse> response = new ResponseBase<MMPro.MM.AddChatRoomMemberResponse>();
            if (addGroupMember.ToWxIds == null || addGroupMember.ToWxIds.Count == 0)
            {
                response.Success = false;
                response.Code = "400";
                response.Message = "用户列表不能为空";
                return await response.ToHttpResponseAsync();
            }

            IList<MMPro.MM.MemberReq> list = new List<MMPro.MM.MemberReq>();
            foreach (var item in addGroupMember.ToWxIds)
            {
                var memberReq = new MMPro.MM.MemberReq();
                memberReq.member = new MMPro.MM.SKBuiltinString();
                memberReq.member.@string = item;
                list.Add(memberReq);
            }
            var result = wechat.AddChatRoomMember(addGroupMember.WxId, addGroupMember.ChatRoomName, list.ToArray());
            if (result == null || result.baseResponse.ret != (int)MMPro.MM.RetConst.MM_OK)
            {
                response.Success = false;
                response.Code = "501";
                response.Message = result.baseResponse.errMsg.@string ?? "添加失败";
            }
            else
            {
                response.Message = "添加成功";
                response.Data = result;
            }

            return await response.ToHttpResponseAsync();
        }


        /// <summary>
        /// 邀请群成员
        /// </summary>
        /// <param name="addGroupMember"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("api/Group/InviteChatRoomMember")]
        public async Task<HttpResponseMessage> InviteChatRoomMember(GroupMember addGroupMember)
        {
            ResponseBase<micromsg.InviteChatRoomMemberResponse> response = new ResponseBase<micromsg.InviteChatRoomMemberResponse>();
            if (addGroupMember.ToWxIds == null || addGroupMember.ToWxIds.Count == 0)
            {
                response.Success = false;
                response.Code = "400";
                response.Message = "用户列表不能为空";
                return await response.ToHttpResponseAsync();
            }


            var result = wechat.InviteChatRoomMember(addGroupMember.WxId, addGroupMember.ChatRoomName, addGroupMember.ToWxIds);
            if (result == null || result.BaseResponse.Ret != (int)MMPro.MM.RetConst.MM_OK)
            {
                response.Success = false;
                response.Code = "501";
                response.Message = result.BaseResponse.ErrMsg.String ?? "邀请失败";
            }
            else
            {
                response.Message = "邀请成功";
                response.Data = result;
            }

            return await response.ToHttpResponseAsync();
        }


        /// <summary>
        /// 设置群昵称
        /// </summary>
        /// <param name="setChatRoom"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("api/Group/SetChatRoomName")]
        public Task<HttpResponseMessage> SetChatRoomName(SetChatRoom setChatRoom)
        {
            ResponseBase response = new ResponseBase();
            micromsg.ModChatRoomTopic modChatRoomTopic = new micromsg.ModChatRoomTopic()
            {
                ChatRoomName = new micromsg.SKBuiltinString_t() { String = setChatRoom.ChatRoomName },
                ChatRoomTopic = new micromsg.SKBuiltinString_t() { String = setChatRoom.ChatRoomNickName }
            };
            var result = wechat.OpLog(setChatRoom.WxId, 27, modChatRoomTopic);
            if (result == null || result.Ret != 0 || result.OplogRet.Ret.FirstOrDefault() != 0)
            {
                response.Success = false;
                response.Code = "501";
                response.Message = "修改失败";
            }
            else
            {
                response.Message = "修改成功";
            }
            return response.ToHttpResponseAsync();
        }


        /// <summary>
        /// 设置我在本群昵称
        /// </summary>
        /// <param name="setChatRoomNameDisplyName"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("api/Group/SetChatRoomNameDisplyName")]
        public Task<HttpResponseMessage> SetChatRoomNameDisplyName(SetChatRoomNameDisplyName setChatRoomNameDisplyName)
        {
            ResponseBase response = new ResponseBase();
            micromsg.ModChatRoomMemberDisplayName modChatRoomMemberDisplayName = new micromsg.ModChatRoomMemberDisplayName()
            {
                ChatRoomName = setChatRoomNameDisplyName.ChatRoomName,
                DisplayName = setChatRoomNameDisplyName.DisplayName,
                UserName = setChatRoomNameDisplyName.WxId
            };
            var result = wechat.OpLog(setChatRoomNameDisplyName.WxId, 48, modChatRoomMemberDisplayName);
            if (result == null || result.Ret != 0 || result.OplogRet.Ret.FirstOrDefault() != 0)
            {
                response.Success = false;
                response.Code = "501";
                response.Message = "修改失败";
            }
            else
            {
                response.Message = "修改成功";
            }
            return response.ToHttpResponseAsync();
        }


        /// <summary>
        /// 设置群功能 1显示群成员昵称2：保存通讯录
        /// </summary>
        /// <param name="modChatRoomFunction"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("api/Group/ModChatRoomFunction")]
        public Task<HttpResponseMessage> ModChatRoomFunction(ModChatRoomFunction modChatRoomFunction)
        {

            ResponseBase response = new ResponseBase();
            micromsg.ModChatRoomMemberFlag modChatRoomMemberFlag = new micromsg.ModChatRoomMemberFlag()
            {
                UserName = modChatRoomFunction.WxId,
                ChatRoomName = modChatRoomFunction.ChatRoomName,
                FlagSwitch = modChatRoomFunction.FlagSwitch,
                Value = modChatRoomFunction.Value
            };
            var result = wechat.OpLog(modChatRoomFunction.WxId, 49, modChatRoomMemberFlag);
            if (result == null || result.Ret != 0 || result.OplogRet.Ret.FirstOrDefault() != 0)
            {
                response.Success = false;
                response.Code = "501";
                response.Message = "修改失败";
            }
            else
            {
                response.Message = "修改成功";
            }
            return response.ToHttpResponseAsync();
        }

        /// <summary>
        /// 删除群成员
        /// </summary>
        /// <param name="deleteGroupMember"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("api/Group/DeleteGroupMember")]
        public async Task<HttpResponseMessage> DeleteGroupMember(GroupMember deleteGroupMember)
        {
            ResponseBase response = new ResponseBase();
            if (deleteGroupMember.ToWxIds == null || deleteGroupMember.ToWxIds.Count == 0)
            {
                response.Success = false;
                response.Code = "400";
                response.Message = "用户列表不能为空";
                return await response.ToHttpResponseAsync();
            }

            IList<MMPro.MM.DelMemberReq> list = new List<MMPro.MM.DelMemberReq>();
            foreach (var item in deleteGroupMember.ToWxIds)
            {
                var memberReq = new MMPro.MM.DelMemberReq();
                memberReq.memberName = new MMPro.MM.SKBuiltinString();
                memberReq.memberName.@string = item;
                list.Add(memberReq);
            }
            var result = wechat.DelChatRoomMember(deleteGroupMember.WxId, deleteGroupMember.ChatRoomName, list.ToArray());
            if (result == null || result.baseResponse.ret != (int)MMPro.MM.RetConst.MM_OK)
            {
                response.Success = false;
                response.Code = "501";
                response.Message = result.baseResponse.errMsg.@string ?? "删除失败";
            }
            else
            {
                response.Message = "删除成功";
            }


            return await response.ToHttpResponseAsync();
        }

        /// <summary>
        /// 获取群成员
        /// </summary>
        /// <param name="addGroupMember"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("api/Group/GetGroupMembers")]
        public async Task<HttpResponseMessage> GetGroupMembers(GetGroupMember addGroupMember)
        {
            ResponseBase<MMPro.MM.ChatRoomMemberData> response = new ResponseBase<MMPro.MM.ChatRoomMemberData>();

            var result = wechat.GetChatroomMemberDetail(addGroupMember.WxId, addGroupMember.ChatRoomName);
            if (result == null || result.baseResponse.ret != (int)MMPro.MM.RetConst.MM_OK)
            {
                response.Success = false;
                response.Code = "501";
                response.Message = result.baseResponse.errMsg.@string ?? "获取失败";
            }
            else
            {
                response.Data = result.newChatroomData;
            }


            return await response.ToHttpResponseAsync();
        }



        /// <summary>
        /// 退出群
        /// </summary>
        /// <param name="quitGroup"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("api/Group/QuitGroup")]
        public async Task<HttpResponseMessage> QuitGroup(QuitGroup quitGroup)
        {
            ResponseBase response = new ResponseBase();
            micromsg.QuitChatRoom quitChatRoom = new micromsg.QuitChatRoom()
            {
                UserName = new micromsg.SKBuiltinString_t()
                {
                    String = quitGroup.WxId
                },
                ChatRoomName = new micromsg.SKBuiltinString_t()
                {
                    String = quitGroup.ChatRoomName
                }
            };
            var result = wechat.OpLog(quitGroup.WxId, 16, quitChatRoom);
            //var result = wechat.QuitGroup(quitGroup.WxId, quitGroup.ChatRoomName);
            if (result == null || result.Ret != 0 || result.OplogRet.Ret.FirstOrDefault() != 0)
            {
                response.Success = false;
                response.Code = "501";
                response.Message = "退群失败";
            }
            else
            {
                response.Message = "退出成功";
            }


            return await response.ToHttpResponseAsync();
        }


        /// <summary>
        /// 移交群管理
        /// </summary>
        /// <param name="transferChatRoomOwner"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("api/Group/TransferChatRoomOwner")]
        public async Task<HttpResponseMessage> TransferChatRoomOwner(TransferChatRoomOwner transferChatRoomOwner)
        {
            ResponseBase response = new ResponseBase();

            var result = wechat.transferChatRoomOwner(transferChatRoomOwner.WxId, transferChatRoomOwner.ChatRoomName, transferChatRoomOwner.ToWxId);
            if (result == null || result.BaseResponse.Ret != (int)MMPro.MM.RetConst.MM_OK)
            {
                response.Success = false;
                response.Code = "501";
                response.Message = result.BaseResponse.ErrMsg.String ?? "移交失败";
            }
            else
            {
                response.Message = "移交成功";
            }

            return await response.ToHttpResponseAsync();
        }


        /// <summary>
        /// 获取群公告
        /// </summary>
        /// <param name="getChatRoomAnnouncement"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("api/Group/GetChatRoomAnnouncement")]
        public async Task<HttpResponseMessage> GetChatRoomAnnouncement(GetChatRoomAnnouncement getChatRoomAnnouncement)
        {
            ResponseBase<string> response = new ResponseBase<string>();

            var result = wechat.getChatRoomAnnouncement(getChatRoomAnnouncement.WxId, getChatRoomAnnouncement.ChatRoomName);
            if (result == null || result.BaseResponse.Ret != (int)MMPro.MM.RetConst.MM_OK)
            {
                response.Success = false;
                response.Code = "501";
                response.Message = result.BaseResponse.ErrMsg.String ?? "获取失败";
            }
            else
            {
                response.Data = result.Announcement;
                response.Message = "获取成功";
            }

            return await response.ToHttpResponseAsync();
        }



        /// <summary>
        /// 修改公告
        /// </summary>
        /// <param name="groupAnnouncement"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("api/Group/UpdateGroupAnnouncement")]
        public async Task<HttpResponseMessage> UpdateGroupAnnouncement(GroupAnnouncement groupAnnouncement)
        {
            ResponseBase response = new ResponseBase();

            var result = wechat.setChatRoomAnnouncement(groupAnnouncement.WxId, groupAnnouncement.ChatRoomName, groupAnnouncement.Announcement);
            if (result == null || result.BaseResponse.Ret != (int)MMPro.MM.RetConst.MM_OK)
            {
                response.Success = false;
                response.Code = "501";
                response.Message = result.BaseResponse.ErrMsg.String ?? "修改失败";
            }
            else
            {
                response.Message = "修改成功";
            }

            return await response.ToHttpResponseAsync();
        }
    }
}