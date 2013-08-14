using JustPressPlay.Models;
using JustPressPlay.Models.Repositories;
using JustPressPlay.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;

namespace JustPressPlay.Controllers
{
    public class MobileAppController : Controller
    {
        [DataContract]
        private class MobileAppValidationModel
        {
            [DataMember(Name = "success")]
            public Boolean Success { get; set; }

            [DataMember(Name = "message")]
            public String Message { get; set; }

            [DataMember(Name = "token")]
            public String Token { get; set; }

            [DataMember(Name = "refresh")]
            public String Refresh { get; set; }
        }

        [DataContract]
        private class MobileAppTokenErrorModel
        {
            [DataMember(Name = "success")]
            public Boolean Success { get; set; }

            [DataMember(Name = "message")]
            public String Message { get; set; }
        }

        [DataContract]
        private class MobileAppAchievementModel
        {
            [DataMember(Name = "aID")]
            public int AchievementID { get; set; }

            [DataMember(Name = "name")]
            public String Title { get; set; }

            [DataMember(Name = "icon")]
            public String Icon { get; set; }
        }

        [DataContract]
        private class MobileAppScanResultModel
        {
            [DataMember(Name = "success")]
            public Boolean Success { get; set; }

            [DataMember(Name = "message")]
            public String Message { get; set; }

            [DataMember(Name = "code")]
            public int Code { get; set; }
        }
        private enum LoginValidationResult
        {
            Success,
            FailureInvalid,
            FailurePermissions,
            FailureNoAchievements,
            FailureHash,
            FailureOther
        }

        private enum TokenValidationResult
        {
            Success,
            FailureInvalid,
            FailureExpired,
            FailureHash,
            FailureOther
        }

        private enum ScanResult
        {
            Success,
            SuccessNoCard,
            SuccessYesCard,

            SuccessRepetition,

            SuccessThresholdTriggered,
            SuccessThresholdTriggeredCard,

            FailureAlreadyAchieved,
            FailureInvalidPlayer,
            FailureRepetition,
            FailureUnauthorized,
            FailureHash,
            FailureOther
        }
        //TODO: ADD REFRESH COLUMN TO DATABASE
        //TODO: RETURN STUFF CORRECTLY, POST ACTION HTTPS
        public JsonResult Login(string username, string password, string authHash)
        {
            string salt = Request.Url.GetLeftPart(UriPartial.Authority).ToString() + username;
            string paramString = "password=" + password + "&username=" + username;
            string stringToHash = salt + "?" + paramString;

            MobileAppValidationModel response = new MobileAppValidationModel() { Success = false, Message = "" };

            /*if (!ValidateHash(stringToHash, authHash))
            {
                response.Message = GetLoginResultMessage(LoginValidationResult.FailureHash);
                return Json(response, JsonRequestBehavior.AllowGet);
            }*/

            if (Membership.ValidateUser(username, password))
            {
                if (!Roles.IsUserInRole(username, JPPConstants.Roles.AssignIndividualAchievements) && !Roles.IsUserInRole(username, JPPConstants.Roles.FullAdmin))
                {
                    response.Message = GetLoginResultMessage(LoginValidationResult.FailurePermissions);
                    return Json(response, JsonRequestBehavior.AllowGet);
                }

                UnitOfWork work = new UnitOfWork();
                external_token token = work.SystemRepository.GenerateAuthorizationToken(username, Request.UserHostAddress);

                if (token != null)
                {
                    response.Success = true;
                    response.Message = GetLoginResultMessage(LoginValidationResult.Success);
                    response.Token = token.token;
                    response.Refresh = token.token;
                    return Json(response, JsonRequestBehavior.AllowGet);
                }
            }

            response.Message = GetLoginResultMessage(LoginValidationResult.FailureInvalid);

            return Json(response, JsonRequestBehavior.AllowGet);
        }

        public JsonResult RemoveAuthorizationToken(string refresh, string authHash)
        {
            UnitOfWork work = new UnitOfWork();
            //Actually Check this
            external_token currentToken = work.SystemRepository.GetAuthorizationToken(refresh);

            if (currentToken == null)
                return null;

            string salt = currentToken.token;
            string paramString = "token=" + refresh;
            string stringToHash = salt + "?" + paramString;

            //if (!ValidateHash(stringToHash, authHash))
                //return null;


            bool removed = work.SystemRepository.RemoveAuthorizationToken(refresh);
            return Json(removed, JsonRequestBehavior.AllowGet);
        }

        public JsonResult RefreshAuthorizationToken(string token, string refreshToken, string authHash)
        {
            MobileAppValidationModel response = new MobileAppValidationModel() { Success = false, Message = "" };

            UnitOfWork work = new UnitOfWork();
            external_token currentToken = work.SystemRepository.GetAuthorizationToken(token);

            if (currentToken == null || currentToken.token != refreshToken)
            {
                response.Message = GetTokenValidationResultMessage(TokenValidationResult.FailureInvalid);
                return Json(response, JsonRequestBehavior.AllowGet);
            }

            string salt = currentToken.token;
            string paramString = "token=" + token + "&refreshToken=" +refreshToken;
            string stringToHash = salt + "?" + paramString;

            /*if (!ValidateHash(stringToHash, authHash))
            {
                response.Message = GetTokenValidationResultMessage(TokenValidationResult.FailureHash);
                return Json(response, JsonRequestBehavior.AllowGet);
            }*/

            /////////////////////////////////////////////////////////////////////////////////
            currentToken = work.SystemRepository.RefreshAuthorizationToken(token);

            if (currentToken != null)
            {
                response.Success = true;
                response.Message = GetTokenValidationResultMessage(TokenValidationResult.Success);
                response.Token = currentToken.token;
                response.Refresh = currentToken.token;
                return Json(response, JsonRequestBehavior.AllowGet);
            }
            else
            {
                response.Message = GetTokenValidationResultMessage(TokenValidationResult.FailureOther);
                return Json(response, JsonRequestBehavior.AllowGet);
            }
        }

        public JsonResult GetAchievements(string token, string authHash)
        {
            UnitOfWork work = new UnitOfWork();
            external_token currentToken = work.SystemRepository.GetAuthorizationToken(token);

            if (currentToken == null)
            {
                return Json(new MobileAppTokenErrorModel()
                {Success = false,Message = GetTokenValidationResultMessage(TokenValidationResult.FailureInvalid)},
                JsonRequestBehavior.AllowGet);
            }

            if (DateTime.Now.CompareTo(currentToken.expiration_date) > 0)
            {
                return Json(new MobileAppTokenErrorModel() { Success = false, Message = GetTokenValidationResultMessage(TokenValidationResult.FailureExpired) },
                JsonRequestBehavior.AllowGet);
            }

            string salt = currentToken.token;
            string paramString = "token=" + token;
            string stringToHash = salt + "?" + paramString;

            /*if (!ValidateHash(stringToHash, authHash))
                return Json(new MobileAppTokenErrorModel() { Success = false, Message = GetTokenValidationResultMessage(TokenValidationResult.FailureHash) },
               JsonRequestBehavior.AllowGet);*/

            bool isFullAdmin = Roles.IsUserInRole(currentToken.user.username, JPPConstants.Roles.FullAdmin);
            List<achievement_template> assignableAchievements = work.AchievementRepository.GetAssignableAchievements(currentToken.user_id, isFullAdmin);

            if(assignableAchievements != null && assignableAchievements.Count >= 0)
            {
                List<MobileAppAchievementModel> mobileAppAchievements = new List<MobileAppAchievementModel>();
                foreach (achievement_template achievement in assignableAchievements)
                {
                    MobileAppAchievementModel mobileAchievement = new MobileAppAchievementModel() { AchievementID = achievement.id, Icon = achievement.icon, Title = achievement.title };
                    mobileAppAchievements.Add(mobileAchievement);                    
                }

                return Json(mobileAppAchievements, JsonRequestBehavior.AllowGet);
            }

            return Json(new MobileAppTokenErrorModel() { Success = false, Message = GetLoginResultMessage(LoginValidationResult.FailureNoAchievements) },
            JsonRequestBehavior.AllowGet);
        }

        public JsonResult ReportScan(int aID, bool hasCardToGive, string timeScanned, string token, int userID, string authHash)
        {
            UnitOfWork work = new UnitOfWork();
            external_token currentToken = work.SystemRepository.GetAuthorizationToken(token);

            if (currentToken == null)
                return null;

            string salt = currentToken.token;
            String paramString = "";
            paramString += "aID="+aID + "&hasCardToGive=" + hasCardToGive.ToString().ToLower()+ "&timeScanned=" + timeScanned + "&token=" + token + "&userID=" + userID;
            string stringToHash = salt + "?" + paramString;

            //if (!ValidateHash(stringToHash, authHash))
              //  return null;

            work.AchievementRepository.AssignScanAchievement(userID, aID, currentToken.user_id, DateTime.Now, hasCardToGive);

            MobileAppScanResultModel response = new MobileAppScanResultModel()
            {
                Success = true,
                Message = "Testing",
                Code = 5
            };

            return Json(response, JsonRequestBehavior.AllowGet);
        }        

        public void GetTheme()
        {
        }

        private bool ValidateHash(string stringToHash, string authHash)
        {
            SHA256Managed sha = new SHA256Managed();
            byte[] hash = sha.ComputeHash(Encoding.UTF8.GetBytes(stringToHash));
            String myAuthHash = Convert.ToBase64String(hash);

            return authHash == myAuthHash;
        }

        private String GetLoginResultMessage(LoginValidationResult loginResult)
        {
            switch (loginResult)
            {
                case LoginValidationResult.Success:
                    return "Success";
                case LoginValidationResult.FailureInvalid:
                    return "Invalid Credentials";
                case LoginValidationResult.FailurePermissions:
                    return "You are not authorized to use this app";
                case LoginValidationResult.FailureNoAchievements:
                    return "There are no achievements that you can assign at this time";
                case LoginValidationResult.FailureHash:
                    return "Hash Failure";
                default:
                    return "Unknown Error";
            }
        }

        private String GetTokenValidationResultMessage(TokenValidationResult tokenResult)
        {
            switch (tokenResult)
            {
                case TokenValidationResult.Success:
                    return "Success";
                case TokenValidationResult.FailureExpired:
                    return "Expired Token";
                case TokenValidationResult.FailureInvalid:
                    return "Invalid Token";
                case TokenValidationResult.FailureHash:
                    return "Hash Failure";
                default:
                    return "Unknown Error";
            }
        }
    }   
}
