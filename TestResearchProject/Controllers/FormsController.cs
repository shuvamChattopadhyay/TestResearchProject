using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
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
        public IActionResult AddSurveyForm(int survey_form_id = 0)
        {
            SurveyFormModel surveyData = new SurveyFormModel();
            surveyData.ID = survey_form_id;

            //Initializing the list with a value
            //List<SurveyFormDetailsModel> survey_details_list = [new SurveyFormDetailsModel() { device_name = "Device1", device_code = "0001"}];
            //survey_details_list.Add(new SurveyFormDetailsModel() { device_name = "device2", device_code="0002"});
            //surveyData.survey_details = survey_details_list;

            surveyData.survey_details = new List<SurveyFormDetailsModel>();

            return View(surveyData);
        }

        [HttpPost]
        public IActionResult AddSurveyForm(SurveyFormModel surveyData)
        {
            if (surveyData != null)
            {
                var user_id = HttpContext.Session.GetString("User_ID");
                surveyData.survey_user_id = Convert.ToInt32(user_id);
                string jsonData = Newtonsoft.Json.JsonConvert.SerializeObject(surveyData);
                try
                {
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
            else
            {
                TempData["SuccessMessage"] = "Data sent for save is null";
            }
            
            return View(surveyData);
        }

        public IActionResult SurveyFormList()
        {
            var loggedin_userid = HttpContext.Session.GetString("User_ID");
            List<SurveyFormModel> surveyList = new List<SurveyFormModel>();
            //Stored procedure to be written
            return View();
        }
    }
}
