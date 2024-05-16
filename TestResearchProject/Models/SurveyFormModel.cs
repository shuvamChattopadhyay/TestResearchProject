namespace TestResearchProject.Models
{
    public class SurveyFormModel
    {
        public int ID { get; set; }
        public string owner_name { get; set; }
        public string shop_name { get; set; }
        public string owner_email { get; set; }
        public string address { get; set; }
        public string phone_number { get; set; }
        public int survey_user_id { get; set; }
        public DateTime survey_date { get; set; }
        public List<SurveyFormDetailsModel> survey_details { get; set; }
        public string? deviceName { get; set; }
    }
}
