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
        public int user_id { get; set; }
        public DateTime survey_date { get; set; }
    }
}
