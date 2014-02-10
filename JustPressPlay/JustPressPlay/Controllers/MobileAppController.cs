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
        #region Models

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
        private class MobileAppThemeModel
        {
            [DataMember(Name = "success")]
            public Boolean Success { get; set; }

            [DataMember(Name = "message")]
            public String Message { get; set; }

            [DataMember(Name = "schoolName")]
            public String SchoolName { get; set; }

            [DataMember(Name = "navColor")]
            public String NavColor { get; set; }

            [DataMember(Name = "iconURL")]
            public String IconURL { get; set; }

            [DataMember(Name = "cardsEnabled")]
            public Boolean CardsEnabled { get; set; }
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

        #endregion

        #region Login and Theme

        /// <summary>
        /// Login from the Mobile App
        /// Successful login returns a token that will get passed
        /// in future requests to prevent having to send username
        /// and password multiple times
        /// </summary>
        /// <param name="username">The username of the person logging in</param>
        /// <param name="password">The user's password for their account</param>
        /// <param name="authHash">Hash to double check against for security</param>
        /// <returns>{"Success":true/false, "Message":"", "Token":"", "Refresh":""}</returns>
        [HttpPost]
        [RequireHttps]
        public JsonResult Login(string username, string password, string authHash)
        {
            //Create the response model
            MobileAppValidationModel response = new MobileAppValidationModel() { Success = false, Message = "" };

            /*---------------------------------Hash Validation Begin-----------------------------------*/
            #region Hash Validation
            //Build the string that will be hashed
            string salt = Request.Url.GetLeftPart(UriPartial.Authority).ToString() + username;
            string paramString = "password=" + password + "&username=" + username;
            string stringToHash = salt + "?" + paramString;

            //Invalid hash
            if (!ValidateHash(stringToHash, authHash))
            {
                response.Message = GetLoginResultMessage(LoginValidationResult.FailureHash);
                return Json(response);
            }
            #endregion
            /*----------------------------------Hash Validation End------------------------------------*/

            //Attempt to validate the user
            if (Membership.ValidateUser(username, password))
            {
                //Check the user's roles to see if they have permission to assign achievments
                if (!Roles.IsUserInRole(username, JPPConstants.Roles.AssignIndividualAchievements) && !Roles.IsUserInRole(username, JPPConstants.Roles.FullAdmin))
                {
                    response.Message = GetLoginResultMessage(LoginValidationResult.FailurePermissions);
                    return Json(response);
                }

                //Create a new token for the user
                UnitOfWork work = new UnitOfWork();
                external_token token = work.SystemRepository.GenerateAuthorizationToken(username, Request.UserHostAddress);

                //Make sure the token exists
                if (token == null)
                {
                    response.Success = false;
                    response.Message = GetLoginResultMessage(LoginValidationResult.FailureOther);
                }
                else
                {
                    response.Success = true;
                    response.Message = GetLoginResultMessage(LoginValidationResult.Success);
                    response.Token = token.token;
                    response.Refresh = token.refresh_token;                    
                }

                //Return Success if token exists or FailureOther if token was null
                return Json(response);
            }

            //Invalid username/password
            response.Message = GetLoginResultMessage(LoginValidationResult.FailureInvalid);
            return Json(response);
        }

        /// <summary>
        /// Returns the "theme" for the current instance to the site
        /// </summary>
        /// <param name="token">The token that was generated upon logging in</param>
        /// <param name="authHash">Hash to double check against for security</param>
        /// <returns>{"Success":true/false, "Message":"", "SchoolName":"", "NavColor":"","IconURL":"", "CardsEnabled":true/false} </returns>
        [HttpPost]
        [RequireHttps]
        public JsonResult GetTheme(string token, string authHash)
        {
            /*---------------------------------Token Validation Begin-----------------------------------*/
            #region Validate the Token
            //Get the current token from the database
            UnitOfWork work = new UnitOfWork();
            external_token currentToken = work.SystemRepository.GetAuthorizationToken(token);

            //Invalid token
            if (currentToken == null)
                return Json(new MobileAppTokenErrorModel() { Success = false, Message = GetTokenValidationResultMessage(TokenValidationResult.FailureInvalid) });

            //Token has expired
            if (DateTime.Now.CompareTo(currentToken.expiration_date) > 0)
                return Json(new MobileAppTokenErrorModel() { Success = false, Message = GetTokenValidationResultMessage(TokenValidationResult.FailureExpired) });

            //Build the string that will be hashed
            string salt = currentToken.refresh_token;
            string paramString = "token=" + token;
            string stringToHash = salt + "?" + paramString;

            //Invalid hash
            if (!ValidateHash(stringToHash, authHash))
                return Json(new MobileAppTokenErrorModel() { Success = false, Message = GetTokenValidationResultMessage(TokenValidationResult.FailureHash) });

            #endregion
            /*----------------------------------Token Validation End------------------------------------*/

            //Return the theme
            return Json(GetThemeModel(), JsonRequestBehavior.AllowGet);
        }

        #endregion

        #region Token Management

        /// <summary>
        /// Removes the specified authorization token from the database
        /// Will require the user to login again
        /// </summary>
        /// <param name="refreshToken">The refresh token that was passed to the user upon login</param>
        /// <param name="authHash">Hash to double check against for security</param>
        /// <returns>{"Success":true/false, "Message":""}</returns>
        [HttpPost]
        [RequireHttps]
        public JsonResult RemoveAuthorizationToken(string refreshToken, string authHash)
        {
            //Create the response model
            MobileAppTokenErrorModel response = new MobileAppTokenErrorModel() { Success = false, Message = "" };

            /*---------------------------------Token Validation Begin-----------------------------------*/
            #region Validate the Token

            //Get the currentToken
            UnitOfWork work = new UnitOfWork();
            external_token currentToken = work.SystemRepository.GetAuthorizationTokenByRefresh(refreshToken);

            //Invalid token
            if (currentToken == null)
            {
                response.Message = GetTokenValidationResultMessage(TokenValidationResult.FailureInvalid);
                return Json(response);
            }

            //Build the string to be hashed
            string salt = currentToken.refresh_token;
            string paramString = "refreshToken=" + refreshToken;
            string stringToHash = salt + "?" + paramString;

            //Invalid hash
            if (!ValidateHash(stringToHash, authHash))
            {
                response.Message = GetTokenValidationResultMessage(TokenValidationResult.FailureHash);
                return Json(response);
            }
            #endregion
            /*----------------------------------Token Validation End------------------------------------*/

            //Remove the token from the database
            bool removed = work.SystemRepository.RemoveAuthorizationToken(refreshToken);

            //Populate the model's message
            if (removed)
                response.Message = "Token Removed";
            else
                response.Message = "Unknown Error";

            //Return
            return Json(response);
        }

        /// <summary>
        /// Refreshes an expired authorization token
        /// Generates a new string for token and refresh_token
        /// </summary>
        /// <param name="token">The token to be refreshed</param>
        /// <param name="refreshToken">The refresh token associated with the token</param>
        /// <param name="authHash">Hash to double check against for security</param>
        /// <returns>{"Success":true/false, "Message":"", "Token":"", "Refresh":""}</returns>
        [HttpPost]
        [RequireHttps]
        public JsonResult RefreshAuthorizationToken(string token, string refreshToken, string authHash)
        {
            //Create the response model
            MobileAppValidationModel response = new MobileAppValidationModel() { Success = false, Message = "" };

            /*---------------------------------Token Validation Begin-----------------------------------*/
            #region Validate the Token

            //Get the current token from the database
            UnitOfWork work = new UnitOfWork();
            external_token currentToken = work.SystemRepository.GetAuthorizationToken(token);

            //Invalid token
            if (currentToken == null || currentToken.refresh_token != refreshToken)
            {
                response.Message = GetTokenValidationResultMessage(TokenValidationResult.FailureInvalid);
                return Json(response, JsonRequestBehavior.AllowGet);
            }

            //Build the string to be hashed
            string salt = currentToken.refresh_token;
            string paramString = "token=" + token + "&refreshToken=" +refreshToken;
            string stringToHash = salt + "?" + paramString;

            //Invalid hash
            if (!ValidateHash(stringToHash, authHash))
            {
                response.Message = GetTokenValidationResultMessage(TokenValidationResult.FailureHash);
                return Json(response);
            }
            #endregion
            /*----------------------------------Token Validation End------------------------------------*/

            //Refresh the token
            currentToken = work.SystemRepository.RefreshAuthorizationToken(token, refreshToken);

            //Build the response and return
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

        /// <summary>
        /// Expires the specified authorization token
        /// Further requests will fail until the token is refreshed
        /// </summary>
        /// <param name="token">The token to be expired</param>
        /// <param name="authHash">Hash to double check against for security</param>
        /// <returns></returns>
        [HttpPost]
        [RequireHttps]
        public JsonResult ExpireAuthorizationToken(string token, string authHash)
        {
            /*---------------------------------Token Validation Begin-----------------------------------*/
            #region Validate the Token

            //Get the current token
            UnitOfWork work = new UnitOfWork();
            external_token currentToken = work.SystemRepository.GetAuthorizationToken(token);

            //Invalid token
            if (currentToken == null)
                return Json(new MobileAppTokenErrorModel() { Success = false, Message = GetTokenValidationResultMessage(TokenValidationResult.FailureInvalid) });

            //Build the string to be hashed
            string salt = currentToken.refresh_token;
            string paramString = "token=" + token;
            string stringToHash = salt + "?" + paramString;

            //Invalid hash
            if (!ValidateHash(stringToHash, authHash))
                return Json(new MobileAppTokenErrorModel() { Success = false, Message = GetTokenValidationResultMessage(TokenValidationResult.FailureHash) });

            #endregion
            /*----------------------------------Token Validation End------------------------------------*/

            //Expire the token
            bool expired = work.SystemRepository.ExpireAuthorizationToken(token);

            //Build the response
            MobileAppTokenErrorModel response = new MobileAppTokenErrorModel() { Success = expired, Message = "Unknown Error" };
            if (expired)
                response.Message = "Token Expired";

            //Return
            return Json(response);
        }

        #endregion

        #region Get and Report Achievements

        /// <summary>
        /// Gets a list of achievements the user can assign
        /// </summary>
        /// <param name="token">The authorization token for the user</param>
        /// <param name="authHash">Hash to double check against for security</param>
        /// <returns>List{"AchievementID":int, "Icon":"", "Title":""} OR {"Success":false, "Message":""}</returns> 
        [HttpPost]
        [RequireHttps]
        public JsonResult GetAchievements(string token, string authHash)
        {
            /*---------------------------------Token Validation Begin-----------------------------------*/
            #region Validate the Token
            //Get the current token from the database
            UnitOfWork work = new UnitOfWork();
            external_token currentToken = work.SystemRepository.GetAuthorizationToken(token);

            //Invalid token
            if (currentToken == null)
                return Json(new MobileAppTokenErrorModel(){Success = false,Message = GetTokenValidationResultMessage(TokenValidationResult.FailureInvalid)});
            
            //Expired token
            if (DateTime.Now.CompareTo(currentToken.expiration_date) > 0)            
                return Json(new MobileAppTokenErrorModel() { Success = false, Message = GetTokenValidationResultMessage(TokenValidationResult.FailureExpired) });
            
            //Build the string to be hashed
            string salt = currentToken.refresh_token;
            string paramString = "token=" + token;
            string stringToHash = salt + "?" + paramString;

            //Invalid hash
            if (!ValidateHash(stringToHash, authHash))
                return Json(new MobileAppTokenErrorModel() { Success = false, Message = GetTokenValidationResultMessage(TokenValidationResult.FailureHash) });
            #endregion
            /*----------------------------------Token Validation End------------------------------------*/

            //If the user has full Admin permissions, return all active achievements
            //If not, return all active achievements they are a caretaker of
            bool isFullAdmin = Roles.IsUserInRole(currentToken.user.username, JPPConstants.Roles.FullAdmin);
            List<achievement_template> assignableAchievements = work.AchievementRepository.GetAssignableAchievements(currentToken.user_id, isFullAdmin);

            //Create the list of achievements to return
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

            //The user cannot assign any achievements
            return Json(new MobileAppTokenErrorModel() { Success = false, Message = GetLoginResultMessage(LoginValidationResult.FailureNoAchievements) });
        }

        /// <summary>
        /// Assigns the specified achievement to the specified player
        /// </summary>
        /// <param name="aID">The ID of the achievement</param>
        /// <param name="hasCardToGive">Was there a card given?</param>
        /// <param name="timeScanned">The DateTime the playpass was scanned</param>
        /// <param name="token">The token for the current user</param>
        /// <param name="userID">The ID of the player who gets the achievement</param>
        /// <param name="authHash">Hash to double check against for security</param>
        /// <returns>{"Success":true/false, "Message":"", "Code":int}</returns>
        [HttpPost]
        [RequireHttps]
        public JsonResult ReportScan(int aID, bool hasCardToGive, string timeScanned, string token, int userID, string authHash)
        {
            /*---------------------------------Token Validation Begin-----------------------------------*/
            #region Validate the Token

            //Get the current token
            UnitOfWork work = new UnitOfWork();
            external_token currentToken = work.SystemRepository.GetAuthorizationToken(token);

            //Invalid token
            if (currentToken == null)
                return Json(new MobileAppTokenErrorModel() { Success = false, Message = GetTokenValidationResultMessage(TokenValidationResult.FailureInvalid)});

            //Expired token
            if (DateTime.Now.CompareTo(currentToken.expiration_date) > 0)
                return Json(new MobileAppTokenErrorModel() { Success = false, Message = GetTokenValidationResultMessage(TokenValidationResult.FailureExpired)});

            //Make sure the date passed in is valid
            DateTime dt = DateTime.Now;
            if(!DateTime.TryParse(timeScanned, out dt))
                return Json(new MobileAppScanResultModel() { Success = false, Message = "DateTime was invalid", Code = 11});

            //Build the string to be hashed
            string salt = currentToken.refresh_token;
            String paramString = "";
            paramString += "aID="+aID + "&hasCardToGive=" + hasCardToGive.ToString().ToLower()+ "&timeScanned=" + timeScanned + "&token=" + token + "&userID=" + userID;
            string stringToHash = salt + "?" + paramString;

            //Invalid hash
            if (!ValidateHash(stringToHash, authHash))
                return Json(new MobileAppTokenErrorModel() { Success = false, Message = GetTokenValidationResultMessage(TokenValidationResult.FailureExpired) });

            #endregion
            /*----------------------------------Token Validation End------------------------------------*/

            //Default AchievementResult is falilure
            JPPConstants.AssignAchievementResult assignAchievementResult = JPPConstants.AssignAchievementResult.FailureOther;

            //Assign the achievement and get the result
            assignAchievementResult = work.AchievementRepository.AssignAchievement(userID, aID, currentToken.user_id, true, dt, hasCardToGive);

            //Create the response model
            MobileAppScanResultModel response = GetAssignResultModel(assignAchievementResult);

            //Return
            return Json(response);
        }

        #endregion

        #region Helper Methods

        /// <summary>
        /// Checks the authHash passed in against one built on this side
        /// </summary>
        /// <param name="stringToHash">The string that was built on our end</param>
        /// <param name="authHash">The hashed string that was passed in with the request</param>
        /// <returns>true if the hashes are equal, false if they are not</returns>
        private bool ValidateHash(string stringToHash, string authHash)
        {
            SHA256Managed sha = new SHA256Managed();
            byte[] hash = sha.ComputeHash(Encoding.UTF8.GetBytes(stringToHash));
            String myAuthHash = Convert.ToBase64String(hash);

            return authHash == myAuthHash;
        }

        /// <summary>
        /// Gets the theme for the current site instance from the site settings
        /// </summary>
        /// <returns>MobileAppThemeModel</returns>
        private MobileAppThemeModel GetThemeModel()
        {
            MobileAppThemeModel model = new MobileAppThemeModel();

            model.Success = true;
            model.SchoolName = JPPConstants.SiteSettings.GetValue(JPPConstants.SiteSettings.SchoolName);
            model.NavColor = JPPConstants.SiteSettings.GetValue(JPPConstants.SiteSettings.ColorNavBar);
            model.IconURL = JPPConstants.SiteSettings.GetValue(JPPConstants.SiteSettings.SchoolLogo);
            model.CardsEnabled = Convert.ToBoolean(JPPConstants.SiteSettings.GetValue(JPPConstants.SiteSettings.CardDistributionEnabled));

            return model;
        }

        /// <summary>
        /// Creates the approriate response model based on the result of assigning the achievement
        /// </summary>
        /// <param name="assignAchievementResult">The result of the achievement assigning</param>
        /// <returns>MobileAppScanResultModel</returns>
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

        /// <summary>
        /// Gets the Message for the specified login result
        /// </summary>
        /// <param name="loginResult">The LoginValidationResult</param>
        /// <returns>String Message</returns>
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

        /// <summary>
        /// Gets the Message for the specified Token Validation Result
        /// </summary>
        /// <param name="tokenResult">TokenValidationResult</param>
        /// <returns>String Message</returns>
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

        #endregion
    }   
}
