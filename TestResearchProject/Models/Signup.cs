using System.ComponentModel.DataAnnotations;

namespace TestResearchProject.Models
{
    public class Signup
    {
        [Required]
        public string email { get; set; }
        public string password { get; set; }
        public string address { get; set; }
        public string fullname { get; set; }
        public IFormFile uploaded_image { get; set; }
    }
}
