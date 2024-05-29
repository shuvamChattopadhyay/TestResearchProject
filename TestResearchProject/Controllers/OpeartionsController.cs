using Microsoft.AspNetCore.Mvc;
using TestResearchProject.Helpers;

namespace TestResearchProject.Controllers
{
    /// <summary>
    /// This is the new way to initialize the constructor. This is called Primary Constructor.
    /// This feature is delivered in C# 12 and .NET version 8.0
    /// Can be new way for dependancy injection
    /// </summary>
    /// <param name="configuration"></param>
    /// <param name="webHostEnvironment"></param>
    /// <param name="helper"></param>
    public class OpeartionsController (IConfiguration configuration, IWebHostEnvironment webHostEnvironment, HelperMethods helper) : Controller
    {
        private readonly IConfiguration _configuration = configuration;
        private readonly dynamic _helper = helper;
        private readonly IWebHostEnvironment _webHostEnvironment = webHostEnvironment;
        public string GetConnectionString()
        {   
            return _configuration.GetConnectionString("DefaultConnection");
        }
    }
}
