namespace Api.Helpers
{   
    // Pogledaj QueryObject i bice ti jasno sta je ovo
    public class CommentQueryObject
    {
        public string Symbol { get; set; }
        public bool IsDescending { get; set; } = true;
    }
}
