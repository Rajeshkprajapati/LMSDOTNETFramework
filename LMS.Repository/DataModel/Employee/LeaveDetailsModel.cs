using System;
using System.Collections.Generic;
using System.Text;

namespace LMS.Repository.DataModel.Employee
{
    public class LeaveDetailsModel
    {
        public long Id { get; set; }
        public string EmployeeId { get; set; }
        public string StartDate { get; set; }
        public string EndDate { get; set; }
        public string LeaveStatusId { get; set; }
        public string LeaveStatus { get; set; }
        public string LeaveTypeId { get; set; }
        public string LeaveType { get; set; }
        public bool Status { get; set; }
        public bool IsActive { get; set; }
        public string CreatedDate { get; set; }
        public string Remarks { get; set; }
    }
}
