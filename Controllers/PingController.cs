using Microsoft.AspNetCore.Mvc;

namespace SSCMS.Form.Controllers
{
    [Route("api/form/ping")]
    public class PingController : ControllerBase
    {
        private const string Route = "";

        [HttpGet, Route(Route)]
        public string Get()
        {
            return "pong";
        }
    }
}