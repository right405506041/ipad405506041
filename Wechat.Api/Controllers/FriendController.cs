using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;
using Wechat.Api.Abstracts;
using Wechat.Api.Extensions;
using Wechat.Api.Filters;
using Wechat.Api.Request.Common;
using Wechat.Api.Request.Friend;
using Wechat.Api.Request.Login;
using Wechat.Api.Response.Friend;
using Wechat.Protocol;

namespace Wechat.Api.Controllers
{
    /// <summary>
    /// 好友
    /// </summary>
    public class FriendController : WebchatControllerBase
    {

        /// <summary>
        /// 获取单页好友列表（只包含wxid）
        /// </summary>
        /// <param name="contractList"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("api/Friend/GetContractList")]
        public Task<HttpResponseMessage> GetContractList(ContractList contractList)
        {
            ResponseBase<ContractListResponse> response = new ResponseBase<ContractListResponse>();
            var ruleList = new List<string>();
            var scoreList = new List<string>();
            ruleList.AddRange(new string[] { "qqsafe", "Tencent-Games", "cll_qq", "mphelper", "fmessage", "newsapp", "filehelper", "weibo", "qqmail", "tmessage", "qmessage", "qqsync", "floatbottle", "lbsapp", "shakeapp", "medianote", "qqfriend", "readerapp", "blogapp", "facebookapp", "masssendapp", "meishiapp", "feedsapp", "voip", "blogappweixin", "weixin", "brandsessionholder", "weixinreminder", "wxid_novlwrv3lqwv11", "gh_22b87fa7cb3c", "officialaccounts", "notification_messages", "wxitil", "userexperience_alarm" });
            var result = wechat.InitContact(contractList.WxId, contractList.CurrentWxcontactSeq, contractList.CurrentChatRoomContactSeq);
            if (result == null || result.baseResponse.ret != (int)MMPro.MM.RetConst.MM_OK)
            {
                response.Success = false;
                response.Code = "501";
                response.Message = result.baseResponse.errMsg.@string ?? "获取失败";
                return response.ToHttpResponseAsync();
            }
            else
            {
                //过滤公众号与官方账号
                scoreList.AddRange(result.contactUsernameList);

                if (scoreList.Count > 0)
                {
                    for (int a = scoreList.Count - 1; a >= 0; a--)
                    {
                        if (scoreList[a].Contains("gh_") || ruleList.Contains(scoreList[a]))
                        {
                            scoreList.RemoveAt(a);
                        }
                    }
                }
                ContractListResponse contractResponse = new ContractListResponse();
                contractResponse.Contracts = scoreList;
                contractResponse.CurrentWxcontactSeq = result.currentWxcontactSeq;
                contractResponse.CurrentChatRoomContactSeq = result.currentChatRoomContactSeq;
                response.Data = contractResponse;
            }

            return response.ToHttpResponseAsync();
        }
      
        /// <summary>
        /// 获取全部好友列表（只包含wxid）
        /// </summary>
        /// <param name="GetUserLists"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("api/Friend/GetUserLists")]
        public Task<HttpResponseMessage> GetUserLists(ContractList contractList)
        {
            ResponseBase<ContractListResponse> response = new ResponseBase<ContractListResponse>();
            int CurrentWxcontactSeq = contractList.CurrentWxcontactSeq;
            string wxid = contractList.WxId;
            var scoreList = new List<string>();
            var ruleList = new List<string>();
            ruleList.AddRange(new string[] { "qqsafe", "Tencent-Games", "cll_qq", "mphelper", "fmessage", "newsapp", "filehelper", "weibo", "qqmail", "tmessage", "qmessage", "qqsync", "floatbottle", "lbsapp", "shakeapp", "medianote", "qqfriend", "readerapp", "blogapp", "facebookapp", "masssendapp", "meishiapp", "feedsapp", "voip", "blogappweixin", "weixin", "brandsessionholder", "weixinreminder", "wxid_novlwrv3lqwv11", "gh_22b87fa7cb3c", "officialaccounts", "notification_messages", "wxitil", "userexperience_alarm" });

            while (true)
            {
                var result = wechat.InitContact(wxid, CurrentWxcontactSeq);
                if (result == null || result.baseResponse.ret != (int)MMPro.MM.RetConst.MM_OK)
                {
                    response.Success = false;
                    response.Code = "501";
                    response.Message = result.baseResponse.errMsg.@string ?? "获取失败";
                    return response.ToHttpResponseAsync();
                }
                else
                {
                    if (CurrentWxcontactSeq != result.currentWxcontactSeq)
                    {
                        CurrentWxcontactSeq = result.currentWxcontactSeq;
                        scoreList.AddRange(result.contactUsernameList);
                    }
                    else
                    {
                        //过滤公众号与官方账号
                        if (scoreList.Count > 0)
                        {
                            for (int a = scoreList.Count - 1; a >= 0; a--)
                            {
                                if (scoreList[a].Contains("gh_") || ruleList.Contains(scoreList[a]))
                                {
                                    scoreList.RemoveAt(a);
                                }
                            }
                        }
                        ContractListResponse contractResponse = new ContractListResponse();
                        contractResponse.Contracts = scoreList;
                        response.Data = contractResponse;
                        return response.ToHttpResponseAsync();
                    }
                }
            }
        }

/*        /// <summary>
        /// 获取全部好友信息（包含详情）
        /// </summary>
        /// <param name="GetUserInfos"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("api/Friend/GetUserInfos")]
        public Task<HttpResponseMessage> GetUserInfos(ContractList contractList)
        {
            //ResponseBase<ContractListResponse> response = new ResponseBase<ContractListResponse>();
            ResponseBase<MMPro.MM.GetContactResponse> response = new ResponseBase<MMPro.MM.GetContactResponse>();
            int i = 0;
            int CurrentWxcontactSeq = contractList.CurrentWxcontactSeq;
            var scoreList = new List<string>();
            var ruleList = new List<string>();
            ruleList.AddRange(new string[] { "mphelper", "fmessage","newsapp","filehelper","weibo","qqmail","tmessage","qmessage", "qqsync","floatbottle","lbsapp","shakeapp","medianote","qqfriend","readerapp","blogapp","facebookapp","masssendapp","meishiapp","feedsapp","voip","blogappweixin","weixin","brandsessionholder","weixinreminder","wxid_novlwrv3lqwv11", "gh_22b87fa7cb3c","officialaccounts","notification_messages","wxitil","userexperience_alarm"});
            while (true)
            {
                var result = wechat.InitContact(contractList.WxId, CurrentWxcontactSeq, contractList.CurrentChatRoomContactSeq);
                if (result == null || result.baseResponse.ret != (int)MMPro.MM.RetConst.MM_OK)
                {
                    response.Success = false;
                    response.Code = "501";
                    response.Message = result.baseResponse.errMsg.@string ?? "获取失败";
                    return response.ToHttpResponseAsync();
                }
                else
                {

                    if (result.contactUsernameList != null)
                    {
                        CurrentWxcontactSeq = result.currentWxcontactSeq;
                        scoreList.AddRange(result.contactUsernameList);
                    }
                    else
                    {
                        //过滤公众号
                        if (scoreList?.Count > 0)
                        {
                            for (int a = scoreList.Count - 1; a >= 0; a--)
                            {
                                if (scoreList[a].Contains("gh_")||ruleList.Contains(scoreList[a]))
                                {
                                    scoreList.RemoveAt(a);
                                }
                            }
                        }
                        var userinfo = wechat.GetContactAny(contractList.WxId, scoreList);
                        if (userinfo == null || userinfo.baseResponse.ret != (int)MMPro.MM.RetConst.MM_OK)
                        {
                            response.Success = false;
                            response.Code = "501";
                            response.Message = result.baseResponse.errMsg.@string ?? "获取失败";
                            return response.ToHttpResponseAsync();
                        }
                        else
                        {
                            response.Data = userinfo;
                        }
                        return response.ToHttpResponseAsync();
                    }

                }
            }
        }*/
        /// <summary>
        /// 获取单页好友列表(包含详情)
        /// </summary>
        /// <param name="initUser"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("api/Friend/InitUser")]
        public Task<HttpResponseMessage> InitUser(InitUser initUser)
        {
            ResponseBase<InitResponse> response = new ResponseBase<InitResponse>();
            var result = wechat.GetContracts(initUser.WxId, initUser.IsFirst);
            response.Data = result;
            return response.ToHttpResponseAsync();
        }

        /// <summary>
        /// 获取所有好友列表(包含详情)
        /// </summary>
        /// <param name="wxId"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("api/Friend/GetUers/{wxId}")]
        public Task<HttpResponseMessage> GetUers(string wxId)
        {
            ResponseBase<InitResponse> response = new ResponseBase<InitResponse>();

            var result = wechat.GetContracts(wxId, true);
            int i = 0;
            while (true)
            {
                i++;
                if (i > 5000)
                {
                    break;
                }
                var resultNext = wechat.GetContracts(wxId, false);
                if (resultNext.ModContacts?.Count > 0)
                {
                    result.ModContacts?.AddRange(resultNext.ModContacts);
                }
                else
                {
                    break;
                }
            }

            //过滤公众号
            if (result.ModContacts?.Count > 0)
            {
                for (int a = result.ModContacts.Count - 1; a >= 0; a--)
                {
                    var userInfo = result.ModContacts[a];
                    var username = userInfo.UserName.String;
                    if (username.Contains("gh_"))
                    {
                        result.ModContacts.RemoveAt(a);
                    }
                }
            }

            response.Data = result;

            return response.ToHttpResponseAsync();
        }




        /// <summary>
        /// 获取好友简介
        /// </summary>
        /// <param name="batchContractProfile"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("api/Friend/BatchGetProfile")]
        public Task<HttpResponseMessage> BatchGetContractProfile(BatchContractProfile batchContractProfile)
        {
            ResponseBase<IList<micromsg.ContactProfile>> response = new ResponseBase<IList<micromsg.ContactProfile>>();
            var result = wechat.BatchGetContractProfile(batchContractProfile.WxId, batchContractProfile.SearchWxIds, 0);
            response.Data = result;
            return response.ToHttpResponseAsync();
        }

        /// <summary>
        /// 批量获取微信头像
        /// </summary>
        /// <param name="batchGetHeadImg"></param>
        /// <returns></returns>
        [HttpPost]
        [NoResponseLog]
        [Route("api/Friend/BatchGetHeadImg")]
        public Task<HttpResponseMessage> BatchGetHeadImg(BatchGetHeadImg batchGetHeadImg)
        {
            ResponseBase<IList<micromsg.ImgPair>> response = new ResponseBase<IList<micromsg.ImgPair>>();
            var result = wechat.BatchGetHeadImg(batchGetHeadImg.WxId, batchGetHeadImg.SearchWxIds);
            response.Data = result;
            return response.ToHttpResponseAsync();
        }

        /// <summary>
        /// 获取好友详情
        /// </summary>
        /// <param name="getContractDetail"></param> 
        /// <returns></returns>
        [HttpPost]
        [Route("api/Friend/GetContractDetail")]
        public Task<HttpResponseMessage> GetContractDetail(ContractDetail getContractDetail)
        {
            ResponseBase<MMPro.MM.GetContactResponse> response = new ResponseBase<MMPro.MM.GetContactResponse>();
            var resu1lt = wechat.GetContactBriefInfo(getContractDetail.WxId, getContractDetail.SearchWxIds);
            //friendRelation 为 4 自己拉黑对方
            //friendRelation 为 1是被对方删除了
            //friendrelation 为 5是被对方设置了黑名单
            var result = wechat.GetContactAny(getContractDetail.WxId, getContractDetail.SearchWxIds, getContractDetail.ChatRoom);
            if (result == null || result.baseResponse.ret != (int)MMPro.MM.RetConst.MM_OK)
            {
                //if ((((bitVal & 0xff) >> 3) & 1) == 1)
                //{
                //    //被我拉黑          
                //}
                //else
                //{
                //    //我没有拉黑
                //}
                response.Success = false;
                response.Code = "501";
                response.Message = result.baseResponse.errMsg.@string ?? "获取失败";
                return response.ToHttpResponseAsync();
            }
            else
            {
                response.Data = result;
            }
            return response.ToHttpResponseAsync();
        }

        /// <summary>
        /// 获取好友关系状态1：删除 4：自己拉黑 5：被拉黑
        /// </summary>
        /// <param name="getFriendRelation"></param> 
        /// <returns></returns>
        [HttpPost]
        [Route("api/Friend/GetFriendRelation")]
        public Task<HttpResponseMessage> GetFriendRelation(GetFriendRelation getFriendRelation)
        {
            ResponseBase<MMBizJsApiGetUserOpenIdResp> response = new ResponseBase<MMBizJsApiGetUserOpenIdResp>();
            //friendRelation 为 4 自己拉黑对方
            //friendRelation 为 1是被对方删除了
            //friendrelation 为 5是被对方设置了黑名单
            var result = wechat.GetUserOpenId(getFriendRelation.WxId, getFriendRelation.ToWxId);

            if (result == null || result.baseResponse.Ret != (int)MMPro.MM.RetConst.MM_OK)
            {
                response.Success = false;
                response.Code = "501";
                response.Message = result.baseResponse.ErrMsg.String ?? "获取失败";
                return response.ToHttpResponseAsync();
            }
            else
            {
                response.Data = result;
            }
            return response.ToHttpResponseAsync();
        }
        /// <summary>
        /// 搜索微信用户信息
        /// </summary>
        /// <param name="searchContact"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("api/Friend/SearchContract")]
        public Task<HttpResponseMessage> SearchContract(SearchContact searchContact)
        {
            ResponseBase<MMPro.MM.SearchContactResponse> response = new ResponseBase<MMPro.MM.SearchContactResponse>();

            var result = wechat.SearchContact(searchContact.WxId, searchContact.UserName);
            if (result == null || result.baseResponse.ret != (int)MMPro.MM.RetConst.MM_OK)
            {
                response.Success = false;
                response.Code = "501";
                response.Message = result.baseResponse.errMsg.@string ?? "获取失败";
                return response.ToHttpResponseAsync();
            }
            else
            {
                response.Data = result;
            }

            return response.ToHttpResponseAsync();
        }

        /// <summary>
        /// 修改备注描述
        /// </summary>
        /// <param name="setRemark"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("api/Friend/SetRemark")]
        public Task<HttpResponseMessage> SetRemark(SetRemark setRemark)
        {
            ResponseBase response = new ResponseBase();
 
            var getContactDetail = wechat.GetContactDetail(setRemark.WxId, new List<string>() { setRemark.ToWxId });
            if (getContactDetail.BaseResponse.Ret == 0)
            {
                var modUserInfo = getContactDetail.ContactList[0];

                modUserInfo.Remark = new micromsg.SKBuiltinString_t()
                {
                    String = setRemark.Remark
                };

                var result0 = wechat.OpLogRemark(setRemark.WxId, 2, modUserInfo);
                micromsg.ModDescription modDescription = new micromsg.ModDescription()
                {
                    ContactUsername = setRemark.ToWxId,
                    Desc = setRemark.Desc
                };
                var result = wechat.OpLog(setRemark.WxId, 54, modDescription);
                if (result == null || result.Ret != 0 || result.OplogRet.Ret.FirstOrDefault() != 0)
                {
                    response.Success = false;
                    response.Code = "501";
                    response.Message = "修改失败";
                }
                else
                {
                    response.Success = true;
                    response.Message = "修改成功";
                }
            }
            else
            {
                response.Success = false;
                response.Code = "501";
                response.Message = "修改失败";
            }
            return response.ToHttpResponseAsync();
        }

        /// <summary>
        /// 好友设置选项
        /// </summary>
        /// <param name="setOptions"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("api/Friend/SetOptions")]
        public Task<HttpResponseMessage> SetOptions(SetOptions setOptions)
        {
            ResponseBase response = new ResponseBase();

            var getContactDetail = wechat.GetContactDetail(setOptions.WxId, new List<string>() { setOptions.ToWxId });
            if (getContactDetail.BaseResponse.Ret == 0)
            {
                var modUserInfo = getContactDetail.ContactList[0];
                int bitVal = 3;
                if (setOptions.IsStar)
                {
                    bitVal += FriendOptions.Star;
                }
                //if (setOptions.IsShowFromSns)
                //{
                //    bitVal += FriendOptions.ShowFromSns;
                //}
                //if (setOptions.IsShowToSns)
                //{
                //    bitVal += FriendOptions.ShowToSns;
                //}
                if (setOptions.IsBlackList)
                {
                    bitVal += FriendOptions.BlackList;
                }
                modUserInfo.BitVal = (uint)bitVal;


                if (setOptions.IsMsgNoInterruption)
                {
                    bitVal += FriendOptions.MsgNoInterruption;
                }
                if (setOptions.IsTopMsg)
                {
                    bitVal += FriendOptions.TopMsg;
                }
                modUserInfo.BitVal = (uint)bitVal;
                var result = wechat.OpLog(setOptions.WxId, 2, modUserInfo);

                if (result == null || result.Ret != 0 || result.OplogRet.Ret.FirstOrDefault() != 0)
                {
                    response.Success = false;
                    response.Code = "501";
                    response.Message = "修改失败";
                }
                else
                {
                    response.Success = true;
                    response.Message = "修改成功";
                }
            }
            else
            {
                response.Success = false;
                response.Code = "501";
                response.Message = "修改失败";
            }
            return response.ToHttpResponseAsync();
        }

        /// <summary>
        /// 添加群好友请求
        /// </summary>
        /// <param name="addChatroomFriend"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("api/Friend/AddChatroomFriend")]
        public Task<HttpResponseMessage> AddChatroomFriend(AddChatroomFriendRequest addChatroomFriend)
        {
            ResponseBase<string> response = new ResponseBase<string>();


            var getContactDetail = wechat.GetContactDetail(addChatroomFriend.WxId, new List<string>() { addChatroomFriend.ToWxId }, addChatroomFriend.ChatRoomName);

            if (getContactDetail == null || getContactDetail.BaseResponse.Ret != (int)MMPro.MM.RetConst.MM_OK)
            {

                response.Success = false;
                response.Message = "添加失败";
            }
            else
            {
                var contactDetail = getContactDetail.ContactList[0];

                var serachContactResp = wechat.SearchContact(addChatroomFriend.WxId, contactDetail.Alias);
                if (serachContactResp == null || serachContactResp.baseResponse.ret != (int)MMPro.MM.RetConst.MM_OK)
                {
                    response.Success = false;
                    response.Message = "添加失败";
                }
                else
                {
                    var result = wechat.VerifyUser(addChatroomFriend.WxId, MMPro.MM.VerifyUserOpCode.MM_VERIFYUSER_SENDREQUEST, addChatroomFriend.Content, serachContactResp.antispamTicket, serachContactResp.userName.@string, 3);
                    if (result == null || result.baseResponse.ret != (int)MMPro.MM.RetConst.MM_OK)
                    {
                        response.Success = false;
                        response.Message = result?.baseResponse?.errMsg?.@string;
                        return response.ToHttpResponseAsync();
                    }
                    else
                    {
                        response.Data = result.userName;
                    }

                }

            }


            return response.ToHttpResponseAsync();
        }



        /// <summary>
        /// 添加好友
        /// </summary>
        /// <param name="addFriend"></param> 
        /// <returns></returns>
        [HttpPost]
        [Route("api/Friend/AddFriend")]
        public Task<HttpResponseMessage> AddFriend(AddFriend addFriend)
        {
            ResponseBase<string> response = new ResponseBase<string>();

            var result = wechat.VerifyUser(addFriend.WxId, MMPro.MM.VerifyUserOpCode.MM_VERIFYUSER_ADDCONTACT, addFriend.Content, addFriend.AntispamTicket, addFriend.UserNameV1, (byte)addFriend.Origin);
            if (result == null || result.baseResponse.ret != (int)MMPro.MM.RetConst.MM_OK)
            {
                response.Success = false;
                response.Code = "501";
                response.Message = result?.baseResponse?.errMsg?.@string;
                return response.ToHttpResponseAsync();
            }
            else
            {
                response.Data = result.userName;
            }


            return response.ToHttpResponseAsync();
        }
        /// <summary>
        /// 发送好友请求
        /// </summary>
        /// <param name="addFriend"></param> 
        /// <returns></returns>
        [HttpPost]
        [Route("api/Friend/AddFriendRequest")]
        public Task<HttpResponseMessage> AddFriendRequest(AddFriend addFriend)
        {
            ResponseBase<string> response = new ResponseBase<string>();

            var result = wechat.VerifyUser(addFriend.WxId, MMPro.MM.VerifyUserOpCode.MM_VERIFYUSER_SENDREQUEST, addFriend.Content, addFriend.AntispamTicket, addFriend.UserNameV1, (byte)addFriend.Origin);
            if (result == null || result.baseResponse.ret != (int)MMPro.MM.RetConst.MM_OK)
            {
                response.Success = false;
                response.Code = "501";
                response.Message = result?.baseResponse?.errMsg?.@string;
                return response.ToHttpResponseAsync();
            }
            else
            {
                response.Data = result.userName;
            }

            return response.ToHttpResponseAsync();
        }

        /// <summary>
        /// 批量添加好友
        /// </summary>
        /// <param name="addFriendList"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("api/Friend/AddFriendRequestList")]
        public Task<HttpResponseMessage> AddFriendRequestList(AddFriendList addFriendList)
        {
            ResponseBase<string> response = new ResponseBase<string>();
            if (addFriendList == null || addFriendList.Friends == null || addFriendList.Friends.Count == 0)
            {
                response.Success = false;
                response.Code = "400";
                response.Message = "添加好友信息不能为空";
                return response.ToHttpResponseAsync();
            }

            MMPro.MM.VerifyUser[] verifyUser_ = new MMPro.MM.VerifyUser[addFriendList.Friends.Count];
            byte[] senceList = new byte[addFriendList.Friends.Count];
            for (int i = 0; i < addFriendList.Friends.Count; i++)
            {
                MMPro.MM.VerifyUser user = new MMPro.MM.VerifyUser();
                user.value = addFriendList.Friends[i].UserNameV1;
                user.antispamTicket = addFriendList.Friends[i].AntispamTicket;
                user.friendFlag = 0;
                user.scanQrcodeFromScene = 0;
                verifyUser_[i] = user;

                senceList[i] = (byte)addFriendList.Friends[i].Origin;
            }


            var result = wechat.VerifyUserList(addFriendList.WxId, MMPro.MM.VerifyUserOpCode.MM_VERIFYUSER_SENDREQUEST, addFriendList.Content, verifyUser_, senceList);
            if (result == null || result.baseResponse.ret != (int)MMPro.MM.RetConst.MM_OK)
            {
                response.Success = false;
                response.Code = "501";
                response.Message = result?.baseResponse?.errMsg?.@string;
                return response.ToHttpResponseAsync();
            }
            else
            {
                response.Data = result.userName;
            }

            return response.ToHttpResponseAsync();
        }





        /// <summary>
        /// 通过好友验证
        /// </summary>
        /// <param name="passFriendVerify"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("api/Friend/PassFriendVerify")]
        public Task<HttpResponseMessage> PassFriendVerify(FriendVerify passFriendVerify)
        {
            ResponseBase<string> response = new ResponseBase<string>();

            var result = wechat.VerifyUser(passFriendVerify.WxId, MMPro.MM.VerifyUserOpCode.MM_VERIFYUSER_VERIFYOK, passFriendVerify.Content, passFriendVerify.AntispamTicket, passFriendVerify.UserNameV1, (byte)passFriendVerify.Origin);
            if (result == null || result.baseResponse.ret != (int)MMPro.MM.RetConst.MM_OK)
            {
                response.Success = false;
                response.Code = "501";
                response.Message = result?.baseResponse?.errMsg?.@string;
                return response.ToHttpResponseAsync();
            }
            else
            {
                response.Data = result.userName;
            }

            return response.ToHttpResponseAsync();
        }





        /// <summary>
        /// 拒绝好友验证
        /// </summary>
        /// <param name="rejectFriendVerify"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("api/Friend/RejectFriendVerify")]
        public Task<HttpResponseMessage> RejectFriendVerify(FriendVerify rejectFriendVerify)
        {
            ResponseBase<string> response = new ResponseBase<string>();

            var result = wechat.VerifyUser(rejectFriendVerify.WxId, MMPro.MM.VerifyUserOpCode.MM_VERIFYUSER_VERIFYREJECT, rejectFriendVerify.Content, rejectFriendVerify.AntispamTicket, rejectFriendVerify.UserNameV1, (byte)rejectFriendVerify.Origin);
            if (result == null || result.baseResponse.ret != (int)MMPro.MM.RetConst.MM_OK)
            {
                response.Success = false;
                response.Code = "501";
                response.Message = result?.baseResponse?.errMsg?.@string;
                return response.ToHttpResponseAsync();
            }
            else
            {
                response.Data = result.userName;
            }


            return response.ToHttpResponseAsync();
        }




        /// <summary>
        /// 删除好友
        /// </summary>
        /// <param name="deleteFriend"></param> 
        /// <returns></returns>
        [HttpPost]
        [Route("api/Friend/DeleteFriend")]
        public Task<HttpResponseMessage> DeleteFriend(DeleteFriend deleteFriend)
        {
            ResponseBase<string> response = new ResponseBase<string>();

            var getContactDetail = wechat.GetContactDetail(deleteFriend.WxId, new List<string>() { deleteFriend.ToWxId });

            micromsg.DelContact delContact = new micromsg.DelContact()
            {
                UserName = new micromsg.SKBuiltinString_t()
                {
                    String = deleteFriend.ToWxId
                }
            };

            var result = wechat.OpLog(deleteFriend.WxId, 4, delContact);

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
        /// 批量删除好友
        /// </summary>
        /// <param name="batchDeleteFriend"></param> 
        /// <returns></returns>
        [HttpPost]
        [Route("api/Friend/BatchDeleteFriend")]
        public Task<HttpResponseMessage> BatchDeleteFriend(BatchDeleteFriend batchDeleteFriend)
        {
            ResponseBase<string> response = new ResponseBase<string>();
            int okCount = 0;
            int errCount = 0;
            foreach (var wxid in batchDeleteFriend.DeleteWxIds)
            {
                micromsg.DelContact delContact = new micromsg.DelContact()
                {
                    UserName = new micromsg.SKBuiltinString_t()
                    {
                        String = wxid
                    }
                };

                var result = wechat.OpLog(batchDeleteFriend.WxId, 4, delContact);
                if (result == null || result.Ret != 0 || result.OplogRet.Ret.FirstOrDefault() != 0)
                {
                    errCount++;
                }
                else
                {
                    okCount++;
                }
                Thread.Sleep(1000);
            }
            response.Message = "删除成功";
            response.Data = $"删除成功{okCount}个，删除失败{errCount}个";
            return response.ToHttpResponseAsync();
        }

        /// <summary>
        /// 上传手机联系人
        /// </summary>
        /// <param name="uploadContrat"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("api/Friend/UploadContrats")]
        public async Task<HttpResponseMessage> UploadContrats(UploadContrat uploadContrat)
        {
            ResponseBase response = new ResponseBase();
            if (uploadContrat == null || uploadContrat.PhoneNos == null || uploadContrat.PhoneNos.Count == 0)
            {
                response.Success = false;
                response.Code = "400";
                response.Message = "上传手机号码不能为空";
                return await response.ToHttpResponseAsync();
            }

            micromsg.Mobile[] mobiles = new micromsg.Mobile[uploadContrat.PhoneNos.Count];
            for (int i = 0; i < uploadContrat.PhoneNos.Count; i++)
            {
                micromsg.Mobile mobile = new micromsg.Mobile();
                mobile.v = uploadContrat.PhoneNos[i];
                mobiles[i] = mobile;
            }
            var result = wechat.UploadMContact(uploadContrat.WxId, uploadContrat.CurrentPhoneNo, mobiles);
            if (result == null || result.BaseResponse.Ret != (int)MMPro.MM.RetConst.MM_OK)
            {
                response.Success = false;
                response.Code = "501";
                response.Message = result?.BaseResponse?.ErrMsg?.String ?? "上传失败";
                return await response.ToHttpResponseAsync();
            }
            else
            {
                response.Message = "上传成功";
            }


            return await response.ToHttpResponseAsync();
        }


        /// <summary>
        /// 获取手机联系人列表
        /// </summary>
        /// <param name="wxId">微信id</param>
        /// <returns></returns>
        [HttpPost]
        [Route("api/Friend/GetMFriend/{wxId}")]
        public Task<HttpResponseMessage> GetMFriend(string wxId)
        {
            ResponseBase<micromsg.GetMFriendResponse> response = new ResponseBase<micromsg.GetMFriendResponse>();
            var result = wechat.GetMFriend(wxId, 0);
            response.Data = result;
            return response.ToHttpResponseAsync();
        }








        ///// <summary>
        ///// 无痕查询僵尸粉(1个好友大概耗时1s)
        ///// </summary>
        ///// <param name="getCorpseFans">微信Id</param>
        ///// <returns></returns>
        //[HttpPost]
        //[Route("api/Friend/GetCorpseFansDetail")]
        //public Task<HttpResponseMessage> GetCorpseFansDetail(GetCorpseFansRequest getCorpseFans)
        //{
        //    ResponseBase<CorpseFansDetailResponse> response = new ResponseBase<CorpseFansDetailResponse>();
        //    List<micromsg.ModContact> corpseFansUserList = new List<micromsg.ModContact>();
        //    List<micromsg.ModContact> blockFansUserList = new List<micromsg.ModContact>();
        //    int count = 0;
        //    wechat.SendNewMsg(getCorpseFans.WxId, getCorpseFans.WxId, "检测僵尸粉开始");
        //    Stopwatch watch = new Stopwatch();
        //    watch.Start();
        //    //获取所有好友
        //    var friendList = wechat.GetContracts(getCorpseFans.WxId, true);

        //    if (friendList?.ModContacts?.Count > 0)
        //    {
        //        while (friendList.ModContacts.Count > 0)
        //        {
        //            foreach (var contact in friendList.ModContacts)
        //            {

        //                if (contact.PersonalCard == 1 && !string.IsNullOrEmpty(contact.Alias))
        //                {
        //                    count++;
        //                    string cardNickName = string.IsNullOrEmpty(contact.NickName.String) ? contact.UserName.String : contact.NickName.String;
        //                    if (string.IsNullOrEmpty(contact.BigHeadImgUrl))
        //                    {
        //                        corpseFansUserList.Add(contact);
        //                        contact.Remark = new micromsg.SKBuiltinString_t()
        //                        {
        //                            String = $"被拉黑"
        //                        };
        //                        var result = wechat.OpLogRemark(getCorpseFans.WxId, 2, contact);
        //                        string appMessageFormat = $"<?xml version=\"1.0\"?>\n<msg bigheadimgurl=\"\" smallheadimgurl=\"\" username=\"{ contact.UserName.String}\" nickname=\"{cardNickName}（被拉黑）\" fullpy=\"\" shortpy=\"\" alias=\"{contact.Alias}\" imagestatus=\"0\" scene=\"17\" province=\"\" city=\"\" sign=\"\" sex=\"{contact.Sex}\" certflag=\"0\" certinfo=\"\" brandIconUrl=\"\" brandHomeUrl=\"\" brandSubscriptConfigUrl=\"\" brandFlags=\"0\" regionCode=\"CN\" />\n";
        //                        var newSendMsgRespone = wechat.SendNewMsg(getCorpseFans.WxId, getCorpseFans.WxId, appMessageFormat, 42);
        //                        corpseFansUserList.Add(contact);
        //                    }
        //                    else
        //                    {
        //                        var getcontractAny = wechat.GetContactAny(getCorpseFans.WxId, new List<string>() { contact.UserName.String });
        //                        if (!string.IsNullOrEmpty(getcontractAny.ticket.FirstOrDefault()?.antispamticket))
        //                        {
        //                            if (string.IsNullOrEmpty(contact.BigHeadImgUrl))
        //                            {
        //                                corpseFansUserList.Add(contact);
        //                                contact.Remark = new micromsg.SKBuiltinString_t()
        //                                {
        //                                    String = $"被拉黑"
        //                                };
        //                                var result = wechat.OpLogRemark(getCorpseFans.WxId, 2, contact);
        //                                string appMessageFormat = $"<?xml version=\"1.0\"?>\n<msg bigheadimgurl=\"\" smallheadimgurl=\"\" username=\"{ contact.UserName.String}\" nickname=\"{cardNickName}（被拉黑）\" fullpy=\"\" shortpy=\"\" alias=\"{contact.Alias}\" imagestatus=\"0\" scene=\"17\" province=\"\" city=\"\" sign=\"\" sex=\"{contact.Sex}\" certflag=\"0\" certinfo=\"\" brandIconUrl=\"\" brandHomeUrl=\"\" brandSubscriptConfigUrl=\"\" brandFlags=\"0\" regionCode=\"CN\" />\n";
        //                                var newSendMsgRespone = wechat.SendNewMsg(getCorpseFans.WxId, getCorpseFans.WxId, appMessageFormat, 42);
        //                                blockFansUserList.Add(contact);
        //                            }
        //                            else
        //                            {
        //                                contact.Remark = new micromsg.SKBuiltinString_t()
        //                                {
        //                                    String = $"被删除"
        //                                };
        //                                var result = wechat.OpLogRemark(getCorpseFans.WxId, 2, contact);
        //                                var newSendMsgRespone = wechat.SendNewMsg(getCorpseFans.WxId, getCorpseFans.WxId, $"{cardNickName} {contact.Alias}（被删除）", 1);
        //                                corpseFansUserList.Add(contact);
        //                            }

        //                        }

        //                        Thread.Sleep(1000);

        //                    }
                      
        //                }

        //            }
        //            //循环获取好友
        //            friendList = wechat.GetContracts(getCorpseFans.WxId, false);
        //        }
        //        watch.Stop();
        //        var mSeconds = watch.ElapsedMilliseconds;
        //        CorpseFansDetailResponse corpseFans = new CorpseFansDetailResponse();
        //        corpseFans.CorpseFans = corpseFansUserList;
        //        corpseFans.BlockFans = blockFansUserList;
        //        corpseFans.Info = $"总共查询{count}个好友(包含已删除的好友)，耗时{mSeconds / 1000 / 60 + 1}分钟，发现被拉黑{corpseFansUserList.Count}个，被删除{blockFansUserList.Count}个";
        //        response.Data = corpseFans;

        //        wechat.SendNewMsg(getCorpseFans.WxId, getCorpseFans.WxId, corpseFans.Info);
        //    }
        //    else
        //    {
        //        response.Success = false;
        //        response.Code = "501";
        //        response.Message = "查询失败";

        //    }
        //    return response.ToHttpResponseAsync();
        //}


    }
}