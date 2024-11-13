namespace PicAppAPI.Models
{
    public class ImageData
    {
        public int Id { get; set; }
        public string FileName { get; set; }
        public DateTime UploadDate { get; set; }
        public byte[]? ImageBytes { get; set; } 
        public string FilePath { get; set; } 
    }

}
