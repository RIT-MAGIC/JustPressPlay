using JustPressPlay.Models;
using JustPressPlay.Models.Repositories;
using JustPressPlay.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;

namespace JustPressPlay.Controllers
{
    public class MobileAppController : Controller
    {
        //
        // GET: /MobileApp/

       /* public ActionResult Index()
        {
            return View();
        }*/

        //TODO: RETURN STUFF CORRECTLY
        public JsonResult Login(string username, string password, string authHash)
        {
            string salt = Request.Url.GetLeftPart(UriPartial.Authority).ToString() + username;
            string paramString = "password=" + password + "&username=" + username;
            string stringToHash = salt + "?" + paramString;

            if (!ValidateHash(stringToHash, authHash))
                return null;

            if (Membership.ValidateUser(username, password))
            {
                UnitOfWork work = new UnitOfWork();

                external_token token = work.SystemRepository.GenerateAuthorizationToken(username, Request.UserHostAddress);

                TokenModel tokenModel = new TokenModel()
                {
                    Token = token.token,
                    RefreshToken = token.token
                };

                return Json(tokenModel, JsonRequestBehavior.AllowGet);
            }

            return Json("Login Failed", JsonRequestBehavior.AllowGet);
        }

        public JsonResult RemoveAuthorizationToken(string token, string authHash)
        {
            UnitOfWork work = new UnitOfWork();
            //Actually Check this
            external_token currentToken = work.SystemRepository.GetAuthorizationToken(token);

            if (currentToken == null)
                return null;

            string salt = currentToken.token;
            string paramString = "token=" + token;
            string stringToHash = salt + "?" + paramString;

            if (!ValidateHash(stringToHash, authHash))
                return null;


            bool removed = work.SystemRepository.RemoveAuthorizationToken(token);
            return Json(removed, JsonRequestBehavior.AllowGet);
        }

        public JsonResult RefreshAuthorizationToken(string token, string authHash)
        {
            UnitOfWork work = new UnitOfWork();
            external_token currentToken = work.SystemRepository.GetAuthorizationToken(token);

            if (currentToken == null)
                return null;

            string salt = currentToken.token;
            string paramString = "token=" + token;
            string stringToHash = salt + "?" + paramString;

            if (!ValidateHash(stringToHash, authHash))
                return null;

            /////////////////////////////////////////////////////////////////////////////////
            external_token newToken = work.SystemRepository.RefreshAuthorizationToken(token);

            TokenModel tokenModel = new TokenModel()
            {
                Token = newToken.token,
                RefreshToken = newToken.token
            };
            return Json(token, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetAchievements(string token, string authHash)
        {
            UnitOfWork work = new UnitOfWork();
            external_token currentToken = work.SystemRepository.GetAuthorizationToken(token);

            if (currentToken == null)
                return null;

            string salt = currentToken.token;
            string paramString = "token=" + token;
            string stringToHash = salt + "?" + paramString;

            if (!ValidateHash(stringToHash, authHash))
                return null;

            return null;
        }

        public JsonResult ReportScan(int aID, bool? hasCardToGive, string timeScanned, string token, string userID, string authHash)
        {
            UnitOfWork work = new UnitOfWork();
            external_token currentToken = work.SystemRepository.GetAuthorizationToken(token);

            if (currentToken == null)
                return null;

            string salt = currentToken.token;
            String paramString = "";
            paramString += "aID="+aID;
            if (hasCardToGive != null) paramString += "&hasCardToGive=" + hasCardToGive.Value.ToString().ToLower();
            paramString += "&timeScanned=" + timeScanned + "&token=" + token + "&userID=" + userID;
            string stringToHash = salt + "?" + paramString;

            if (!ValidateHash(stringToHash, authHash))
                return null;

            return null;
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

    }

    public class TokenModel
    {
        public String Token { get; set; }
        public String RefreshToken { get; set; }
    }
}
