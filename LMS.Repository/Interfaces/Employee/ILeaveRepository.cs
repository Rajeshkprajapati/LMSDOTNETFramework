using LMS.Repository.DataModel.Employee;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace LMS.Repository.Interfaces.Employee
{
    public interface ILeaveRepository
    {
        bool AddLeave(LeaveDetailsModel model);
        DataTable GetAllLeave(int id);
        DataTable GetLeaveType();
    }
}
