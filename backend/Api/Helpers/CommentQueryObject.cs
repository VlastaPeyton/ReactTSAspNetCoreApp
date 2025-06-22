namespace Api.Helpers
{
    /* Koristi se za [FromQuery] jer Axios GET Request u ReactTS moze poslati samo Request Header, a ne i Body, pa kroz Query Parameters (
    posle ? in URL) u FE prosledim ova polja ovim imenima i redosledom.
    */
    public class CommentQueryObject
    {
        public string Symbol { get; set; }
        public bool IsDescending { get; set; } = true;

        // https://localhost:port/api/comment/?Symbol=${symbol}&IsDescending=true - Ovako sam u FE uradio 
    }
}
