using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace User.Management.API.Controllers
{
    [Authorize(Roles ="Admin")]
    [Route("api/[controller]")]
    [ApiController]
    public class AdminController : Controller
    {
        /// <summary>
        /// Test
        /// </summary>
        /// <returns></returns>
        [HttpGet("employees")]
        public IEnumerable<string> Get()
        {
            return new List<string>{"properties","method","field" };
        }
    }
}
