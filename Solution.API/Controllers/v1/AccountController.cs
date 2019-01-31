using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Solution.DLL.Identity.Repository;
using Solution.DLL.Identity.RequestModel;

namespace Solution.API.Controllers.v1
{
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly IUserRepository _userRepository;

        public AccountController(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        [HttpPost("Registration")]
        public async Task<ActionResult> Registration(CustomerRegistrationRequest registration)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var result = await _userRepository.Create(registration);

            return Ok("account created");
        }

        [HttpPost("Login")]
        public async Task<ActionResult> Login(CustomerRegistrationRequest registration)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var result = await _userRepository.Login(registration);

            return Ok(result);
        }


        [Authorize]
        [HttpGet("check")]
        public  ActionResult Check()
        {
            

            return Ok("you are authorized");
        }


    }
}