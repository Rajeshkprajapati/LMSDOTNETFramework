using LMS.Repository.DataModel.Shared;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace LMS.Repository.Interfaces.Auth
{
    public interface IAuthRepository
    {
        DataRow Login(string username,string password);
        int Signup(UserModel user);
        bool CheckIfUserExist(string email);
        DataTable GetUserData(string emailId);
        bool ResetPassword(UserModel user);
        bool CreateNewPassword(CreateNewPasswordModel user);
    }
}
