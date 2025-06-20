namespace Api.Helpers
{
    // Koristi se za [FromQuery] jer Axios GET Request u ReactTS moze poslati samo Request Header, a ne i Body, u GET Axios
    public class CommentQueryObject
    {
        public string Symbol { get; set; }
        public bool IsDescending { get; set; } = true;
    }
}
