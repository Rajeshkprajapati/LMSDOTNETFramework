using LMS.Repository.DataModel.Employee;
using LMS.Repository.Helper;
using LMS.Repository.Interfaces.Employee;
using LMS.Utility.Exceptions;
//using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Text;
using System.Configuration;
namespace LMS.Repository.Repositories.Employee
{
    public class LeaveRepository : ILeaveRepository
    {
        private readonly string connectionString;
        public LeaveRepository(Configuration configuration)
        {
            //connectionString = configuration["ConnectionStrings:LMSDB"];
            connectionString = ConfigurationManager.ConnectionStrings["LMS"].ConnectionString;
        }

        public bool AddLeave(LeaveDetailsModel model)
        {
            using (var connection = new SqlConnection(connectionString))
            {
                try
                {
                    SqlParameter[] parameters = new SqlParameter[] {
                        new SqlParameter("@EmployeeId",model.EmployeeId),
                        new SqlParameter("@StartDate",model.StartDate),
                        new SqlParameter("@EndDate",model.EndDate),
                        new SqlParameter("@Remarks",model.Remarks),
                        new SqlParameter("@StatusID",1),//Always will be pending at start
                        new SqlParameter("@LeaveTypeID",model.LeaveTypeId),
                        new SqlParameter("@IsActive",model.IsActive),
                        new SqlParameter("@CreatedBy",model.EmployeeId)
                    };
                    var user =
                        SqlHelper.ExecuteNonQuery
                        (
                            connection,
                            CommandType.StoredProcedure,
                            "usp_SaveLeaveDetails",
                            parameters
                            );
                    if (user > 0)
                    {
                        return true;
                    }
                }
                finally
                {
                    SqlHelper.CloseConnection(connection);
                }
            }
            throw new InvalidDataException("Invalid Data to Add");
        }

        public DataTable GetAllLeave(int id)
        {
            using (var connection = new SqlConnection(connectionString))
            {
                try
                {
                    SqlParameter[] parameters = new SqlParameter[] {
                        new SqlParameter("@Id",id),
                    };
                    var leave =
                        SqlHelper.ExecuteDataset
                        (
                            connection,
                            CommandType.StoredProcedure,
                            "usp_ListEmployeeLeaveDetails",
                            parameters
                            );
                    if (leave != null && leave.Tables.Count > 0 && leave.Tables[0].Rows.Count > 0)
                    {
                        return leave.Tables[0];
                    }
                }
                finally
                {
                    SqlHelper.CloseConnection(connection);
                }
            }
            throw new DataNotFoundException("No Leave Found");
        }

        public DataTable GetLeaveType()
        {
            using (var connection = new SqlConnection(connectionString))
            {
                try
                {
                    var leave =
                        SqlHelper.ExecuteDataset
                        (
                            connection,
                            CommandType.StoredProcedure,
                            "usp_GetLeaveType"
                            );
                    if (leave != null && leave.Tables.Count > 0 && leave.Tables[0].Rows.Count > 0)
                    {
                        return leave.Tables[0];
                    }
                }
                finally
                {
                    SqlHelper.CloseConnection(connection);
                }
            }
            throw new DataNotFoundException("No LeaveType Found");
        }
    }
}
