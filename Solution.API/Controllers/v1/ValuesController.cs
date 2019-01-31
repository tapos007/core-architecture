using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Solution.DLL;
using Solution.DLL.Identity.DBContext;
using Solution.DLL.Identity.Repository;
using Solution.DLL.Repository;

namespace Solution.API.Controllers.v1
{
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiController]
    public class ValuesController : ControllerBase
    {
        private readonly ITestRepository _testRepository;
        private readonly IUserRepository _userRepository;

        public ValuesController(ITestRepository testRepository,IUserRepository userRepository)
        {
            _testRepository = testRepository;
            _userRepository = userRepository;
        }

        // GET api/values
        [HttpGet]
        public async Task<ActionResult<string>> Get()
        {
            
           // await _testRepository.addDataElastic();
            return await _testRepository.getData("name");
        }

        // GET api/values/5
        [HttpGet("{id}")]
        public async  Task<ActionResult> Get(int id)
        {
            var person = new Person()
            {
                Name = "tapos",
                Email = "tapos@gmail.com"
            };
            return Ok(await _testRepository.insertObject(person));
        }

        // POST api/values
        [HttpPost]
        public void Post([FromBody] string value)
        {
        }

        // PUT api/values/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody] string value)
        {
        }

        // DELETE api/values/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}
