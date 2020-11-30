using System;
using System.Collections.Generic;
using System.Text;

namespace LMS.Repository.DataModel.Shared
{
    public class UserModel
    {
        public int UserId { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string RoleName { get; set; }
        public string Mobile { get; set; }        
        public int RoleId { get; set; }
        public int IsActive { get; set; }
        public string IsApproved { get; set; }
        public byte[] PasswordHash { get; set; }
        public byte[] PasswordSalt { get; set; }
        public string ProfilePic { get; set; }
    }
    public class RolesModel
    {
        public int RoleId { get; set; }
        public string RoleName { get; set; }
        public bool IsEmp { get; set; }
    }
    public class CreateNewPasswordModel
    {

        public string Email { get; set; }
        public string OldPassword { get; set; }
        public string Password { get; set; }
        public byte[] PasswordHash { get; set; }
        public byte[] PasswordSalt { get; set; }
    }
}
