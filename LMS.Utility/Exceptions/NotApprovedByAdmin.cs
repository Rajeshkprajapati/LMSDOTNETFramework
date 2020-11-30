using System;
using System.Collections.Generic;
using System.Text;

namespace LMS.Utility.Exceptions
{
    public class NotApprovedByAdmin:Exception
    {
        public NotApprovedByAdmin(string message) : base(message)
        {

        }
        public NotApprovedByAdmin(string message, Exception exception) : base(message, exception)
        {

        }
    }
}
