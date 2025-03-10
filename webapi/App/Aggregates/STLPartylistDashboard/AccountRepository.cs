﻿using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;

using Comm.Commons.Extensions;
using Infrastructure.Repositories;
using webapi.Commons.AutoRegister;
using webapi.App.Aggregates.Common;
using webapi.App.RequestModel.AppRecruiter;
using webapi.App.Model.User;
using webapi.App.Aggregates.Common.Dto;

namespace webapi.App.Aggregates.STLPartylistDashboard
{
    [Service.ITransient(typeof(AccountRepository))]
    public interface IAccountRepository
    {
        Task<(Results result, String message, STLAccount account)>DashboardSignInAsync(STLSignInRequest request);
        Task<(Results result, String message)> RequiredChangePassword(RequiredChangePassword request);
        Task<(object PartyList, object Group)> MemberGroup(STLAccount account);
        Task<(String Message, object menu)> LoadPGS(string userid, string accttype);

        Task<(Results result, String message, STLAccount account)> DashboardSignInAsync02(STLSignInRequest request);
        Task<(String Message, object menu)> LoadPGS02(string profileid, string accttype);


    }
    public class AccountRepository : IAccountRepository
    {
        private readonly IRepository _repo;
        public AccountRepository(IRepository repo)
        {
            _repo = repo;
        }

        public async Task<(object PartyList, object Group)> MemberGroup(STLAccount account)
        {
            var results = _repo.DSpQueryMultiple("dbo.spfn_CBA01", new Dictionary<string, object>()
            {
                {"parmplid",account.PL_ID },
                {"parmpgrpid",account.PGRP_ID },
                //{"parmpsncd",account.PSN_CD }
            });
            if (results != null)
            {
                var Group = STLSubscriberDto.GetGroup(results.ReadSingleOrDefault());
                var PartyList = STLSubscriberDto.GetPartyList(results.ReadSingleOrDefault());
                return (PartyList, Group);
            }
            return (null, null);


        }

        public async Task<(Results result, String message, STLAccount account)> DashboardSignInAsync(STLSignInRequest request)
        {
            var results = _repo.DSpQueryMultiple("dbo.spfn_BDAOL", new Dictionary<string, object>()
            {
                {"parmusernm",request.Username },
                {"parmusrpsswrd",request.Password },
            });
            if (results != null)
            {
                var row = ((IDictionary<string, object>)results.ReadFirstOrDefault());
                string ResultCode = row["RESULT"].Str();
                if (ResultCode == "1")
                {
                    return (Results.Success, null, new STLAccount()
                    {
                        PL_ID = row["PL_ID"].Str(),
                        PGRP_ID = row["PGRP_ID"].Str(),
                        //PSN_CD = row["PSN_CD"].Str(),
                        USR_ID = row["USR_ID"].Str(),
                        ACT_ID = row["ACT_ID"].Str(),
                        ACT_TYP = row["ACT_TYP"].Str(),
                        FRST_NM = row["FRST_NM"].Str(),
                        LST_NM = row["LST_NM"].Str(),
                        MDL_NM = row["MDL_NM"].Str(),
                        FLL_NM = row["FLL_NM"].Str(),
                        NCK_NM = row["NCK_NM"].Str(),

                        MOB_NO = row["MOB_NO"].Str(),
                        EML_ADD = row["EML_ADD"].Str(),
                        HM_ADDR = row["HM_ADDR"].Str(),
                        PRSNT_ADDR = row["PRSNT_ADDR"].Str(),
                        LOC_REG = row["LOC_REG"].Str(),
                        LOC_PROV = row["LOC_PROV"].Str(),
                        LOC_MUN = row["LOC_MUN"].Str(),
                        LOC_BRGY = row["LOC_BRGY"].Str(),
                        LOC_BRGY_NM = row["LOC_BRGY_NM"].Str(),

                        GNDR = row["GNDR"].Str(),
                        MRTL_STAT = row["MRTL_STAT"].Str(),
                        CTZNSHP = row["CTZNSHP"].Str(),
                        ImageUrl = row["IMG_URL"].Str(),
                        BRT_DT = row["BRT_DT"].Str(),
                        BLD_TYP = row["BLD_TYP"].Str(),
                        NATNLTY = row["NATNLTY"].Str(),
                        OCCPTN = row["OCCPTN"].Str(),
                        SKLLS = row["SKLLS"].Str(),
                        PRF_PIC = row["PRF_PIC"].Str(),
                        SIGNATUREID = row["SIGNATUREID"].Str(),
                        S_ACTV = row["S_ACTV"].Str(),
                        SessionID = row["SSSN_ID"].Str(),
                        SKIN = row["SKIN"].Str(),
                        SUB_TYP = row["SUB_TYP"].Str(),
                        sActive = true,
                        IsLogin = true
                    });
                }
                else if (ResultCode == "22")
                    return (Results.Failed, "Your account was in-active", null);
                else if (ResultCode == "21")
                    return (Results.Failed, "Your account has no rigth to access this system", null);
                else if (ResultCode == "0")
                    return (Results.Failed, "Invalid username and password! Please try again", null);
                return (Results.Failed, "Invalid username and password! Please try again", null);
            }
            return (Results.Null, null, null);
        }

        public async Task<(String Message, object menu)> LoadPGS(string userid, string accttype)
        {
            //var results = _repo.DQuery<dynamic>($"dbo.spfn_CAA01 '8888'");
            //var results = _repo.DQuery<dynamic>(@"Select top(1) PGS from STLAAD where USR_ID=@USR_ID", new Dictionary<string, object>() { { "USR_ID", userid} });
            var results = _repo.DQuery<dynamic>($"Select top(1) PGS from STLAAD where USR_ID='{userid}'");
            if (results != null)
            {
                var menupage = STLSubscriberDto.GetMenuPage(results.FirstOrDefault(), accttype);
                return (null, menupage);
            }
            return (null, null);
        }

        public async Task<(String Message, object menu)> LoadPGS02(string profileid, string accttype)
        {
            //var results = _repo.DQuery<dynamic>($"dbo.spfn_CAA01 '8888'");
            //var results = _repo.DQuery<dynamic>(@"Select top(1) PGS from STLAAD where USR_ID=@USR_ID", new Dictionary<string, object>() { { "USR_ID", userid} });
            var results = _repo.DSpQuery<dynamic>($"dbo.spfn_PROFILE_GETSINGLE", new Dictionary<string, object>()
            {
                //{"parmplid",account.PL_ID },
                {"parmprofileid", profileid},


            });
            if (results != null)
            {
                var menupage = STLSubscriberDto.GetMenuPage(results.FirstOrDefault(), accttype);
                return (null, menupage);
            }
            return (null, null);
        }

        public async Task<(Results result, String message, STLAccount account)> DashboardSignInAsync02(STLSignInRequest request)
        {
            var results = _repo.DSpQueryMultiple("dbo.spfn_BDAOL01", new Dictionary<string, object>()
            {
                {"parmusernm",request.Username },
                {"parmusrpsswrd",request.Password },
            });
            if (results != null)
            {
                var row = ((IDictionary<string, object>)results.ReadFirstOrDefault());
                string ResultCode = row["RESULT"].Str();
                if (ResultCode == "1")
                {
                    return (Results.Success, null, new STLAccount()
                    {
                        PL_ID = row["PL_ID"].Str(),
                        PGRP_ID = row["PGRP_ID"].Str(),
                        //PSN_CD = row["PSN_CD"].Str(),
                        USR_ID = row["USR_ID"].Str(),
                        ACT_ID = row["ACT_ID"].Str(),
                        ACT_TYP = row["ACT_TYP"].Str(),
                        FRST_NM = row["FRST_NM"].Str(),
                        LST_NM = row["LST_NM"].Str(),
                        MDL_NM = row["MDL_NM"].Str(),
                        FLL_NM = row["FLL_NM"].Str(),
                        NCK_NM = row["NCK_NM"].Str(),

                        MOB_NO = row["MOB_NO"].Str(),
                        EML_ADD = row["EML_ADD"].Str(),
                        HM_ADDR = row["HM_ADDR"].Str(),
                        PRSNT_ADDR = row["PRSNT_ADDR"].Str(),
                        LOC_REG = row["LOC_REG"].Str(),
                        LOC_PROV = row["LOC_PROV"].Str(),
                        LOC_MUN = row["LOC_MUN"].Str(),
                        LOC_BRGY = row["LOC_BRGY"].Str(),
                        LOC_BRGY_NM = row["LOC_BRGY_NM"].Str(),

                        GNDR = row["GNDR"].Str(),
                        MRTL_STAT = row["MRTL_STAT"].Str(),
                        CTZNSHP = row["CTZNSHP"].Str(),
                        ImageUrl = row["IMG_URL"].Str(),
                        BRT_DT = row["BRT_DT"].Str(),
                        BLD_TYP = row["BLD_TYP"].Str(),
                        NATNLTY = row["NATNLTY"].Str(),
                        OCCPTN = row["OCCPTN"].Str(),
                        SKLLS = row["SKLLS"].Str(),
                        PRF_PIC = row["PRF_PIC"].Str(),
                        SIGNATUREID = row["SIGNATUREID"].Str(),
                        S_ACTV = row["S_ACTV"].Str(),
                        SessionID = row["SSSN_ID"].Str(),
                        SKIN = row["SKIN"].Str(),
                        SUB_TYP = row["SUB_TYP"].Str(),
                        CAN_CREATE = row["CAN_CREATE"].Str(),
                        CAN_UPDATE = row["CAN_UPDATE"].Str(),
                        CAN_DELETE = row["CAN_DELETE"].Str(),
                        PROFILE_ACCESS = row["PROFILE_ACCESS"].Str(),
                        SUBSCRIBER_ID = row["SUBSCRIBER_ID"].Str(),
                        PROFILE_ID = row["PROFILE_ID"].Str(),
                        RQRD_CHNG_PSSWRD = row["RQRD_CHNG_PSSWRD"].Str(),
                        TRGR_CHNG_PSSWRD = row["TRGR_CHNG_PSSWRD"].Str(),
                        sActive = true,
                        IsLogin = true
                    });
                }
                else if (ResultCode == "22")
                    return (Results.Failed, "Your account was in-active", null);
                else if (ResultCode == "21")
                    return (Results.Failed, "Your account has no rigth to access this system", null);
                else if (ResultCode == "0")
                    return (Results.Failed, "Invalid username and password! Please try again", null);
                return (Results.Failed, "Invalid username and password! Please try again", null);
            }
            return (Results.Null, null, null);
        }

        public async Task<(Results result, string message)> RequiredChangePassword(RequiredChangePassword request)
        {
            var result = _repo.DSpQuery<dynamic>($"dbo.spfn_DASHBOARDUSER0A", new Dictionary<string, object>()
            {
                {"parmplid", request.PLID },
                {"parmpgrpid", request.PGRPID },
                {"parmusrename", request.Username },
                {"parmpassword", request.OldPassword },
                {"parmnewpassword", request.Password },
                {"parmconfirmpassword", request.ConfirmPassword }
            }).FirstOrDefault();
            if (result != null)
            {
                var row = ((IDictionary<string, object>)result);
                var ResultCode = row["RESULT"].Str();
                if (ResultCode == "1")
                    return (Results.Success, "Password changed Successfully!");
                else if (ResultCode == "61")
                    return (Results.Failed, "Password did not match");
                else if (ResultCode == "62")
                    return (Results.Failed, "Using an old password as a new password is not allowed. Please try again.");
                else if (ResultCode == "0")
                    return (Results.Failed, "You entered an incorrect old password. Please try again.");
                else if (ResultCode == "21")
                    return (Results.Failed, "You are try to access a blocked account. Please try again.");
                return (Results.Failed, "Failed to Change! your request is already done");
            }
            return (Results.Null, null);
        }
    }
}
