using System;
using System.Collections.Generic;
using System.Text;

namespace LMS.Utility.Exceptions
{
    public class DataNotFoundException : Exception
    {
        public DataNotFoundException(string message) : base(message)
        {

        }

        public DataNotFoundException(string message, Exception exception) : base(message, exception)
        {

        }
    }
}
