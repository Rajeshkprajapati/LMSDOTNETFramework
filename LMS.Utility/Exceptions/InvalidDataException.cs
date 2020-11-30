using System;
using System.Collections.Generic;
using System.Text;

namespace LMS.Utility.Exceptions
{
    public class InvalidDataException : Exception
    {
        public InvalidDataException(string message) : base(message)
        {

        }

        public InvalidDataException(string message, Exception exception) : base(message, exception)
        {

        }
    }
}
