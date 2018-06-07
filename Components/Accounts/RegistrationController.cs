using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Users;

namespace Accounts
{
    [Authorize(Policy = "pal-tracker")]
    [Route("registration"), Produces("application/json")]
    public class RegisationController : Controller
    {
        private readonly IRegistrationService _service;

        public RegisationController(IRegistrationService service)
        {
            _service = service;
        }

        [HttpPost]
        public IActionResult Post([FromBody] UserInfo info)
        {
            var record = _service.CreateUserWithAccount(info.Name);
            var value = new UserInfo(record.Id, record.Name, "registration info");
            return Created($"registration/{value.Id}", value);
        }
    }
}