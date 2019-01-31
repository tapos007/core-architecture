using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;

namespace Solution.API.Middlewares
{
    // You may need to install the Microsoft.AspNetCore.Http.Abstractions package into your project
    public class ExceptionMiddleware
    {
        private readonly RequestDelegate _next;

        public ExceptionMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext httpContext)
        {

            try
            {
                await _next(httpContext);
            }
            catch (Exception ex)
            {

                //await HandleExceptionAsync(httpContext, ex);
            }
        }


//        private static Task HandleExceptionAsync(HttpContext context, Exception exception)
//        {
//           
//
//            var result = JsonConvert.SerializeObject(new { error = exception.Message });
//            context.Response.ContentType = "application/json";
//            context.Response.StatusCode = (int)code;
//            return context.Response.WriteAsync(result);
//        }
    }
}
