using LogApp;
using Newtonsoft.Json.Linq;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Entity.Migrations;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;

namespace Database.DAL
{
    public class UserDAL
    {
        SinglePoint_CloudEntities singlePoint_CloudEntities;
        SettingDAL _settingsDAL;
        private string TC_APIUrl;
        private bool AllowTrainingCourses;

        public UserDAL()
        {
            _settingsDAL = new SettingDAL();
            singlePoint_CloudEntities = new SinglePoint_CloudEntities();
            TC_APIUrl = ConfigurationManager.AppSettings["TC_URL"];
            AllowTrainingCourses = ConfigurationManager.AppSettings["AllowTrainingCourses"].Trim() == "" ? true : false;
        }
        public static Users GetCurrentUser()
        {
            Users userDetail = (Users)HttpContext.Current.Session["UserEntity"];
            return userDetail;
        }
        public Users CheckUserAuthentication(string userName, string password)
        {
            try
            {
                Users user = singlePoint_CloudEntities.Users.Where(x => x.UserName == userName).FirstOrDefault();
                bool isPasswordVerified = false;
                if (user.UserPassword != null)
                {
                    isPasswordVerified = Cryptography.Cryptography.VerifyHash(password, user.UserPassword);
                    user.Active = true;
                    if (isPasswordVerified && user.Active)
                    {
                        SecurityDAL securityManagement = new SecurityDAL();
                        securityManagement.UpdateWrongPasswordAttempts(user.UserId, user.WrongPaswordAttempts, true);
                        user.UserAuthenticated = true;
                    }
                    else
                    {
                        SecurityDAL securityManagement = new SecurityDAL();
                        securityManagement.UpdateWrongPasswordAttempts(user.UserId, user.WrongPaswordAttempts, false);
                        user.WrongPaswordAttempts = user.WrongPaswordAttempts + 1;
                        user.UserAuthenticated = false;
                    }
                }
                else
                {
                    user = null;
                }
                return user;
            }
            catch (Exception ex)
            {
                Log4Net.WriteException(ex);
                return null;
            }
        }
        public string LoginUser(Users userEntity)
        {
            //Cryptography.Cryptography.Decrypt("+dOvxno7KM7qOS4zbis7VQ==");
            var LockMesssage = "You have been locked out of the system and only Ghost Digital & Data can unlock you. " +
                    "Please send an email to support@ghost-digital.com stating your name and company.";
            string feedback = null;
            if (userEntity != null)
            {
                if (!userEntity.Active)
                {
                    feedback = LockMesssage;
                    //feedback = "Your account is temporarily disabled.";
                }
                else
                {
                    if (userEntity.UserAuthenticated)
                    {
                        if (Convert.ToBoolean(userEntity.TwoFactorEnabled) && _settingsDAL.IsEnabledTwoFA())
                        {
                            feedback = "3";
                        }
                        else if (!Convert.ToBoolean(userEntity.TwoFactorEnabled) && _settingsDAL.IsEnabledTwoFA())
                        {
                            feedback = "Verification not sent on your email";
                            if (SendEmailVerificationCode(userEntity))
                            {
                                feedback = "4";
                            }
                        }
                        else if (String.IsNullOrWhiteSpace(userEntity.LastPasswordChange.ToString()))
                        {
                            feedback = "2";
                            userEntity.EncryptedUserId = Cryptography.Cryptography.Encrypt(userEntity.UserId.ToString());
                            HttpContext.Current.Session["UserEntity"] = userEntity;
                            ActivityLogDAL.LogActivity("Logged In user (" + userEntity.UserName + ")");
                        }
                        else
                        {
                            //var client = new RestClient(APIUrl + "api/UserService/Users");
                            //client.Timeout = -1;
                            //var request = new RestRequest(Method.GET);
                            //IRestResponse response = client.Execute(request);
                            //Console.WriteLine(response.Content);
                            //int StatusCode = (int)response.StatusCode;
                            feedback = "1";
                            HttpContext.Current.Session["UserEntity"] = userEntity;
                            ActivityLogDAL.LogActivity("Logged In user (" + userEntity.UserName + ")");
                        }
                    }
                    else
                    {
                        if (userEntity.WrongPaswordAttempts < 3)
                        {
                            int remaningAttempts = 3 - userEntity.WrongPaswordAttempts;
                            feedback = "Invalid Username or Password. " + remaningAttempts + " attempt(s) remaining";
                        }
                        else
                        {
                            feedback = LockMesssage;
                            //feedback = "Your account has been temporarily disabled";
                        }
                    }
                }
            }
            else
            {
                feedback = "Invalid Username and Password.";
            }
            return feedback;
        }
        public bool SendEmailVerificationCode(Users userEntity)
        {
            try
            {

                Settings settings = _settingsDAL.GetSettings();

                Random random = new Random();
                string randomNumber = random.Next(1000, 9999).ToString();
                Users user = singlePoint_CloudEntities.Users.Where(x => x.UserName == userEntity.UserName).FirstOrDefault();
                user.TwoFAEmailCode = randomNumber;
                user.TwoFAEmailCodeDateTime = DateTime.Now;
                singlePoint_CloudEntities.SaveChanges();
                MailMessage mail = new MailMessage();
                mail.To.Add(userEntity.UserEmail);
                mail.From = new MailAddress(settings.SMTPUserName);

                string AppName = "Single Point";
                if (Convert.ToInt32(ConfigurationManager.AppSettings["OtherApplication"]) == 1)
                {
                    AppName = "Compendium Framework";

                }
                mail.Subject = AppName + " verification code";

                string URL = ConfigurationManager.AppSettings["SinglePointURL"].ToString() + "/images/heading-image.png";
                string Body = "<div style='margin-bottom: 30px'><img src='" + URL + "' ></div></p>below is the authentication code required for you to login to " + AppName + ".</p> <h1><b>" + randomNumber + "</b></h1><p>If you have any issues obtaining access, please contact your system administrator.</p>";
                mail.Body = Body;

                mail.IsBodyHtml = true;
                SmtpClient smtp = new SmtpClient();
                smtp.Host = settings.SMTPHost; //Or Your SMTP Server Address
                smtp.Port = settings.SMTPPort; //Or Your SMTP Server Address
                smtp.Credentials = new System.Net.NetworkCredential
                     (settings.SMTPUserName, settings.SMTPPassword);
                //Or your Smtp Email ID and Password
                smtp.EnableSsl = true;
                smtp.Send(mail);
                return true;
            }
            catch (Exception ex)
            {
                Log4Net.WriteException(ex);
                return false;
            }
        }

        public List<Users> GetUsers()
        {
            try
            {
                List<Users> users = singlePoint_CloudEntities.Users.ToList();
                users.Select(c => { c.EncryptedUserId = Cryptography.Cryptography.Encrypt(c.UserId.ToString()); return c; }).ToList();
                return users;
            }
            catch (Exception ex)
            {
                Log4Net.WriteException(ex);
                return null;
            }
        }
        public string AddOrUpdateUser(Users user)
        {
            try
            {
                user.UserId = user.EncryptedUserId != "0" ? Convert.ToInt32(Cryptography.Cryptography.Decrypt(user.EncryptedUserId)) : Convert.ToInt32(user.EncryptedUserId);
                Users LoggedInUser = GetCurrentUser();
                string feedback = "1";

                var regex = @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]{8,}$";
                if (user.UserPassword == null)
                { user.UserPassword = ""; }

                Log4Net.WriteLog("user.UserName: " + user.UserName, LogType.GENERALLOG);
             
             

                var match = Regex.Match(user.UserPassword, regex);
                if (IfUsernameAlreadyExist(user.UserName, user.UserId))
                {
                    feedback = "3";
                }
                else if (IfEmailAlreadyExist(user.UserEmail, user.UserId))
                {
                    feedback = "4";
                }
                else if (user.UserId == 0 && !match.Success)
                {
                    feedback = "Passwords must have at least 8 characters with one in uppercase, one lowercase, one number and one special character.";
                }
                else if (user.UserId == 0)
                {
                    Log4Net.WriteLog("Adding new user", LogType.GENERALLOG);
                    byte[] passwordSalt = Cryptography.Cryptography.GenerateSalt();
                    string password = user.UserPassword;
                    user.UserPassword = Cryptography.Cryptography.ComputeHash(user.UserPassword, passwordSalt);
                    user.CreatedBy = LoggedInUser.UserId;
                    user.CreatedOn = DateTime.Now;
                    user.TwoFactorEnabled = false;
                    user.IsGoogleAuthenticatorEnabled = false;
                    singlePoint_CloudEntities.Users.AddOrUpdate(user);

                    if (AllowTrainingCourses)
                    {
                        Log4Net.WriteLog("Adding new user in Training ourse via service : " + TC_APIUrl + "api/UserService/CreateAdmin", LogType.GENERALLOG);
                        // Create Admin User is Training Courses
                        // Json to post.
                        var client = new RestClient(TC_APIUrl + "api/UserService/CreateAdmin");
                        client.Timeout = -1;
                        var request = new RestRequest(Method.POST);
                        request.AddHeader("Content-Type", "application/json");
                        request.AddParameter("application/json", "{\r\n    \"Email\":\"" + user.UserEmail + "\",\r\n    \"Password\":\"Test\",\r\n    \"ConfirmPassword\":\"Test\",\r\n    \"FirstName\":\"" + user.UserFirstName + "\",\r\n    \"LastName\":\"" + user.UserLastName + "\"\r\n}", ParameterType.RequestBody);
                        IRestResponse response = client.Execute(request);
                        int StatusCode = (int)response.StatusCode;
                        Log4Net.WriteLog("Status Code" + StatusCode, LogType.GENERALLOG);
                        if (StatusCode == 200)
                        {
                            singlePoint_CloudEntities.SaveChanges();
                        }
                        else
                        {

                            return "Exception occur while creating user in Training Courses";
                        }
                    }
                    else
                    {
                        Log4Net.WriteLog("NO insertion in TC", LogType.GENERALLOG);
                        singlePoint_CloudEntities.SaveChanges();
                    }
                    ActivityLogDAL.LogActivity("New user added with username (" + user.UserName + ")");
                }
                else
                {
                    Log4Net.WriteLog("Updating user information with user id: " + user.UserId, LogType.GENERALLOG);
                    Users userDetail = singlePoint_CloudEntities.Users.Where(x => x.UserId == user.UserId).SingleOrDefault();
                    string password = "";
                    if (user.UserPassword != null && user.UserPassword.Trim() != "")
                    {
                       password = user.UserPassword;
                        byte[] passwordSalt = Cryptography.Cryptography.GenerateSalt();
                        userDetail.UserPassword = Cryptography.Cryptography.ComputeHash(user.UserPassword, passwordSalt);
                    }

                    userDetail.UserFirstName = user.UserFirstName;
                    userDetail.UserLastName = user.UserLastName;
                    userDetail.UserName = user.UserName;
                    string PreviousEmail = userDetail.UserEmail;
                    userDetail.UserEmail = user.UserEmail;
                    userDetail.Active = user.Active;
                    userDetail.UpdatedOn = DateTime.Now;
                    userDetail.UpdatedBy = LoggedInUser.UserId;
                    singlePoint_CloudEntities.Users.AddOrUpdate(userDetail);

                    if (AllowTrainingCourses)
                    {
                        Log4Net.WriteLog("Adding new user in Training ourse via service : " + TC_APIUrl + "api/UserService/CreateAdmin", LogType.GENERALLOG);
                        // Update Admin User is Training Courses
                        // Json to post.
                        var client = new RestClient(TC_APIUrl + "api/UserService/CreateAdmin");
                        client.Timeout = -1;
                        var request = new RestRequest(Method.POST);
                        request.AddHeader("Content-Type", "application/json");
                        request.AddParameter("application/json", "{\r\n    \"Email\":\"" + user.UserEmail + "$$$" + PreviousEmail + "\",\r\n    \"Password\":\"Test\",\r\n    \"ConfirmPassword\":\"Test\",\r\n    \"FirstName\":\"" + user.UserFirstName + "\",\r\n    \"LastName\":\"" + user.UserLastName + "\"\r\n}", ParameterType.RequestBody);
                        IRestResponse response = client.Execute(request);
                        int StatusCode = (int)response.StatusCode;
                        if (StatusCode == 200)
                        {
                            singlePoint_CloudEntities.SaveChanges();
                        }
                        else
                        {
                            return "Exception occur while updating user in Training Courses";
                        }
                    }
                    else
                    {
                        Log4Net.WriteLog("NO insertion in TC", LogType.GENERALLOG);
                        singlePoint_CloudEntities.SaveChanges();
                    }
                    ActivityLogDAL.LogActivity("User information updated against username (" + user.UserName + ")");
                    Log4Net.WriteLog("feedback: " + feedback, LogType.GENERALLOG);
                    feedback = "2";
                }
                return feedback;
            }
            catch (Exception ex)
            {
                Log4Net.WriteException(ex);
                return "EXception occur";
            }
        }
        public bool DeleteUser(string userID)
        {
            try
            {
                int decryptedUserId = Convert.ToInt32(Cryptography.Cryptography.Decrypt(userID));
                Users getUserDetail = singlePoint_CloudEntities.Users.Where(x => x.UserId == decryptedUserId).SingleOrDefault();
                string username = getUserDetail.UserName;
                singlePoint_CloudEntities.Users.Remove(getUserDetail);

                if (AllowTrainingCourses)
                {
                    // Delete Admin User is Training Courses
                    // Json to post.
                    var client = new RestClient(TC_APIUrl + "api/UserService/DeleteAdmin?Email=" + getUserDetail.UserEmail);
                    client.Timeout = -1;
                    var request = new RestRequest(Method.POST);
                    IRestResponse response = client.Execute(request);
                    int StatusCode = (int)response.StatusCode;
                    if (StatusCode == 200)
                    {
                        singlePoint_CloudEntities.SaveChanges();
                    }
                    else if (StatusCode == 404)
                    {
                        Log4Net.WriteLog("User not found in Training Courses which we trying to delete.", LogType.GENERALLOG);
                        singlePoint_CloudEntities.SaveChanges();
                    }
                    else
                    {
                        return false;
                    }
                }
                else
                {
                    singlePoint_CloudEntities.SaveChanges();
                }
                ActivityLogDAL.LogActivity("User with username (" + username + ") has been deleted");
                return true;
            }
            catch (Exception ex)
            {
                Log4Net.WriteException(ex);
                return false;
            }
        }
        public bool IfUsernameAlreadyExist(string username, int userID)
        {
            try
            {
                bool userExist = false;
                if (userID != 0)
                {
                    if (singlePoint_CloudEntities.Users.Where(x => x.UserName == username && x.UserId != userID).Count() > 0)
                        userExist = true;
                }
                else
                    if (singlePoint_CloudEntities.Users.Where(x => x.UserName == username).Count() > 0)
                    userExist = true;
                return userExist;
            }
            catch (Exception ex)
            {
                Log4Net.WriteException(ex);
                return true;
            }
        }
        private bool IfEmailAlreadyExist(string email, int userID)
        {
            try
            {
                bool userExist = false;
                if (userID != 0)
                {
                    if (singlePoint_CloudEntities.Users.Where(x => x.UserEmail == email && x.UserId != userID).Count() > 0)
                        userExist = true;
                }
                else
                    if (singlePoint_CloudEntities.Users.Where(x => x.UserEmail == email).Count() > 0)
                    userExist = true;
                return userExist;
            }
            catch (Exception ex)
            {
                Log4Net.WriteException(ex);
                return true;
            }
        }
        public bool VerifyEmailCode(Users userEntity, string code)
        {
            try
            {
                bool ifVerified = false;

                if (singlePoint_CloudEntities.Users.Where(x => x.UserName == userEntity.UserName && x.TwoFAEmailCode == code).Count() > 0)
                {
                    ifVerified = true;
                }
                return ifVerified;
            }
            catch (Exception ex)
            {
                Log4Net.WriteException(ex);
                return false;
            }
        }
        public bool Reset2FA(Users userEntity)
        {
            try
            {
                Users user = singlePoint_CloudEntities.Users.Where(x => x.UserId == userEntity.UserId).FirstOrDefault();
                if (user != null)
                {
                    user.IsGoogleAuthenticatorEnabled = false;
                    user.GoogleAuthenticatorSecretKey = null;
                    user.TwoFactorEnabled = false;
                    singlePoint_CloudEntities.SaveChanges();
                }
                if (SendEmailVerificationCode(userEntity))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception ex)
            {
                Log4Net.WriteException(ex);
                return false;
            }
        }
    }
}
