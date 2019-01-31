using System;
using System.Collections.Generic;
using System.Text;

namespace UtilityProject
{
    public class CustomError
    {
        public string Error { get; }
        public int Code { get; }

        public CustomError(string message,int code)
        {
            Error = message;
            Code = code;
        }
    }
}
