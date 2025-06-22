namespace Api.Helpers
{   
    /* Koristi se za [FromQuery] jer Axios GET Request u ReactTS moze poslati samo Request Header, a ne i Body, pa kroz Query Parameters (
     posle ? in URL) u FE prosledim ova polja ovim imenima i redosledom.
      */
    public class QueryObject
    {
        // Zbog https://localhost:port/api/stock/?symbol=tsla 
        public string? Symbol { get; set; } = null;

        // Zbog https://localhost:port/api/stock/?companyname=tesla
        public string? CompanyName { get; set; } = null;

        // Zbog https://localhost:port/api/stock/?sortby=nesto
        public string? SortBy { get; set; } = null;

        // Zbog https://localhost:port/api/stock/?isdescending=true
        public bool IsDescending { get; set; } = false;

        // Zbog https://localhost:port/api/stock/pangenumber=2
        public int PageNumber { get; set; } = 1;// Pagination 

        // Zbog https://localhost:port/api/stock/pagesize=20
        public int PageSize { get; set; } = 10;// Pagination 
    }
}
