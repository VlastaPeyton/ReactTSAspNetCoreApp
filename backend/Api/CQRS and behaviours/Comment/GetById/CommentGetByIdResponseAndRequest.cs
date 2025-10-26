using Api.DTOs.CommentDTOs;

namespace Api.CQRS_and_Validation.Comment
{
    // Umesto dosadasnjeg "id" argumenta u GetById endpoint, koristim CQRS fazon 

    //public record CommentGetByIdRequest(int Id); // Mora imati polja istog imena i tipa kao CommentGetByIdQuery
    /* Moze i "id" da se zove, ali je ruzno. Svakako, .NET automatski mapira "id" iz [HttpGet("{id:int}")] u ovaj "Id" jer je case insesitive
      obzirom da samo 1 argument prostog tipa, onda ne treba mi Request objekat */

    public record CommentGetByIdResponse(CommentDTOResponse commentDTOResponse); // Mora imati polja istog imena i tipa kao CommentGetByIdResult
    /* Ako clientu posaljem ovakav response, moram i FE da zamenim zbog trenutnog "return Ok(commentDTOResponse)", jer onda bih imao 
      "return Ok(CommentGetByIdResponse)" pa FE mora da otpakuje CommentDTOResponse, pa zato necu ni da koristim ovaj Response iako bih trebao.
    */
}
