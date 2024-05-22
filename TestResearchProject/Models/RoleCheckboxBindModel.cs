namespace TestResearchProject.Models
{
    public class RoleCheckboxBindModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public List<int> Roles { get; set; }
        public string User_id { get; set; }
    }
}
