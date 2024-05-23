using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System.Data;
using TestResearchProject.Models;

namespace TestResearchProject.Controllers
{   
    public class FormsController : Controller
    {
        private readonly IConfiguration _configuration;
        private readonly IWebHostEnvironment _webHostEnvironment;
        public FormsController(IConfiguration configuration, IWebHostEnvironment webHostEnvironment)
        {
            _configuration = configuration;
            _webHostEnvironment = webHostEnvironment;
        }
        public string GetConnectionString()
        {
            return _configuration.GetConnectionString("DefaultConnection");
        }

        [Authorize(Policy = "ConfirmedEmployee", Roles ="Admin")]
        public IActionResult AddSurveyForm(int survey_form_id = 0)
        {
            SurveyFormModel surveyData = new SurveyFormModel();
            surveyData.survey_details = new List<SurveyFormDetailsModel>();
            try
            {
                surveyData.ID = survey_form_id;
                //Initializing the list with a value
                //List<SurveyFormDetailsModel> survey_details_list = [new SurveyFormDetailsModel() { device_name = "Device1", device_code = "0001"}];
                //survey_details_list.Add(new SurveyFormDetailsModel() { device_name = "device2", device_code="0002"});
                //surveyData.survey_details = survey_details_list;
                if (survey_form_id > 0)
                {
                    using (SqlConnection conn = new SqlConnection(GetConnectionString()))
                    {
                        conn.Open();
                        SqlCommand command = new SqlCommand("GET_SURVEY_FORM_FOR_EDIT", conn);
                        command.CommandType = CommandType.StoredProcedure;
                        
                        command.Parameters.AddWithValue("@SURVEY_FORM_ID", survey_form_id);
                        command.Parameters.Add("@MESSAGE", System.Data.SqlDbType.NVarChar, 200);
                        command.Parameters["@MESSAGE"].Direction = System.Data.ParameterDirection.Output;
                        SqlDataAdapter da = new SqlDataAdapter(command);
                        DataSet ds = new DataSet();
                        da.Fill(ds);

                        DataTable head_table = ds.Tables[0];
                        DataTable details_table = ds.Tables[1];

                        foreach(DataRow row in head_table.Rows)
                        {
                            surveyData.ID = Convert.ToInt32(row["ID"]);
                            surveyData.owner_name = Convert.ToString(row["OWNER_NAME"]);
                            surveyData.owner_email = Convert.ToString(row["OWNER_EMAIL"]);
                            surveyData.shop_name = Convert.ToString(row["SHOP_NAME"]);
                            surveyData.address = Convert.ToString(row["ADDRESS"]);
                            surveyData.phone_number = Convert.ToString(row["PHONE_NUMBER"]);
                            surveyData.survey_date = Convert.ToDateTime(row["SURVEY_DATE"]);

                            foreach (DataRow row1 in details_table.Rows)
                            {
                                SurveyFormDetailsModel sfdm = new SurveyFormDetailsModel();
                                sfdm.device_name = Convert.ToString(row1["DEVICE_NAME"]);
                                sfdm.device_code = Convert.ToString(row1["DEVICE_CODE"]);

                                surveyData.survey_details.Add(sfdm);
                            }
                        }

                    }
                }
                
            }
            catch(Exception ex)
            {

            }
            
            

            return View(surveyData);
        }

        
        [HttpPost]
        public IActionResult AddSurveyForm(SurveyFormModel surveyData)
        {
            
           
            if (surveyData != null)
            {
                //var user_id = HttpContext.Session.GetString("User_ID");
                //surveyData.survey_user_id = Convert.ToInt32(user_id);
                var claims_userId = HttpContext.User.FindFirst("UserID").Value;
                surveyData.survey_user_id = Convert.ToInt32(claims_userId);

                if (surveyData.ID > 0)
                {
                    
                    using (SqlConnection conn = new SqlConnection(GetConnectionString()))
                    {
                        conn.Open();
                        SqlTransaction transaction = conn.BeginTransaction();
                        try
                        {
                            SqlCommand command = conn.CreateCommand();
                            command.Transaction = transaction;
                            command.CommandType = CommandType.StoredProcedure;
                            command.CommandText = "UPDATE_FORM_HEAD";
                            command.Parameters.AddWithValue("@FORM_ID", surveyData.ID);
                            command.Parameters.AddWithValue("@OWNER_NAME", surveyData.owner_name);
                            command.Parameters.AddWithValue("@OWNER_EMAIL", surveyData.owner_email);
                            command.Parameters.AddWithValue("@SHOP_NAME", surveyData.shop_name);
                            command.Parameters.AddWithValue("@ADDRESS", surveyData.address);
                            command.Parameters.AddWithValue("@PHONE_NUMBER", surveyData.phone_number);
                            command.Parameters.Add("@MESSAGE", System.Data.SqlDbType.NVarChar, 200);
                            command.Parameters["@MESSAGE"].Direction = System.Data.ParameterDirection.Output;

                            command.ExecuteNonQuery();
                            string message = Convert.ToString(command.Parameters["@MESSAGE"].Value);
                            if (message == "SUCCESSFULLY UPDATED")
                            {
                                foreach (var items in surveyData.survey_details)
                                {
                                    SqlCommand command1 = conn.CreateCommand();
                                    command1.Transaction = transaction;
                                    command1.CommandType = CommandType.StoredProcedure;
                                    command1.CommandText = "UPDATE_FORMS_DETAILS";
                                    command1.Parameters.AddWithValue("@FORM_ID", surveyData.ID);
                                    command1.Parameters.AddWithValue("@DEVICE_NAME", items.device_name);
                                    command1.Parameters.AddWithValue("@DEVICE_CODE", items.device_code);
                                    command1.ExecuteNonQuery();

                                }
                            }
                            transaction.Commit();
                            TempData["SuccessMessage"] = message;
                        }
                        catch(Exception ex)
                        {
                            transaction.Rollback();
                        }
                        
                    }
                }
                else
                {
                    try
                    {
                        string jsonData = Newtonsoft.Json.JsonConvert.SerializeObject(surveyData);
                        using (SqlConnection conn = new SqlConnection(GetConnectionString()))
                        {
                            conn.Open();
                            SqlCommand command = conn.CreateCommand();
                            command.CommandType = System.Data.CommandType.StoredProcedure;
                            command.CommandText = "SAVE_SURVEY_HEAD_DETAILS";
                            command.Parameters.AddWithValue("@JSON_DATA", jsonData);
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
                    catch(Exception ex)
                    {

                    }
                }
            }
            else
            {
                TempData["SuccessMessage"] = "Data sent for save is null";
            }
            
            return View(surveyData);
        }

        [Authorize]
        public IActionResult SurveyFormList()
        {
            List<SurveyFormModel> surveyList = new List<SurveyFormModel>();
            
            try
            {
                //var loggedin_userid = HttpContext.Session.GetString("User_ID");
                var claims_userId = HttpContext.User.FindFirst("UserID").Value;

                using (SqlConnection conn = new SqlConnection(GetConnectionString()))
                {
                    conn.Open();
                    SqlCommand command = new SqlCommand("GET_SURVEY_FORM_LIST", conn);
                    command.CommandType = System.Data.CommandType.StoredProcedure;
                    
                    command.Parameters.AddWithValue("@USER_ID", Convert.ToInt32(claims_userId));
                    command.Parameters.Add("@MESSAGE", System.Data.SqlDbType.NVarChar, 200);
                    command.Parameters["@MESSAGE"].Direction = System.Data.ParameterDirection.Output;
                    SqlDataAdapter da = new SqlDataAdapter(command);
                    DataTable dt = new DataTable();
                    da.Fill(dt);
                    var rowss = dt.Rows;
                    foreach(DataRow row in dt.Rows)
                    {
                        SurveyFormModel formsData = new SurveyFormModel();
                        formsData.ID = Convert.ToInt32(row["ID"]);
                        formsData.owner_name = Convert.ToString(row["OWNER_NAME"]);
                        formsData.owner_email = Convert.ToString(row["OWNER_EMAIL"]);
                        formsData.shop_name = Convert.ToString(row["SHOP_NAME"]);
                        formsData.address = Convert.ToString(row["ADDRESS"]);
                        formsData.phone_number = Convert.ToString(row["PHONE_NUMBER"]);
                        formsData.survey_date = Convert.ToDateTime(row["SURVEY_DATE"]);
                        formsData.deviceName = Convert.ToString(row["DEVICES"]);

                        surveyList.Add(formsData);
                    }
                }

            }
            catch(Exception ex)
            {

            }
            
            return View(surveyList);
        }

        [Authorize(Roles ="ADMIN, SUPER_ADMIN")]
        public IActionResult ViewAdminAndSuperAdminOnlyPage()
        {
            return View();
        }

        [Authorize(Roles = "SURVEYER")]
        public IActionResult ViewSurveyerOnlyPage()
        {
            return View();
        }

    }
}
