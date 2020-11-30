using System;
using System.Collections.Generic;
using System.Text;

namespace LMS.Model.ViewModel.Admin
{
    public class ManageLeaveViewModel
    {
        public int Id { get; set; }
        public int EmployeeId { get; set; }
        public string EmployeeName { get; set; }
        public string EmployeeEmail { get; set; }
        public string LeaveType { get; set; }
        public string LeaveStatus { get; set; }
        public string LeaveTypeId { get; set; }
        public string LeaveStatusId { get; set; }
        public string StartDate { get; set; }
        public string CreatedDate { get; set; }
        public string EndDate { get; set; }
        public string Remarks { get; set; }
        public bool IsActive { get; set; }
    }
}
