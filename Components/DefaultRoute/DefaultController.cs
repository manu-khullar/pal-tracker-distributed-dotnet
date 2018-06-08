using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace DefaultRoute
{
    [Route("")]
    [Authorize(Policy = "pal-tracker")]
    public class DefaultController : Controller
    {
        [HttpGet]
        public string Default() => "Noop!";
    }
}