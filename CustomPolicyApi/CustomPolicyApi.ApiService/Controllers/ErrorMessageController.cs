using Microsoft.AspNetCore.Mvc;

namespace CustomPolicyApi.ApiService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ErrorMessageController : ControllerBase
    {
        [HttpGet("user-already-exists")]
        public IActionResult UserAlreadyExists()
        {
            return BadRequest(new
            {
                version = "1.0.0",
                status = 400,
                code = "UserAlreadyExists",
                message = "A user with this email address already exists."
            });
        }
        
        [HttpGet("user-already-exists-2")]
        public IActionResult UserAlreadyExists2()
        {
            return BadRequest(new
            {
                message = "A user with this email address already exists."
            });
        }

        [HttpGet("invalid-password")]
        public IActionResult InvalidPassword()
        {
            return BadRequest(new
            {
                version = "1.0.0",
                status = 400,
                code = "InvalidPassword",
                message = "The password provided does not meet complexity requirements."
            });
        }
        
        [HttpGet("invalid-password-2")]
        public IActionResult InvalidPassword2()
        {
            return BadRequest(new
            {
                message = "The password provided does not meet complexity requirements."
            });
        }

        [HttpGet("server-error")]
        public IActionResult ServerError()
        {
            return StatusCode(500, new
            {
                version = "1.0.0",
                status = 500,
                code = "ServerError",
                message = "An unexpected server error occurred."
            });
        }
    }
}