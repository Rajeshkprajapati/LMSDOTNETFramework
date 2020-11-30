using System;
using System.Collections.Generic;
using System.Text;

namespace LMS.Model.ViewModel.Shared
{
    [Serializable]
    public class UserViewModel
    {
        public int UserId { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string RoleName { get; set; }
        public string ProfilePic { get; set; }
        public string IsApproved { get; set; }
        public string PasswordExpirayDate { get; set; }
        public int RoleId { get; set; }
        public string Mobile { get; set; }
        public byte[] PasswordHash { get; set; }
        public byte[] PasswordSalt { get; set; }
    }
}
