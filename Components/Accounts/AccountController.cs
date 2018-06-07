using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;


   
namespace Accounts
{
     [Authorize(Policy = "pal-tracker")]
    [Route("accounts"), Produces("application/json")]
    public class AccountController : Controller
    {
        private readonly IAccountDataGateway _gateway;

        public AccountController(IAccountDataGateway gateway)
        {
            _gateway = gateway;
        }

        [HttpGet]
        public IActionResult Get([FromQuery] int ownerId)
        {
            var list = _gateway.FindBy(ownerId).Select(record => new AccountInfo(record.Id, record.OwnerId, record.Name,
                    "account info"))
                .ToList();

            return Ok(list);
        }
    }
}