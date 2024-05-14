using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System.Net;
using TestResearchProject.Models;
using TestResearchProject.Helpers;
using System.Text;
using System.Security.Cryptography;
using System.Security.Cryptography.Xml;
using Microsoft.Build.Execution;



namespace TestResearchProject.Controllers
{
    public class LoginController : Controller
    {
        private readonly IConfiguration _configuration;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly dynamic _helper;
        public LoginController(IConfiguration configuration, IWebHostEnvironment webHostEnvironment, HelperMethods helper)
        {
            _configuration = configuration;
            _webHostEnvironment = webHostEnvironment;
            _helper = new HelperMethods(_webHostEnvironment);
        }

        public string GetConnectionString()
        {
            return _configuration.GetConnectionString("DefaultConnection");
        }


        public ViewResult Signup(int user_id = 0)
        {
            Signup userData = new Signup();
            //{
            //    fullname = "",
            //    password = "",
            //    address = "",
            //    email = "",
                
            //};
            if (user_id > 0)
            {

            }
            return View(userData);
        }

        [HttpPost]
        public IActionResult Signup(Signup signupData)
        {
            try
            {
                if (!string.IsNullOrWhiteSpace(signupData.email))
                {
                    var uploadedFilePath = _helper.UploadFile(signupData.uploaded_image, signupData.email);
                    var hashedPwd = GenerateHash(signupData.password);
                    using (SqlConnection conn = new SqlConnection(GetConnectionString()))
                    {
                        conn.Open();
                        SqlCommand command = conn.CreateCommand();
                        command.CommandType = System.Data.CommandType.StoredProcedure;
                        command.CommandText = "INSERT_USERS_DETAILS";
                        command.Parameters.AddWithValue("@USER_NAME", signupData.email);
                        command.Parameters.AddWithValue("@PASSWORD", signupData.password);
                        command.Parameters.AddWithValue("@USER_ADDRESS", signupData.address);
                        command.Parameters.AddWithValue("@FULL_NAME", signupData.fullname);
                        command.Parameters.AddWithValue("@IMAGE_PATH", uploadedFilePath.Result);
                        command.Parameters.AddWithValue("@ENC_PWD", hashedPwd);
                        command.Parameters.Add("@STATUS", System.Data.SqlDbType.Int, 1024);
                        command.Parameters["@STATUS"].Direction = System.Data.ParameterDirection.Output;
                        command.Parameters.Add("@MESSAGE", System.Data.SqlDbType.NVarChar, 200);
                        command.Parameters["@MESSAGE"].Direction = System.Data.ParameterDirection.Output;
                        command.ExecuteNonQuery();

                        int status = command.Parameters["@STATUS"].Value == DBNull.Value ? 0 : Convert.ToInt32(command.Parameters["@STATUS"].Value);
                        string message = Convert.ToString(command.Parameters["@MESSAGE"].Value);
                        TempData["SuccessMessage"] = message;
                        if(status == 0)
                        {
                            System.IO.File.Delete(uploadedFilePath);
                            //if (Path.Exists(uploadedFilePath))
                            //{
                                
                            //}
                        }
                    }
                    
                }
            }
            catch(Exception ex)
            {

            }
            return RedirectToAction("Login", "Login");
        }

        public IActionResult Login()
        {
            
            return View();
        }

        [HttpPost]
        public IActionResult Login(LoginModel logindata)
        {
            if(!string.IsNullOrWhiteSpace(logindata.username) && !string.IsNullOrWhiteSpace(logindata.password))
            {
                try
                {
                    using (SqlConnection conn = new SqlConnection(GetConnectionString()))
                    {
                        conn.Open();
                        SqlCommand command = conn.CreateCommand();
                        command.CommandType = System.Data.CommandType.StoredProcedure;
                        command.CommandText = "CHECK_LOGIN";
                        command.Parameters.AddWithValue("@USERNAME", logindata.username);
                        command.Parameters.AddWithValue("@PASSWORD", logindata.password);
                        string hashPassword = GenerateHash(logindata.password);
                        command.Parameters.AddWithValue("@HASHPASSWORD", hashPassword);
                        var returned_data = command.ExecuteNonQuery();
                        SqlDataReader reader = command.ExecuteReader();

                        if(reader.HasRows)
                        {
                            TempData["SuccessMessage"] = "Logged in successfully";

                        }
                        else
                        {
                            TempData["SuccessMessage"] = "Wrong username or password!!";
                        }
                    }
                }
                catch(Exception ex)
                {

                }
                
            }
            return View();
        }

        public string GenerateHash(string pwd)
        {
            string hashPassword = string.Empty;
            if (!string.IsNullOrWhiteSpace(pwd))
            {
                using (SHA256 sha256 = SHA256.Create())
                {
                    byte[] bytes = Encoding.UTF8.GetBytes(pwd);
                    byte[] converted_data = sha256.ComputeHash(bytes);

                    hashPassword = Encoding.UTF8.GetString(converted_data);
                }
            }
            return hashPassword;
        }

        
    }
}
