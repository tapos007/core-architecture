using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using Nest;

namespace Solution.DLL.Identity.RequestModel
{
   public class CustomerRegistrationRequest
    {
        [Required(ErrorMessage = "User Name required")]
        public string UserName { get; set; }

        [Required(ErrorMessage = "Email required")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Password required")]
        public string Password { get; set; }

        [Required(ErrorMessage = "Phone Number required")]
        public string PhoneNumber { get; set; }
    }
}
