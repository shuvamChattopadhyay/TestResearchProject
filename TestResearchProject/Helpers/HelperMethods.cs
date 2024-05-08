namespace TestResearchProject.Helpers
{
    public class HelperMethods
    {
        private IWebHostEnvironment _webHostEnvironment;
        public HelperMethods(IWebHostEnvironment webHostEnvironment)
        {
            _webHostEnvironment = webHostEnvironment;
        }
        public async Task<string> UploadFile(IFormFile file, string username)
        {
            string result = string.Empty;
            try
            {
                //Here "filename" is the folder name
                string uploads = Path.Combine(_webHostEnvironment.WebRootPath, "filename");
                string file_extension = System.IO.Path.GetExtension(file.FileName);
                string file_name = "Uploaded" + username + file_extension;
                string filePath = Path.Combine(uploads, file_name);
                using (Stream fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(fileStream);
                }
                result = filePath;
            }
            catch(Exception ex)
            {
                result = ex.Message;
            }
            return result;
        }
    }
}
