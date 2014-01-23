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
        public string TestLocalContext()
        {
            JustPressPlayDBEntities entities = new JustPressPlayDBEntities();
            entities.user.Find(null);
            return entities.user.Local.Count.ToString();
        }

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

        [HttpPost]
        [RequireHttps]
        public JsonResult Login(string username, string password, string authHash)
        {
            string salt = Request.Url.GetLeftPart(UriPartial.Authority).ToString() + username;
            string paramString = "password=" + password + "&username=" + username;
            string stringToHash = salt + "?" + paramString;

            MobileAppValidationModel response = new MobileAppValidationModel() { Success = false, Message = "" };

            if (!ValidateHash(stringToHash, authHash))
            {
                response.Message = GetLoginResultMessage(LoginValidationResult.FailureHash);
                return Json(response);
            }

            if (Membership.ValidateUser(username, password))
            {
                if (!Roles.IsUserInRole(username, JPPConstants.Roles.AssignIndividualAchievements) && !Roles.IsUserInRole(username, JPPConstants.Roles.FullAdmin))
                {
                    response.Message = GetLoginResultMessage(LoginValidationResult.FailurePermissions);
                    return Json(response);
                }

                UnitOfWork work = new UnitOfWork();
                external_token token = work.SystemRepository.GenerateAuthorizationToken(username, Request.UserHostAddress);

                if (token != null)
                {
                    response.Success = true;
                    response.Message = GetLoginResultMessage(LoginValidationResult.Success);
                    response.Token = token.token;
                    response.Refresh = token.refresh_token;
                    return Json(response);
                }
            }

            response.Message = GetLoginResultMessage(LoginValidationResult.FailureInvalid);

            return Json(response);
        }

        [HttpPost]
        [RequireHttps]
        public JsonResult RemoveAuthorizationToken(string refreshToken, string authHash)
        {
            UnitOfWork work = new UnitOfWork();
            //Actually Check this
            external_token currentToken = work.SystemRepository.GetAuthorizationTokenByRefresh(refreshToken);

            if (currentToken == null)
                return Json(new MobileAppTokenErrorModel() { Success = false, Message = GetTokenValidationResultMessage(TokenValidationResult.FailureInvalid) });

            string salt = currentToken.refresh_token;
            string paramString = "refreshToken=" + refreshToken;
            string stringToHash = salt + "?" + paramString;

            if (!ValidateHash(stringToHash, authHash))
                return Json(new MobileAppTokenErrorModel() { Success = false, Message = GetTokenValidationResultMessage(TokenValidationResult.FailureHash) });


            bool removed = work.SystemRepository.RemoveAuthorizationToken(refreshToken);

            MobileAppTokenErrorModel response = new MobileAppTokenErrorModel() { Success = removed, Message = "" };

            if (removed)
            {
                response.Message = "Token Removed";
            }

            return Json(removed);
        }

        [HttpPost]
        [RequireHttps]
        public JsonResult RefreshAuthorizationToken(string token, string refreshToken, string authHash)
        {
            MobileAppValidationModel response = new MobileAppValidationModel() { Success = false, Message = "" };

            UnitOfWork work = new UnitOfWork();
            external_token currentToken = work.SystemRepository.GetAuthorizationToken(token);

            if (currentToken == null || currentToken.refresh_token != refreshToken)
            {
                response.Message = GetTokenValidationResultMessage(TokenValidationResult.FailureInvalid);
                return Json(response, JsonRequestBehavior.AllowGet);
            }

            string salt = currentToken.refresh_token;
            string paramString = "token=" + token + "&refreshToken=" +refreshToken;
            string stringToHash = salt + "?" + paramString;

            if (!ValidateHash(stringToHash, authHash))
            {
                response.Message = GetTokenValidationResultMessage(TokenValidationResult.FailureHash);
                return Json(response);
            }

            /////////////////////////////////////////////////////////////////////////////////
            currentToken = work.SystemRepository.RefreshAuthorizationToken(token, refreshToken);

            if (currentToken != null)
            {
                response.Success = true;
                response.Message = GetTokenValidationResultMessage(TokenValidationResult.Success);
                response.Token = currentToken.token;
                response.Refresh = currentToken.refresh_token;
                return Json(response);
            }
            else
            {
                response.Message = GetTokenValidationResultMessage(TokenValidationResult.FailureOther);
                return Json(response);
            }
        }

        [HttpPost]
        [RequireHttps]
        public JsonResult ExpireAuthorizationToken(string token, string authHash)
        {
            UnitOfWork work = new UnitOfWork();
            //Actually Check this
            external_token currentToken = work.SystemRepository.GetAuthorizationToken(token);

            if (currentToken == null)
                return Json(new MobileAppTokenErrorModel() { Success = false, Message = GetTokenValidationResultMessage(TokenValidationResult.FailureInvalid) });

            string salt = currentToken.refresh_token;
            string paramString = "token=" + token;
            string stringToHash = salt + "?" + paramString;

            if (!ValidateHash(stringToHash, authHash))
                return Json(new MobileAppTokenErrorModel() { Success = false, Message = GetTokenValidationResultMessage(TokenValidationResult.FailureHash) });


            bool expired = work.SystemRepository.ExpireAuthorizationToken(token);

            MobileAppTokenErrorModel response = new MobileAppTokenErrorModel() { Success = expired, Message = "" };

            if (expired)
            {
                response.Message = "Token Expired";
            }

            return Json(response);
        }

        [HttpPost]
        [RequireHttps]
        public JsonResult GetAchievements(string token, string authHash)
        {
            UnitOfWork work = new UnitOfWork();
            external_token currentToken = work.SystemRepository.GetAuthorizationToken(token);

            if (currentToken == null)
                return Json(new MobileAppTokenErrorModel(){Success = false,Message = GetTokenValidationResultMessage(TokenValidationResult.FailureInvalid)});

            if (DateTime.Now.CompareTo(currentToken.expiration_date) > 0)            
                return Json(new MobileAppTokenErrorModel() { Success = false, Message = GetTokenValidationResultMessage(TokenValidationResult.FailureExpired) });
            

            string salt = currentToken.refresh_token;
            string paramString = "token=" + token;
            string stringToHash = salt + "?" + paramString;

            if (!ValidateHash(stringToHash, authHash))
                return Json(new MobileAppTokenErrorModel() { Success = false, Message = GetTokenValidationResultMessage(TokenValidationResult.FailureHash) });

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
                return Json(mobileAppAchievements);
            }

            return Json(new MobileAppTokenErrorModel() { Success = false, Message = GetLoginResultMessage(LoginValidationResult.FailureNoAchievements) });
        }

        [HttpPost]
        [RequireHttps]
        public JsonResult ReportScan(int aID, bool hasCardToGive, string timeScanned, string token, int userID, string authHash)
        {
            UnitOfWork work = new UnitOfWork();
            external_token currentToken = work.SystemRepository.GetAuthorizationToken(token);

            if (currentToken == null)
            {
                return Json(new MobileAppTokenErrorModel() { Success = false, Message = GetTokenValidationResultMessage(TokenValidationResult.FailureInvalid)});
            }

            if (DateTime.Now.CompareTo(currentToken.expiration_date) > 0)
            {
                return Json(new MobileAppTokenErrorModel() { Success = false, Message = GetTokenValidationResultMessage(TokenValidationResult.FailureExpired)});
            }

            DateTime dt = DateTime.Now;
            if(!DateTime.TryParse(timeScanned, out dt))
                return Json(new MobileAppScanResultModel() { Success = false, Message = "DateTime was invalid", Code = 11});

            string salt = currentToken.refresh_token;
            String paramString = "";
            paramString += "aID="+aID + "&hasCardToGive=" + hasCardToGive.ToString().ToLower()+ "&timeScanned=" + timeScanned + "&token=" + token + "&userID=" + userID;
            string stringToHash = salt + "?" + paramString;

            if (!ValidateHash(stringToHash, authHash))
                return Json(new MobileAppTokenErrorModel() { Success = false, Message = GetTokenValidationResultMessage(TokenValidationResult.FailureExpired) });

            JPPConstants.AssignAchievementResult assignAchievementResult = JPPConstants.AssignAchievementResult.FailureOther;

            assignAchievementResult = work.AchievementRepository.AssignAchievement(userID, aID, currentToken.user_id, true, dt, hasCardToGive);

            MobileAppScanResultModel response = GetAssignResultModel(assignAchievementResult);

            return Json(response);
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

        private MobileAppScanResultModel GetAssignResultModel(JPPConstants.AssignAchievementResult assignAchievementResult)
        {
            switch (assignAchievementResult)
            {
                case JPPConstants.AssignAchievementResult.Success:
                    return new MobileAppScanResultModel()
                    {
                        Success = true,
                        Message = "Achievement Successfully Assigned",
                        Code = 0
                    };
                case JPPConstants.AssignAchievementResult.SuccessNoCard:
                    return new MobileAppScanResultModel()
                    {
                        Success = true,
                        Message = "Achievement Successfully Assigned, Pick Up Card Elsewhere",
                        Code = 0
                    };
                case JPPConstants.AssignAchievementResult.SuccessYesCard:
                    return new MobileAppScanResultModel()
                    {
                        Success = true,
                        Message = "Achievement Successfully Assigned, Give Card",
                        Code = 0
                    };
                case JPPConstants.AssignAchievementResult.SuccessRepetition:
                    return new MobileAppScanResultModel()
                    {
                        Success = true,
                        Message = "Repetition Success",
                        Code = 0
                    };
                case JPPConstants.AssignAchievementResult.SuccessThresholdTriggered:
                    return new MobileAppScanResultModel()
                    {
                        Success = true,
                        Message = "Achievement Successfully Assigned, Threshold Unlocked",
                        Code = 0
                    };
                case JPPConstants.AssignAchievementResult.FailureInvalidAchievement:
                    return new MobileAppScanResultModel()
                    {
                        Success = false,
                        Message = "Achievement Invalid",
                        Code = 0
                    };
                case JPPConstants.AssignAchievementResult.FailureInvalidPlayer:
                    return new MobileAppScanResultModel()
                    {
                        Success = false,
                        Message = "Player Invalid",
                        Code = 0
                    };
                case JPPConstants.AssignAchievementResult.FailureUnauthorizedPlayer:
                    return new MobileAppScanResultModel()
                    {
                        Success = false,
                        Message = "Player Unauthorized",
                        Code = 0
                    };
                case JPPConstants.AssignAchievementResult.FailureInvalidAssigner:
                    return new MobileAppScanResultModel()
                    {
                        Success = false,
                        Message = "Assigner Invalid",
                        Code = 0
                    };
                case JPPConstants.AssignAchievementResult.FailureUnauthorizedAssigner:
                    return new MobileAppScanResultModel()
                    {
                        Success = false,
                        Message = "Assigner Unauthorized",
                        Code = 0
                    };
                case JPPConstants.AssignAchievementResult.FailureAlreadyAchieved:
                    return new MobileAppScanResultModel()
                    {
                        Success = false,
                        Message = "Player already has this achievement",
                        Code = 0
                    };
                case JPPConstants.AssignAchievementResult.FailureRepetitionDelay:
                    return new MobileAppScanResultModel()
                    {
                        Success = false,
                        Message = "Player needs to wait longer to get this achievement again",
                        Code = 0
                    };
                case JPPConstants.AssignAchievementResult.FailureOther:
                    return new MobileAppScanResultModel()
                    {
                        Success = false,
                        Message = "Failure Other",
                        Code = 0
                    };
            }
            return null;

        }
    }   
}
