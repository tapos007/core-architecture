using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;


namespace UtilityProject
{
   public static class APIResponse
    {

        public static object ErrorResponse(string msg, int code = 400)
        {
            return  new { error = msg,Code=code };
        }
    }
}
