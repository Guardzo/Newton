namespace GuradzoApi.Models
{
    public class ApiError
    {
        public int Status { get; set; }
        public string Message { get; set; } = string.Empty;
        public string? StackTrace { get; set; }
    }
}
