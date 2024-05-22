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
using System.Data;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;



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


        public IActionResult Signup(int user_id = 0)
        {
            Signup userData = new Signup() { ID = user_id};
            
            if (user_id > 0)
            {
                using (SqlConnection conn = new SqlConnection(GetConnectionString()))
                {
                    conn.Open();
                    SqlCommand command = new SqlCommand("GET_USER_DATA", conn);
                    command.CommandType = CommandType.StoredProcedure;
                    
                    command.Parameters.AddWithValue("@USER_ID", user_id);
                    
                    SqlDataAdapter da = new SqlDataAdapter(command);
                    
                    DataTable dt = new DataTable();
                    da.Fill(dt);
                    
                    //Needed to be changed, follow reference to get the value by field name
                    foreach(DataRow row in dt.Rows)
                    {
                        userData.email = row["USER_NAME"].ToString();
                        userData.password = row["PASSWORD"].ToString();
                        userData.address = row["USER_ADDRESS"].ToString();
                        //userData.uploaded_image = row["UPLOADED_IMG"].ToString();
                        userData.fullname = row["FULL_NAME"].ToString();
                        userData.file_path = row["UPLOADED_IMG"].ToString();
                        userData.ID = user_id;
                    }
                }
            }
            return View(userData);
        }

        
        [HttpPost]
        public IActionResult Signup(Signup signupData)
        {
            try
            {
                if(signupData.ID == 0)
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
                            if (status == 0)
                            {
                                System.IO.File.Delete(uploadedFilePath);
                                //if (Path.Exists(uploadedFilePath))
                                //{

                                //}
                            }
                        }

                    }
                    else
                    {
                        throw new Exception();
                    }
                }
                else
                {
                    if (!string.IsNullOrWhiteSpace(signupData.email))
                    {
                        //var uploadedFilePath = _helper.UploadFile(signupData.uploaded_image, signupData.email);
                        var hashedPwd = GenerateHash(signupData.password);
                        using (SqlConnection conn = new SqlConnection(GetConnectionString()))
                        {
                            conn.Open();
                            SqlCommand command = conn.CreateCommand();
                            command.CommandType = System.Data.CommandType.StoredProcedure;
                            command.CommandText = "UPDATE_USER_DETAILS";
                            command.Parameters.AddWithValue("@USER_ID", signupData.ID);
                            command.Parameters.AddWithValue("@USER_NAME", signupData.email);
                            command.Parameters.AddWithValue("@PASSWORD", signupData.password);
                            command.Parameters.AddWithValue("@USER_ADDRESS", signupData.address);
                            command.Parameters.AddWithValue("@FULL_NAME", signupData.fullname);
                            command.Parameters.AddWithValue("@ENC_PWD", hashedPwd);
                            command.Parameters.AddWithValue("@IMAGE_PATH",signupData.file_path );
                            command.Parameters.Add("@STATUS", System.Data.SqlDbType.Int, 1024);
                            command.Parameters["@STATUS"].Direction = System.Data.ParameterDirection.Output;
                            command.Parameters.Add("@MESSAGE", System.Data.SqlDbType.NVarChar, 200);
                            command.Parameters["@MESSAGE"].Direction = System.Data.ParameterDirection.Output;
                            command.ExecuteNonQuery();

                            int status = command.Parameters["@STATUS"].Value == DBNull.Value ? 0 : Convert.ToInt32(command.Parameters["@STATUS"].Value);
                            string message = Convert.ToString(command.Parameters["@MESSAGE"].Value);

                            TempData["SuccessMessage"] = message;
                        }

                    }
                    else
                    {
                        throw new Exception();
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
        public async Task<IActionResult> Login(LoginModel logindata)
        {
            var ret = 0;
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
                        ret = Convert.ToInt32(command.ExecuteScalar());
                        SqlDataReader reader = command.ExecuteReader();
                        
                        if(reader.HasRows && ret != null)
                        {   
                            //TempData["SuccessMessage"] = "Logged in successfully";
                            HttpContext.Session.SetString("User_ID", ret.ToString());

                            //Creating the security context
                            var claims = new List<Claim>
                            {
                                new Claim(ClaimTypes.Name, logindata.username),
                                new Claim(ClaimTypes.Hash, hashPassword),


                                //Adding hardcoded claims and roles for now, change it later
                                new Claim("Status","Confirmed"),
                                new Claim(ClaimTypes.Role, "Admin")
                            };
                            //Adding the claim to Identity
                            var identity = new ClaimsIdentity(claims, "MySecurity");
                            ClaimsPrincipal principal = new ClaimsPrincipal(identity);

                            await HttpContext.SignInAsync("MySecurity", principal);

                            //Redirecting to Dashboard page
                            return RedirectToAction("Dashboard", "Login");
                        }
                        else
                        {
                            TempData["SuccessMessage"] = "Wrong username or password!!";
                            return View();
                        }
                    }
                }
                catch(Exception ex)
                {

                }
                
            }
            //return RedirectToAction("Signup", "Login", new { user_id = ret });
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


        public IActionResult LogOut()
        {
            HttpContext.Session.Clear();
            HttpContext.SignOutAsync("MySecurity");

            return RedirectToAction("Login", "Login");
        }

        [Authorize]
        public IActionResult Dashboard()
        {
            return View();
        }

        [AllowAnonymous]
        public IActionResult Unauthorized()
        {
            return View();
        }
    }
}
