using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Security.Claims;
using System.Threading.Tasks;
using LMS.Model.ViewModel.Shared;
using LMS.Repository.DataModel.Shared;
using LMS.Repository.Interfaces.Auth;
using LMS.Utility.Exceptions;
using LMS.Utility.ExtendedMethods;
using LMS.Utility.Helpers;
//using Microsoft.AspNetCore.Authentication;
//using Microsoft.AspNetCore.Authentication.Cookies;
//using Microsoft.AspNetCore.Hosting;
//using Microsoft.AspNetCore.Http;
//using Microsoft.AspNetCore.Mvc;
//using Microsoft.Extensions.Configuration;
using System.Web.Mvc;
using System.Configuration;
using System.Web.Security;
using LMS.Repository.Repositories.Auth;

namespace LMS.WEBDOTNET.Controllers
{
    public class AuthController : Controller
    {
        // private readonly IHostingEnvironment _hostingEnvironment;
        // private readonly IConfiguration _configuration;
        private IAuthRepository _authRepository = new AuthRepository();
        //var container = Configuration.IoCBuilder.Build();
        //public AuthController( IAuthRepository authRepository)
        //{
        //  _hostingEnvironment = hostingEnvironment;
        //   _configuration = configuration;
        //_authRepository = authRepository;
        //}
        public ActionResult Index()
        {
            return View();
        }
        [HttpPost]
        public ActionResult Login(UserViewModel emp)
        {
            try
            {
                if (emp != null && emp.Email != null && emp.Password != null)
                {
                    //var resp = _authRepository.Login(emp.Email.Trim(), emp.Password);
                    //var resp = _authRepository.Login(emp.Email.Trim(), emp.Password)
                    //using (true)
                    //{
                    var resp = _authRepository.Login(emp.Email.Trim(), emp.Password);
                    var u = new UserViewModel();
                    if (resp != null)
                    {
                        u.UserId = Convert.ToInt32(resp["UserId"]);
                        u.FirstName = Convert.ToString(resp["FirstName"]);
                        u.LastName = Convert.ToString(resp["LastName"]);
                        u.Mobile = Convert.ToString(resp["MobileNo"]);
                        u.Email = Convert.ToString(resp["Email"]);
                        u.RoleName = Convert.ToString(resp["RoleName"]);
                        u.PasswordExpirayDate = Convert.IsDBNull(resp["PasswordExpiryDate"]) ? "" : Convert.ToString(resp["PasswordExpiryDate"]);
                        u.IsApproved = Convert.ToString(resp["IsApproved"]);
                        u.ProfilePic = Convert.ToString(resp["ProfilePic"]);
                        u.PasswordHash = resp["PasswordHash"] as byte[];
                        u.PasswordSalt = resp["PasswordSalt"] as byte[];
                    }

                    if (u != null)
                    {
                        if (VerifyPassword(emp.Password, u.PasswordHash, u.PasswordSalt))
                        {
                            return SetSession(u);
                        }
                    }

                    //}
                }
                throw new InvalidUserCredentialsException("Entered user credentials are not valid");
            }
            catch (InvalidUserCredentialsException ex)
            {
                Logger.Logger.WriteLog(Logger.Logtype.Error, ex.Message, 0, typeof(AuthController), ex);
                //ModelState.AddModelError("ErrorMessage", string.Format("{0}", ex.Message));
                ViewBag.Message = "Invalid Credential!";
                ViewBag.Success = false;
            }

            return View("Index");
        }
        [HttpPost]
        public ActionResult Signup(UserViewModel user)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    byte[] passwordHash, passwordSalt;
                    CreatePasswordHash(user.Password, out passwordHash, out passwordSalt);
                    var u = new UserModel()
                    {
                        FirstName = user.FirstName,
                        Email = user.Email,
                        Mobile = user.Mobile,
                        RoleId = 2,
                        PasswordHash = passwordHash,
                        PasswordSalt = passwordSalt,
                        IsActive = 1,
                        IsApproved = "1"
                    };
                    var result = CheckeIfUserExist(u.Email);
                    if (!result)
                    {

                        var response = _authRepository.Signup(u);
                        if (response > 0)
                        {
                            SendRegistrationMail(user);
                            ViewBag.Success = true;
                            ViewBag.Message = "Registration Successful! Please Login";
                        }
                        else
                        {
                            ViewBag.Success = false;
                            ViewBag.Message = "Registration Failed! Kindly Try Again";
                        }
                    }
                    else
                    {
                        ViewBag.Success = false;
                        ViewBag.Message = "User Already Exist! Kindly Login";
                    }
                }
            }
            catch (UserNotCreatedException ex)
            {
                Logger.Logger.WriteLog(Logger.Logtype.Error, ex.Message, 0, typeof(AuthController), ex);
                ModelState.AddModelError("ErrorMessage", string.Format("{0}", ex.Message));
            }

            return View("Index");
        }

        private bool CheckeIfUserExist(string email)
        {
            try
            {
                var resp = _authRepository.CheckIfUserExist(email);
                return resp;
            }
            catch (UserNotCreatedException ex)
            {
                Logger.Logger.WriteLog(Logger.Logtype.Error, ex.Message, 0, typeof(AuthController), ex);
            }
            catch (Exception ex)
            {
                Logger.Logger.WriteLog(Logger.Logtype.Error, ex.Message, 0, typeof(AuthController), ex);
            }
            return false;
        }

        private void SendRegistrationMail(UserViewModel u)
        {
            try
            {
                var basePath = string.Format("{0}://{1}", Request.Url.Scheme, Request.Url.Host);
                var link = basePath + "/Auth/Index";

                var subject = "LMS Registration Successful!";
                var body = "Dear " + u.FirstName + ",<br/><br/> Congratulations!!!, You has been registerd successfully for Leave Management System.<br/>"
                        + "<br/>Credentials:<br/>Email: " + u.Email + "<br/>Password: " + u.Password + "<br/><br/>"
                        + "<a href=" + link + "> Click here</a> To Login"
                        + "<br/><br/>Thank You <br/> Steeprise Team";

                var strFrom = ConfigurationManager.AppSettings["Fromemail"];
                var strFromPassword = ConfigurationManager.AppSettings["FromPassword"];
                var strCCEmail = ConfigurationManager.AppSettings["CCemail"];


                //var strFrom = _configuration["EmailCredential:Fromemail"];
                //var strFromPassword = _configuration["EmailCredential:FromPassword"];
                //var strCCEmail = _configuration["EmailNotification:CCemail"];

                var email = new MailMessage(strFrom, u.Email)
                {
                    Subject = subject,
                    Body = body,
                    IsBodyHtml = true
                };
                if (!string.IsNullOrEmpty(strCCEmail))
                {

                    string[] ccMuliIds = strCCEmail.Split(',');
                    foreach (string ccEMailId in ccMuliIds)
                    {
                        email.CC.Add(new MailAddress(ccEMailId)); //adding multiple CC Email Id
                    }
                }
                var fssmtp = new SmtpClient
                {
                    Host = "smtp.gmail.com",
                    Port = 587,
                    EnableSsl = true,
                    UseDefaultCredentials = true,
                    Credentials = new NetworkCredential(strFrom, strFromPassword)
                };
                fssmtp.Send(email);
            }
            catch (Exception ex)
            {
                Logger.Logger.WriteLog(Logger.Logtype.Error, ex.Message, 0, typeof(AuthController), ex);
                ModelState.AddModelError("ErrorMessage", string.Format("{0}", ex.Message));
            }
        }

        private void CreatePasswordHash(string password, out byte[] passwordHash, out byte[] passwordSalt)
        {
            using (var hmac = new System.Security.Cryptography.HMACSHA512())
            {
                passwordSalt = hmac.Key;
                passwordHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
            }
        }

        private bool VerifyPassword(string password, byte[] passwordHash, byte[] passwordSalt)
        {
            using (var hmac = new System.Security.Cryptography.HMACSHA512(passwordSalt))
            {
                var computedHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
                for (int i = 0; i < computedHash.Length; i++)
                {
                    if (computedHash[i] != passwordHash[i]) return false;
                }
            }
            return true;
        }
        private ActionResult GoAhead(string role, int userid)
        {
            string rUrl = Convert.ToString(TempData[Constants.SessionRedirectUrl]);
            if (!string.IsNullOrWhiteSpace(rUrl))
            {
                return new RedirectResult(rUrl);
            }

            if (!string.IsNullOrWhiteSpace(rUrl))
            {
                return new RedirectResult(rUrl);
            }

            if (role == Constants.AdminRole)
            {
                return RedirectToAction("Dashboard", "Admin");
            }
            else
            {
                return RedirectToAction("Dashboard", "Employee");
            }
        }
        public ActionResult Logout(string returnUrl = "")
        {

            Session.Clear();
            FormsAuthentication.SignOut();
            //HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            HttpContext.Session.Clear();
            return RedirectToAction("Index", "Auth", new { returnUrl = returnUrl });
        }
        public ActionResult SetSession(UserViewModel result)
        {
            try
            {
                //var identity = new ClaimsIdentity(new[] {
                //    new Claim(ClaimTypes.Email,result.Email),
                //    new Claim(ClaimTypes.Role,result.RoleName)
                //    }, Cookie);

                //var principal = new ClaimsPrincipal(identity);
                //HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

                if (!string.IsNullOrEmpty(result.PasswordExpirayDate) && DateTime.Now.Date <= Convert.ToDateTime(result.PasswordExpirayDate))
                {
                    //Handled if image url exist in db but not available physically
                    string picpath = "";//_hostingEnvironment.WebRootPath + result.ProfilePic;
                    if (!System.IO.File.Exists(picpath))
                    {
                        string fName = $@"\ProfilePic\" + "Avatar.jpg";
                        result.ProfilePic = fName;
                    }
                    //HttpContext.Session.Set<UserViewModel>(Constants.SessionKeyUserInfo, result);
                    Session[Constants.SessionKeyUserInfo] = result;
                    return GoAhead(result.RoleName, result.UserId);
                }
                else
                {
                    return View("CreateNewPassword");
                }
            }
            catch (Exception ex)
            {
                Logger.Logger.WriteLog(Logger.Logtype.Error, ex.Message, 0, typeof(AuthController), ex);
            }
            return GoAhead(result.RoleName, result.UserId);
        }
        [HttpGet]
        public ActionResult UnauthorizedUser()
        {
            var user = (UserViewModel)Session[Constants.SessionKeyUserInfo];//HttpContext.Session.Get<UserViewModel>(Constants.SessionKeyUserInfo);
            if (user?.RoleName == Constants.AdminRole)
            {
                ViewBag.Message = "This features is not related to Admin";
            }
            else if (user?.RoleName == Constants.EmployeeRole)
            {
                ViewBag.Message = "This features is not related to Employee";
            }
            return View();
        }
        [HttpGet]
        public ActionResult ForgotPassword()
        {
            return View();
        }
        [HttpPost]
        public ActionResult ForgotPassword(string email)
        {
            var user = (UserViewModel)Session[Constants.SessionKeyUserInfo];//HttpContext.Session.Get<UserViewModel>(Constants.SessionKeyUserInfo);//for loggging
            try
            {
                string emailID = string.Empty;
                var dt = _authRepository.GetUserData(email);
                if (dt != null && dt.Rows.Count > 0)
                {
                    emailID = Convert.ToString(dt.Rows[0]["Email"]);

                    /* Mail Send */
                    string emailEncr = EncryptDecrypt.Encrypt(emailID, "sblw-3hn8-sqoy19");
                    var basePath = string.Format("{0}://{1}", Request.Url.Scheme, Request.Url.Host);
                    var link = basePath + "/Auth/ResetPassword/?id=" + emailEncr;

                    //var strFrom = ConfigurationManager.AppSettings["EmailCredential:Fromemail"];
                    //var strFromPassword = ConfigurationManager.AppSettings["EmailCredential:FromPassword"];
                    //var strCCEmail = ConfigurationManager.AppSettings["EmailNotification:CCemail"];

                    var strFrom = ConfigurationManager.AppSettings["Fromemail"];
                    var strFromPassword = ConfigurationManager.AppSettings["FromPassword"];
                    var strCCEmail = ConfigurationManager.AppSettings["CCemail"];


                    var subject = "Reset Password";
                    var body = "Dear Candidate,<br/>You initiated a request to help with your account password. Click the link below to set a new password for Steeprise LMS portal" +
                        "<br/><br/><a href=" + link + ">Reset Password link</a><br><br>" + "Thank You<br>Steeprise Team";

                    var eModel = new MailMessage(strFrom, emailID)
                    {
                        Subject = subject,
                        Body = body,
                        IsBodyHtml = true,
                    };
                    if (!string.IsNullOrEmpty(strCCEmail))
                    {

                        string[] ccMuliIds = strCCEmail.Split(',');
                        foreach (string ccEMailId in ccMuliIds)
                        {
                            eModel.CC.Add(new MailAddress(ccEMailId)); //adding multiple CC Email Id
                        }
                    }
                    var fssmtp = new SmtpClient
                    {
                        Host = "smtp.gmail.com",
                        Port = 587,
                        EnableSsl = true,
                        UseDefaultCredentials = true,
                        Credentials = new NetworkCredential(strFrom, strFromPassword)
                    };
                    fssmtp.Send(eModel);
                    ViewData["SuccessMessage"] = "Password Reset link send to your Email";
                }
                else
                {

                    ViewData["SuccessMessage"] = "Email Not Found";
                }
            }
            catch (UserNotFoundException ex)
            {
                Logger.Logger.WriteLog(Logger.Logtype.Error, ex.Message, 0, typeof(AuthController), ex);
                ModelState.AddModelError("ErrorMessage", string.Format("{0}", ex.Message));
                ViewData["SuccessMessage"] = ex.Message;
            }
            catch (DataNotFoundException ex)
            {
                Logger.Logger.WriteLog(Logger.Logtype.Error, ex.Message, 0, typeof(AuthController), ex);
                ModelState.AddModelError("ErrorMessage", string.Format("{0}", ex.Message));
                ViewData["SuccessMessage"] = ex.Message;
            }
            catch (Exception ex)
            {
                Logger.Logger.WriteLog(Logger.Logtype.Error, ex.Message, 0, typeof(AuthController), ex);
                ModelState.AddModelError("ErrorMessage", string.Format("{0}", ex.Message));
                ViewData["SuccessMessage"] = ex.Message;//"Error Occured,Please contact at support@steeprise.com";
            }
            return View();
        }
        [HttpGet]
        public ActionResult ResetPassword(string id)
        {
            int mod4 = id.Length % 4;
            if (mod4 > 0)
            {
                id += new string('=', 4 - mod4);
            }
            string email = EncryptDecrypt.Decrypt(id.Replace(" ", "+"), "sblw-3hn8-sqoy19");
            UserViewModel userModel = new UserViewModel
            {
                Email = email
            };
            return View(userModel);
        }

        [HttpPost]
        public ActionResult ResetPassword(UserViewModel user)
        {
            try
            {
                var resp = ResetPassworData(user);
                if (resp)
                {
                    ViewData["SuccessMessage"] = "Password changed successfully! Click to login";
                }
                else
                {
                    ViewData["SuccessMessage"] = "Password change Faild, Kindly try again";
                }
            }
            catch (UserNotCreatedException ex)
            {
               // Logger.Logger.WriteLog(Logger.Logtype.Error, ex.Message, user.UserId, typeof(AuthController), ex);
                ModelState.AddModelError("ErrorMessage", string.Format("{0}", ex.Message));
            }
            ModelState.Clear();
            return View("ForgotPassword");
        }

        public bool ResetPassworData(UserViewModel u)
        {
            byte[] passwordHash, passwordSalt;
            CreatePasswordHash(u.Password, out passwordHash, out passwordSalt);
            var user = new UserModel
            {
                PasswordHash = passwordHash,
                PasswordSalt = passwordSalt,
                Email = u.Email
            };
            bool isRegister = _authRepository.ResetPassword(user);
            if (isRegister)
            {
                return true;
            }
            throw new UserNotCreatedException("Unable to create user, please contact your teck deck.");
        }
        [HttpGet]
        public ActionResult ChangePassword()
        {
            var user = (UserViewModel)Session[Constants.SessionKeyUserInfo];//HttpContext.Session.Get<UserViewModel>(Constants.SessionKeyUserInfo);
            if (user != null)
            {
                ViewBag.user = user?.Email;
                ViewData["SuccessMessage"] = null;
                ViewData["NotFoundMessage"] = null;
            }
            return View();
        }
        [HttpPost]
        public ActionResult CreateNewPassword(ResetPasswordViewModel model)
        {
            var user = (UserViewModel)Session[Constants.SessionKeyUserInfo]; //HttpContext.Session.Get<UserViewModel>(Constants.SessionKeyUserInfo);
            try
            {
                var dt = _authRepository.GetUserData(model.Email);
                if (dt != null && dt.Rows.Count > 0)
                {
                    var passwordHash = dt.Rows[0]["PasswordHash"] as byte[];
                    var passwordSalt = dt.Rows[0]["PasswordSalt"] as byte[];
                    if (!VerifyPassword(model.OldPassword, passwordHash, passwordSalt))
                    {
                        throw new InvalidUserCredentialsException("Inavlid Current Password");
                    }
                    CreatePasswordHash(model.Password, out passwordHash, out passwordSalt);
                    var u = new CreateNewPasswordModel
                    {
                        Email = user.Email,
                        PasswordHash = passwordHash,
                        PasswordSalt = passwordSalt
                        //Password = user.Password,
                        //OldPassword = user.OldPassword
                    };
                    bool isRegister = _authRepository.CreateNewPassword(u);
                    if (isRegister)
                    {
                        ViewData["SuccessMessage"] = "Password Successfully Changed! Click to login";
                        ViewData["NotFoundMessage"] = null;
                    }
                    else
                    {
                        ViewData["NotFoundMessage"] = "Invalid Credential! Kindly try again";
                        ViewData["SuccessMessage"] = null;
                    }
                }
            }
            catch (InvalidUserCredentialsException ex)
            {
                Logger.Logger.WriteLog(Logger.Logtype.Error, ex.Message, user.UserId, typeof(AuthController), ex);
                ViewData["NotFoundMessage"] = ex.Message;
            }
            catch (UserNotCreatedException ex)
            {
                Logger.Logger.WriteLog(Logger.Logtype.Error, ex.Message, user.UserId, typeof(AuthController), ex);
                ViewData["NotFoundMessage"] = ex.Message;
            }
            catch (Exception ex)
            {
                Logger.Logger.WriteLog(Logger.Logtype.Error, ex.Message, user.UserId, typeof(AuthController), ex);
                ViewData["NotFoundMessage"] = ex.Message;
            }
            return View("ChangePassword");
        }
        //public void WriteToFile(string Message)
        //{
        //    string path = AppDomain.CurrentDomain.BaseDirectory + "\\Logs";
        //    if (!Directory.Exists(path))
        //    {
        //        Directory.CreateDirectory(path);
        //    }
        //    string filepath = AppDomain.CurrentDomain.BaseDirectory + "\\Logs\\LMSLog_" + DateTime.Now.Date.ToShortDateString().Replace('/', '_') + ".txt";
        //    if (System.IO.File.Exists(filepath))
        //    {
        //        // Create a file to write to.   
        //        using (StreamWriter sw = System.IO.File.CreateText(filepath))
        //        {
        //            sw.WriteLine(Message);
        //        }
        //    }
        //    else
        //    {
        //        using (StreamWriter sw = System.IO.File.AppendText(filepath))
        //        {
        //            sw.WriteLine(Message);
        //        }
        //    }
        //}

    }
}