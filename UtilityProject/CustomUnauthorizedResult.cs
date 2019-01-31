using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace UtilityProject
{
    public class CustomUnauthorizedResult : JsonResult
    {
        public CustomUnauthorizedResult(string message,int code)
            : base(new CustomError(message,code))
        {
            StatusCode = StatusCodes.Status401Unauthorized;
        }
    }
}
