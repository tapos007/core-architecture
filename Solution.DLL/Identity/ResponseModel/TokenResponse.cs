using System;
using System.Collections.Generic;
using System.Text;

namespace Solution.DLL.Identity.ResponseModel
{
    public class TokenResponse
    {
        public string AccessToken { get; set; }

        public string RefreshToken { get; set; }

        public string UserName { get; set; }

        public string Email { get; set; }

        public DateTime TokenExpiration { get; set; }


    }
}
