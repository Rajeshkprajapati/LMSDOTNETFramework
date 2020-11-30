using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using System.Web.Mvc;
using LMS.Model.ViewModel.Employee;
using LMS.Model.ViewModel.Shared;
using LMS.Repository.DataModel.Employee;
using LMS.Repository.Interfaces.Employee;
using LMS.Repository.Repositories.Employee;
using LMS.Utility.Exceptions;
using LMS.Utility.ExtendedMethods;
using LMS.Utility.Helpers;

namespace LMS.WEBDOTNET.Controllers
{
    //[Route("[controller]")]
    //[UserAuthentication(Constants.EmployeeRole)]
    public class EmployeeController : Controller
    {
        private readonly ILeaveRepository _leaveRepository = new LeaveRepository();

        public ActionResult Index()
        {
            return View();
        }
        [Route("[action]")]
        public ActionResult Dashboard()
        {
            var user = (UserViewModel)Session[Constants.SessionKeyUserInfo];
            if (user == null || user.RoleName != Constants.EmployeeRole)
            {
                return RedirectToAction("Index", "Auth");
            }
            else
            {
                ViewBag.Name = user?.FirstName;
            }
            return View();
        }

        [HttpGet]
        [Route("[action]")]
        public PartialViewResult ApplyLeave()
        {
            ViewBag.LeaveType = GetLeaveTyep() ?? new List<LeaveTypeViewModel>();
            return PartialView("_ApplyLeave");
        }
        [HttpGet]
        [Route("[action]")]
        public PartialViewResult LeaveDetails()
        {
            var user = (UserViewModel)Session[Constants.SessionKeyUserInfo];
            if (user != null)
            {
                var list = GetAllLeaveData(user.UserId) ?? new List<LeaveDetailsViewModel>();
                return PartialView("_LeaveDetails", list);
            }
            return PartialView("_LeaveDetails", null);



        }

        private IEnumerable<LeaveTypeViewModel> GetLeaveTyep()
        {
            try
            {
                var dt = _leaveRepository.GetLeaveType();
                if (dt != null && dt.Rows.Count > 0)
                {
                    var list = new List<LeaveTypeViewModel>();
                    foreach (DataRow row in dt.Rows)
                    {
                        list.Add(new LeaveTypeViewModel()
                        {
                            LeaveTypeId = row["Id"] as int? ?? 0,
                            LeaveType = row["Type"] as string ?? "",
                        });
                    }
                    return list;
                }
            }
            catch (DataNotFoundException ex)
            {
                Logger.Logger.WriteLog(Logger.Logtype.Error, ex.Message, 0, typeof(EmployeeController), ex);
            }
            catch (Exception ex)
            {
                Logger.Logger.WriteLog(Logger.Logtype.Error, ex.Message, 0, typeof(EmployeeController), ex);                
            }
            return null;
        }

        public IEnumerable<LeaveDetailsViewModel> GetAllLeaveData(int id)
        {
            try
            {
                var dt = _leaveRepository.GetAllLeave(id);
                if (dt != null && dt.Rows.Count > 0)
                {
                    var list = new List<LeaveDetailsViewModel>();
                    foreach (DataRow row in dt.Rows)
                    {
                        list.Add(new LeaveDetailsViewModel()
                        {
                            Id = row["Id"] as int? ?? 0,
                            EmployeeId = row["Employeeid"] as string ?? "",
                            StartDate = Convert.IsDBNull(row["Startdate"]) ? "" : Convert.ToDateTime(row["Startdate"]).ToString("MM/dd/yyyy"),
                            EndDate = Convert.IsDBNull(row["Enddate"]) ? "" : Convert.ToDateTime(row["Enddate"]).ToString("MM/dd/yyyy"),
                            Remarks = row["Remarks"] as string ?? "",
                            LeaveStatusId = row["Statusid"] as string ?? "",
                            LeaveStatus = row["Status"] as string ?? "",
                            LeaveTypeId = row["Leavetypeid"] as string ?? "",
                            LeaveType = row["Type"] as string ?? "",
                            IsActive = row["IsActive"] as bool? ?? false,
                            CreatedDate = Convert.IsDBNull(row["Createddate"]) ? "" : Convert.ToDateTime(row["Createddate"]).ToString("MM/dd/yyyy"),
                        });
                    }
                    return list;
                }
            }
            catch (DataNotFoundException ex)
            {
                Logger.Logger.WriteLog(Logger.Logtype.Error, ex.Message, 0, typeof(EmployeeController), ex);
                ModelState.AddModelError("ErrorMessage", string.Format("{0}", ex.Message));
            }
            catch (Exception ex)
            {
                Logger.Logger.WriteLog(Logger.Logtype.Error, ex.Message, 0, typeof(EmployeeController), ex);
                ModelState.AddModelError("ErrorMessage", string.Format("{0}", ex.Message));
            }
            return null;
        }

        [HttpPost]
        [Route("[action]")]
        public ActionResult ApplyLeave(LeaveDetailsViewModel model)
        {
            try
            {
                if (!string.IsNullOrWhiteSpace(model.StartDate) && !string.IsNullOrWhiteSpace(model.EndDate) && !string.IsNullOrWhiteSpace(model.Remarks))
                {
                   bool saveLeave= SaveLeaveData(model);
                    if (saveLeave)
                    {
                        SendLeaveAppliedMail(model);
                    }
                   
                }
            }
            catch (InvalidDataException ex)
            {
                Logger.Logger.WriteLog(Logger.Logtype.Error, ex.Message, 0, typeof(EmployeeController), ex);
                ModelState.AddModelError("ErrorMessage", string.Format("{0}", ex.Message));
            }
            return View("Dashboard");
        }

        public bool SaveLeaveData(LeaveDetailsViewModel leave)
        {
            try
            {
                var user = (UserViewModel)Session[Constants.SessionKeyUserInfo];
                var model = new LeaveDetailsModel()
                {
                    StartDate = leave.StartDate,
                    EndDate = leave.EndDate,
                    Remarks = leave.Remarks,
                    EmployeeId = user?.UserId.ToString(),
                    LeaveTypeId = leave.LeaveTypeId,
                    IsActive = true
                };
                var resp = _leaveRepository.AddLeave(model);

                return (resp == true) ? true : false;

            }
            catch (InvalidDataException ex)
            {
                Logger.Logger.WriteLog(Logger.Logtype.Error, ex.Message, 0, typeof(EmployeeController), ex);
                ModelState.AddModelError("ErrorMessage", string.Format("{0}", ex.Message));
            }
            catch (Exception ex)
            {
                Logger.Logger.WriteLog(Logger.Logtype.Error, ex.Message, 0, typeof(EmployeeController), ex);
                ModelState.AddModelError("ErrorMessage", string.Format("{0}", ex.Message));
            }
            return false;

        }

        private void SendLeaveAppliedMail(LeaveDetailsViewModel model)
        {
            try
            {
                var user = (UserViewModel)Session[Constants.SessionKeyUserInfo];
                var subject = "Leave has been applied successfully";
                var body = "Dear " + user?.Email + ",<br/><br/> Your Leave has been applied for duration " + model.StartDate + " To " + model.EndDate + " successfully."
                        + "<br/><br/> Thank You <br/> Steeprise Team";
                var strFrom = ConfigurationManager.AppSettings["EmailCredential:Fromemail"];
                var strFromPassword = ConfigurationManager.AppSettings["EmailCredential:FromPassword"];
                var strCCEmail = ConfigurationManager.AppSettings["EmailNotification:CCemail"];

                var email = new MailMessage(strFrom, user.Email)
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
                ModelState.AddModelError("ErrorMessage", string.Format("{0}", ex.Message));
            }
        }


    }
}