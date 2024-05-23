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
using Newtonsoft.Json;



namespace TestResearchProject.Controllers
{
    [Authorize]
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

        [AllowAnonymous]
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


        [AllowAnonymous]
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


        [AllowAnonymous]
        public IActionResult Login()
        {
            
            return View();
        }

        [AllowAnonymous]
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
                        SqlCommand command = new SqlCommand("CHECK_LOGIN", conn);
                        command.CommandType = System.Data.CommandType.StoredProcedure;
                        //command.CommandText = "CHECK_LOGIN";
                        command.Parameters.AddWithValue("@USERNAME", logindata.username);
                        command.Parameters.AddWithValue("@PASSWORD", logindata.password);
                        string hashPassword = GenerateHash(logindata.password);
                        command.Parameters.AddWithValue("@HASHPASSWORD", hashPassword);
                        SqlDataAdapter da = new SqlDataAdapter(command);
                        DataTable dt = new DataTable();
                        da.Fill(dt);

                        string userID = Convert.ToString(dt.Rows[0]["ID"]);
                        string userGUID = Convert.ToString(dt.Rows[0]["USER_GUID"]);
                        string userName = Convert.ToString(dt.Rows[0]["USER_NAME"]);
                        string userFullName = Convert.ToString(dt.Rows[0]["FULL_NAME"]);
                        var userRoles = dt.Rows[0]["ROLES"] != DBNull.Value ? Convert.ToString(dt.Rows[0]["ROLES"]).Split(",") : Array.Empty<string>();
                        
                        if (dt.Rows.Count > 0 && ret != null)
                        {   
                            //TempData["SuccessMessage"] = "Logged in successfully";
                            HttpContext.Session.SetString("User_ID", ret.ToString());

                            //Creating the security context
                            var claims = new List<Claim>
                            {
                                new Claim(ClaimTypes.Name,userName),
                                new Claim(ClaimTypes.Hash, hashPassword),
                                new Claim("Status","Confirmed"),
                                new Claim(ClaimTypes.Role, "Admin"),
                                new Claim("UserID",userID),
                                new Claim("Name",userFullName)
                            };
                            
                            foreach(var item in userRoles)
                            {
                                claims.Add(new Claim(ClaimTypes.Role, item));
                            }

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

        
        public IActionResult Dashboard()
        {
            return View();
        }

       
        public IActionResult Unauthorized()
        {
            return View();
        }


        public IActionResult MapUserRole()
        {
            return View();
        }

        [HttpPost]
        public IActionResult MapUserRole(RoleCheckboxBindModel data)
        {
            try
            {
                if (data != null)
                {
                    string json = JsonConvert.SerializeObject(data);
                    using (SqlConnection conn = new SqlConnection(GetConnectionString()))
                    {
                        conn.Open();
                        SqlCommand comm = conn.CreateCommand();
                        comm.CommandType = CommandType.StoredProcedure;
                        comm.CommandText = "SET_ROLES_AGAINST_USER";
                        comm.Parameters.AddWithValue("@JSON_VALUE", json);
                        comm.Parameters.Add("@STATUS", System.Data.SqlDbType.Int, 1024);
                        comm.Parameters["@STATUS"].Direction = System.Data.ParameterDirection.Output;
                        comm.Parameters.Add("@MESSAGE", System.Data.SqlDbType.NVarChar, 200);
                        comm.Parameters["@MESSAGE"].Direction = System.Data.ParameterDirection.Output;
                        comm.ExecuteNonQuery();

                        int status = comm.Parameters["@STATUS"].Value == DBNull.Value ? 0 : Convert.ToInt32(comm.Parameters["@STATUS"].Value);
                        string message = Convert.ToString(comm.Parameters["@MESSAGE"].Value);
                        TempData["message"] = message;
                    }
                }
            }
            catch(Exception ex)
            {

            }
            
            return View();
        }

        public string BindUserList()
        {
            string jsonValue = string.Empty;
            try
            {
                using (SqlConnection conn = new SqlConnection(GetConnectionString()))
                {
                    conn.Open();
                    SqlCommand comm = new SqlCommand("GET_USERS_LIST", conn);
                    comm.CommandType = CommandType.StoredProcedure;
                    //comm.CommandText = "GET_USERS_LIST";
                    SqlDataAdapter da = new SqlDataAdapter(comm);
                    DataTable dt = new DataTable();
                    da.Fill(dt);

                    jsonValue = JsonConvert.SerializeObject(dt); 
                }
            }
            catch(Exception ex)
            {

            }

            return jsonValue;

        }

        public string BindCheckBox(string user_id)
        {
            string jsonValue = string.Empty;
            List<RoleCheckboxBindModel> datamodel = new List<RoleCheckboxBindModel>();
            try
            {
                using (SqlConnection conn = new SqlConnection(GetConnectionString()))
                {
                    conn.Open();
                    SqlCommand comm = new SqlCommand("GET_ROLE_CHECKBOX", conn);
                    comm.CommandType = CommandType.StoredProcedure;
                    comm.Parameters.AddWithValue("@USER_ID", user_id);
                    SqlDataAdapter da = new SqlDataAdapter(comm);
                    DataSet ds = new DataSet();
                    da.Fill(ds);
                    DataTable roles = ds.Tables[0];
                    DataTable selected_role = ds.Tables[1];

                    foreach(DataRow dr in roles.Rows)
                    {
                        RoleCheckboxBindModel data = new RoleCheckboxBindModel();
                        data.Roles = new List<int>();
                        data.Id = Convert.ToInt32(dr["Id"]);
                        data.Name = Convert.ToString(dr["Name"]);

                        List<int> role = new List<int>();
                        foreach (DataRow row in selected_role.Rows)
                        {   
                            role.Add(Convert.ToInt32(row["Id"]));
                        }
                        data.Roles = role;
                        datamodel.Add(data);
                    }
                    jsonValue = JsonConvert.SerializeObject(datamodel);
                }
            }
            catch(Exception ex)
            {

            }
            return jsonValue;
        }

    }
}
