using System.ComponentModel.DataAnnotations;

namespace TestResearchProject.Models
{
    public class Signup
    {
        public int ID { get; set; }
        [Required]
        public string email { get; set; }
        public string password { get; set; }
        public string address { get; set; }
        public string fullname { get; set; }
        public IFormFile uploaded_image { get; set; }
        public string file_path { get; set; }

    }
}
