using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using TestResearchProject.Models;

namespace TestResearchProject.Controllers
{
    public class LoginController : Controller
    {
        private readonly IConfiguration _configuration;
        public LoginController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public string GetConnectionString()
        {
            return _configuration.GetConnectionString("DefaultConnection");
        }

        public ViewResult Signup()
        {
            return View();
        }

        [HttpPost]
        public ViewResult Signup(Signup signupData)
        {
            try
            {
                if (!string.IsNullOrWhiteSpace(signupData.email))
                {

                }
            }
            catch(Exception ex)
            {

            }
            return View();
        }

        public IActionResult Login()
        {
            
            return View();
        }

        [HttpPost]
        public IActionResult Login(LoginModel logindata)
        {

            return View();
        }
    }
}
