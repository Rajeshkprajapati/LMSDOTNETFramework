using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using System.Web.Mvc;
using LMS.Model.ViewModel.Admin;
using LMS.Model.ViewModel.Shared;
using LMS.Repository.DataModel.Admin;
using LMS.Repository.DataModel.Shared;
using LMS.Repository.Interfaces.Admin;
using LMS.Repository.Repositories.Admin;
using LMS.Utility.Exceptions;
using LMS.Utility.ExtendedMethods;
using LMS.Utility.Helpers;
//using LMS.WEBDOTNET.Filters;
//using Microsoft.AspNetCore.Hosting;
//using Microsoft.AspNetCore.Mvc;
//using Microsoft.Extensions.Configuration;

namespace LMS.WEBDOTNET.Controllers
{
   // [Route("[controller]")]
    //[UserAuthentication(Constants.AdminRole)]
    public class AdminController : Controller
    {
       // private readonly IHostingEnvironment _hostingEnvironment;
      // private readonly IConfiguration _configuration;
        private readonly IManageLeavesRepository _manageLeavesRepository = new ManageLeavesRepository();

        public AdminController(IManageLeavesRepository manageLeavesRepository)
        {
            //_hostingEnvironment = hostingEnvironment;
           // _configuration = configuration;
            _manageLeavesRepository = manageLeavesRepository;
        }
        
        public ActionResult Index()
        {
            return View();
        }
        [HttpGet]
        [Route("[action]")]
        public ActionResult Dashboard()
        {
            var user = (UserViewModel)Session[Constants.SessionKeyUserInfo];
            if (user == null || user.RoleName != Constants.AdminRole)
            {
                return RedirectToAction("Index","Auth");
            }
            else
            {
                ViewBag.Name = user?.FirstName;
            }
            return View();
        }
        [HttpGet]
        [Route("[action]")]
        public PartialViewResult ManageLeaves()
        {
            var list = GetAllLeaves() ?? new List<ManageLeaveViewModel>();
            return PartialView("_ManageLeaves", list);
        }

        private IEnumerable<ManageLeaveViewModel> GetAllLeaves()
        {
            try
            {
                var dt = _manageLeavesRepository.GetAllLeave();
                if (dt != null && dt.Rows.Count > 0)
                {
                    var list = new List<ManageLeaveViewModel>();
                    foreach (DataRow row in dt.Rows)
                    {
                        list.Add(new ManageLeaveViewModel()
                        {
                            Id = row["Id"] as int? ?? 0,
                            EmployeeName = row["FirstName"] as string ?? "",
                            EmployeeId = row["Employeeid"] as int? ?? 0,
                            EmployeeEmail = row["Email"] as string ?? "",
                            StartDate = Convert.IsDBNull(row["Startdate"]) ? "" : Convert.ToDateTime(row["Startdate"]).ToString("MM/dd/yyyy"),
                            EndDate = Convert.IsDBNull(row["Enddate"]) ? "" : Convert.ToDateTime(row["Enddate"]).ToString("MM/dd/yyyy"),
                            Remarks = row["Remarks"] as string ?? "",
                            LeaveStatusId = row["Statusid"] as string ?? "",
                            LeaveTypeId = row["Leavetypeid"] as string ?? "",
                            IsActive = row["Isactive"] as bool? ?? false,
                            CreatedDate = Convert.IsDBNull(row["Createddate"]) ? "" : Convert.ToDateTime(row["Createddate"]).ToString("MM/dd/yyyy"),
                            LeaveStatus = row["Status"] as string ?? "",
                            LeaveType = row["Type"] as string ?? "",
                        });
                    }
                    return list;
                }
            }
            catch (DataNotFoundException ex)
            {
                Logger.Logger.WriteLog(Logger.Logtype.Error, ex.Message, 0, typeof(AdminController), ex);
                ModelState.AddModelError("ErrorMessage", string.Format("{0}", ex.Message));
            }
            catch (Exception ex)
            {
                Logger.Logger.WriteLog(Logger.Logtype.Error, ex.Message, 0, typeof(AdminController), ex);
                ModelState.AddModelError("ErrorMessage", string.Format("{0}", ex.Message));
            }
            return null;
        }
        [HttpPost]
        [Route("[action]")]
        public JsonResult ApproveRejectLeave(ManageLeaveViewModel model)
        {
            try
            {
                var m = new ManageLeaveDataModel();
                if (model != null)
                {
                    m.EmployeeId = model.EmployeeId;
                    m.LeaveStatusId = model.LeaveStatusId;
                    m.Id = model.Id;
                }
                var result = _manageLeavesRepository.ApproveRejectLeave(m);
                if (result && m.LeaveStatusId == "2")
                {
                    SendLeaveApprovalMail(model);
                }
                else if(result)
                {
                    SendLeaveRejectionMail(model);
                }
                return Json(new { result });
            }
            catch (Exception ex)
            {
                Logger.Logger.WriteLog(Logger.Logtype.Error, ex.Message, 0, typeof(AdminController), ex);
                ModelState.AddModelError("ErrorMessage", string.Format("{0}", ex.Message));
            }
            return Json(new { res = false });
        }

        private void SendLeaveRejectionMail(ManageLeaveViewModel model)
        {
            try
            {
                var subject = "Leave has been rejected";
                var body = "Dear "+model.EmployeeEmail+",<br/><br/> Your Leave has been rejected for duration "+model.StartDate+" to "+model.EndDate
                        + "<br/>Please contact your reporting manager for any queries.<br/> <br/>Thank You <br/> Steeprise Team";
                var strFrom = ConfigurationManager.AppSettings["EmailCredential:Fromemail"];
                var strFromPassword = ConfigurationManager.AppSettings["EmailCredential:FromPassword"];
                var strCCEmail = ConfigurationManager.AppSettings["EmailNotification:CCemail"];

                var email = new MailMessage(strFrom, model.EmployeeEmail)
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
            catch(Exception ex)
            {
                Logger.Logger.WriteLog(Logger.Logtype.Error, ex.Message, 0, typeof(AdminController), ex);
                ModelState.AddModelError("ErrorMessage", string.Format("{0}", ex.Message));
            }
        }

        private void SendLeaveApprovalMail(ManageLeaveViewModel model)
        {
            try
            {
                var subject = "Leave has been approved";
                var body = "Dear " + model.EmployeeEmail + ",<br/><br/> Your Leave has been approved for duration " + model.StartDate + " To " + model.EndDate
                        + "<br/><br/> Thank You <br/> Steeprise Team";
                var strFrom = ConfigurationManager.AppSettings["EmailCredential:Fromemail"];
                var strFromPassword = ConfigurationManager.AppSettings["EmailCredential:FromPassword"];
                var strCCEmail = ConfigurationManager.AppSettings["EmailNotification:CCemail"];

                var email = new MailMessage(strFrom, model.EmployeeEmail)
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
                Logger.Logger.WriteLog(Logger.Logtype.Error, ex.Message, 0, typeof(AdminController), ex);
                ModelState.AddModelError("ErrorMessage", string.Format("{0}", ex.Message));
            }
        }
        //[HttpGet]
        //[Route("[action]")]
        //public ActionResult ChangePassword()
        //{
        //    return View();
        //}
        //[HttpPost]
        //[Route("[action]")]
        //public ActionResult CreateNewPassword(ResetPasswordViewModel model)
        //{
        //    var status = false;
        //    try
        //    {
        //        var user = (UserViewModel)Session[Constants.SessionKeyUserInfo];
        //        model.Email = user.Email;
        //        status = CreateNewPasswordForAdmin(model);
        //    }
        //    catch (UserNotCreatedException ex)
        //    {
        //        status = false;
        //    }
        //    ModelState.Clear();
        //    return Json(status);
        //}
        //public bool CreateNewPasswordForAdmin(ResetPasswordViewModel user)
        //{
        //    byte[] passwordHash, passwordSalt;            
        //    CreatePasswordHash(user.Password, out passwordHash, out passwordSalt);
        //    var u = new CreateNewPasswordModel
        //    {
        //        Email = user.Email,
        //        PasswordHash = passwordHash,
        //        PasswordSalt = passwordSalt
        //        //Password = user.Password,
        //        //OldPassword = user.OldPassword
        //    };
        //    bool isRegister = _manageLeavesRepository.ApproveRejectLeave(u);
        //    if (isRegister)
        //    {
        //        return true;
        //    }
        //    throw new UserNotCreatedException("Unable to change password, Please insert vailid email and password");
        //}
        //private void CreatePasswordHash(string password, out byte[] passwordHash, out byte[] passwordSalt)
        //{
        //    using (var hmac = new System.Security.Cryptography.HMACSHA512())
        //    {
        //        passwordSalt = hmac.Key;
        //        passwordHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
        //    }
        //}
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