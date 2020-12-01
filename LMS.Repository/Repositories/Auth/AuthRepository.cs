using LMS.Repository.DataModel.Shared;
using LMS.Repository.Helper;
using LMS.Repository.Interfaces.Auth;
using LMS.Utility.Exceptions;
using System;
using System.Configuration;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Text;

namespace LMS.Repository.Repositories.Auth
{
    public class AuthRepository : IAuthRepository
    {
        //private readonly string connectionString = "Data Source=LAPTOP-5DCMGJD9; Initial Catalog=LMS; User ID=sa; Password=steeprise123";//WebConfigurationManager.AppSettings["connString"];;
        private readonly string connectionString = ConfigurationManager.AppSettings["connString"];

        //public AuthRepository( Configuration configuration)
        //{
        //    //connectionString = configuration["ConnectionStrings:LMSDB"];
        //    connectionString = ConfigurationManager.ConnectionStrings["LMS"].ConnectionString;
        //}
        
        public bool CheckIfUserExist(string email)
        {
            using (var connection = new SqlConnection(connectionString))
            {
                try
                {
                    SqlParameter[] parameters = new SqlParameter[] {
                        new SqlParameter("@Email",email),
                    };
                    var result =
                        SqlHelper.ExecuteDataset
                        (
                            connection,
                            CommandType.StoredProcedure,
                            "usp_CheckIfUserExist",
                            parameters
                            );
                    if (result != null && result.Tables.Count > 0 && result.Tables[0].Rows.Count > 0)
                    {
                        return true;
                    }
                }
                finally
                {
                    SqlHelper.CloseConnection(connection);
                }
            }
            throw new UserNotCreatedException("User is not Found.");
        }

        public DataTable GetUserData(string emailId)
        {
            var status = CheckIfUserExist(emailId);
            if (status)
            {
                using (var connection = new SqlConnection(connectionString))
                {
                    try
                    {
                        SqlParameter[] parameters = new SqlParameter[] {
                        new SqlParameter("@Email",emailId)
                    };
                        var user =
                            SqlHelper.ExecuteReader
                            (
                                connection,
                                CommandType.StoredProcedure,
                                "usp_ForgetPassword",
                                parameters
                                );
                        if (null != user && user.HasRows)
                        {
                            var dt = new DataTable();
                            dt.Load(user);
                            //return Convert.ToString(dt.Rows[0]["Email"]);
                            return dt;

                        }
                    }
                    finally
                    {
                        SqlHelper.CloseConnection(connection);
                    }
                }
                throw new InvalidUserCredentialsException("Email Id is not valid");
            }
            else
            {
                throw new DataNotFoundException("data not found");
            }
        }
        public bool ResetPassword(UserModel user)
        {
            using (var connection = new SqlConnection(connectionString))
            {
                try
                {
                    SqlParameter[] parameters = new SqlParameter[] {
                        new SqlParameter("@Email",user.Email),
                        new SqlParameter("@passwordSalt",user.PasswordSalt),
                        new SqlParameter("@passwordHash",user.PasswordHash),
                        //new SqlParameter("@Password",user.Password)
                    };
                    var result =
                        SqlHelper.ExecuteNonQuery
                        (
                            connection,
                            CommandType.StoredProcedure,
                            "usp_UpdatePassword",
                            parameters
                            );
                    if (result > 0)
                    {
                        return true;
                    }

                }
                finally
                {
                    SqlHelper.CloseConnection(connection);
                }
            }
            throw new UserNotCreatedException("Unable to register, please contact your teck deck with your details.");
        }

        public DataRow Login(string userName, string password)
        {
            using (var connection = new SqlConnection(connectionString))
            {
                try
                {

                    SqlParameter[] parameters = new SqlParameter[] {
                        new SqlParameter("@Email",userName),
                        //new SqlParameter("@Password",password)
                    };
                    var user =
                        SqlHelper.ExecuteDataset
                        (
                            connection,
                            CommandType.StoredProcedure,
                            "usp_UserLogin",
                            parameters
                            );
                    if (null != user && user.Tables.Count > 0 && user.Tables[0].Rows.Count > 0)
                    {
                        return user.Tables[0].Rows[0];
                    }
                }
                finally
                {
                    SqlHelper.CloseConnection(connection);
                }
            }
            throw new InvalidUserCredentialsException("Entered user credentials are not valid");
        }
        public int Signup(UserModel user)
        {
            using (var connection = new SqlConnection(connectionString))
            {
                try
                {
                    var outParam = new SqlParameter
                    {
                        ParameterName = "@OutUserId",
                        Direction = ParameterDirection.Output,
                        SqlDbType = SqlDbType.Int
                    };
                    SqlParameter[] parameters = new SqlParameter[] {
                        new SqlParameter("@Email",user.Email),
                        new SqlParameter("@FName",user.FirstName),
                        new SqlParameter("@LName",user.LastName),
                        new SqlParameter("@MobileNo",user.Mobile),
                        //new SqlParameter("@Password",user.Password),
                        new SqlParameter("@RoleId",user.RoleId),
                        new SqlParameter("@passwordSalt",user.PasswordSalt),
                        new SqlParameter("@passwordHash",user.PasswordHash),
                        new SqlParameter("@IsActive",user.IsActive),                        
                        //new SqlParameter("@CreatedBy",user.CreatedBy),
                         new SqlParameter("@IsApproved",user.IsApproved),
                        outParam
                    };
                    var result =
                        SqlHelper.ExecuteNonQuery
                        (
                            connection,
                            CommandType.StoredProcedure,
                            "usp_RegisterEmployee",
                            parameters
                            );
                    if (result > 0)
                    {
                        return Convert.ToInt32(outParam.Value);
                    }
                }
                finally
                {
                    SqlHelper.CloseConnection(connection);
                }
            }
          throw new UserNotCreatedException("Unable to register, please contact your teck deck with your details.");
        }

        public bool CreateNewPassword(CreateNewPasswordModel user)
        {
            using (var connection = new SqlConnection(connectionString))
            {
                try
                {
                    SqlParameter[] parameters = new SqlParameter[] {
                        new SqlParameter("@Email",user.Email),
                        new SqlParameter("@passwordSalt",user.PasswordSalt),
                        new SqlParameter("@passwordHash",user.PasswordHash),
                        //new SqlParameter("@Password",user.Password),
                        //new SqlParameter("@OldPassword",user.OldPassword)

                    };
                    var result =
                        SqlHelper.ExecuteNonQuery
                        (
                            connection,
                            CommandType.StoredProcedure,
                            "usp_CreateNewPassword",
                            parameters
                            );
                    if (result > 0)
                    {
                        return true;
                    }
                }
                finally
                {
                    SqlHelper.CloseConnection(connection);
                }
            }
           throw new UserNotCreatedException("Unable to change password, either email or password is not valid");
        }
    }
}
