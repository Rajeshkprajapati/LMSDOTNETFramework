using LMS.Repository.DataModel.Admin;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace LMS.Repository.Interfaces.Admin
{
    public interface IManageLeavesRepository
    {
        DataTable GetAllLeave();
        bool ApproveRejectLeave(ManageLeaveDataModel m);
    }
}
