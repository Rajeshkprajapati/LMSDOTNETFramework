﻿using System;
using System.Collections.Generic;
using System.Text;

namespace LMS.Model.ViewModel.Shared
{
    public class ResetPasswordViewModel
    {
        public string Email { get; set; }
        public string OldPassword { get; set; }
        public string Password { get; set; }
    }
}
