
using LMS.Repository.DataModel.Admin;
using LMS.Repository.Helper;
using LMS.Repository.Interfaces.Admin;
using LMS.Utility.Exceptions;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Text;
using System.Configuration;
namespace LMS.Repository.Repositories.Admin
{
    public class ManageLeavesRepository : IManageLeavesRepository
    {
        private readonly string connectionString;
        //public ManageLeavesRepository(Configuration configurgation)
        //{
        //    // connectionString = configurgation["ConnectionStrings:LMSDB"];
        //    connectionString = ConfigurationManager.ConnectionStrings["LMS"].ConnectionString;
        //}

        public bool ApproveRejectLeave(ManageLeaveDataModel m)
        {
            using (var connection = new SqlConnection(connectionString))
            {
                try
                {
                    SqlParameter[] parameter = new SqlParameter[]
                    {
                        new SqlParameter("@leaveStatus",m.LeaveStatusId),
                        new SqlParameter("@EmpId",m.EmployeeId),
                        new SqlParameter("@Id",m.Id),
                    };
                    var resp =
                        SqlHelper.ExecuteNonQuery
                        (
                            connection,
                            CommandType.StoredProcedure,
                            "usp_ApproveRejectLeave",
                            parameter
                            );
                    if (resp > 0)
                    {
                        return true;
                    }
                    return false;
                }
                finally
                {
                    SqlHelper.CloseConnection(connection);
                }
            }
            throw new InvalidDataException("Inavlid Data");
        }

        public DataTable GetAllLeave()
        {
            using (var connection = new SqlConnection(connectionString))
            {
                try
                {                    
                    var leaves =
                        SqlHelper.ExecuteDataset
                        (
                            connection,
                            CommandType.StoredProcedure,
                            "usp_ListLeaveDetails"
                            );
                    if (leaves != null && leaves.Tables.Count > 0 && leaves.Tables[0].Rows.Count > 0)
                    {
                        return leaves.Tables[0];
                    }
                }
                finally
                {
                    SqlHelper.CloseConnection(connection);
                }
            }
            throw new DataNotFoundException("No Leave Found");
        }
    }
}
