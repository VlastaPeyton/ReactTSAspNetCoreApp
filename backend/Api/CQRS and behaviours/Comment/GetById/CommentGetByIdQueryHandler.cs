using Api.CQRS;
using Api.DTOs.CommentDTOs;
using Api.Exceptions_i_Result_pattern;
using Api.Interfaces;
using Api.Mapper;

namespace Api.CQRS_and_Validation.Comment
{
    // Query mora imati polja istog tipa i imena kao Request ako zelim da Mapster automatski mapira. Isto vazi i za Result i Response.
    // Koristim Result pattern jer ovo je biznis greska - pogledaj Result pattern.txt
    public record CommentGetByIdQuery(int Id) : IQuery<Result<CommentGetByIdResult>>; // Iako CommentGetByIdRequest ne postoji, Query mora postojati 
    public record CommentGetByIdResult(CommentDTOResponse commentDTOResponse);

    public class CommentGetByIdQueryHandler : IQueryHandler<CommentGetByIdQuery, Result<CommentGetByIdResult>>
    {   
        // CQRS Handler poziva Repository, a ne service, jer ako radim CQRS, ne koristim Service.
        private readonly ICommentRepository _commentRepository; 
        public CommentGetByIdQueryHandler(ICommentRepository commentRepository)
        {
            _commentRepository = commentRepository; 
        }

        // Mora metoda zbog interface. 
        public async Task<Result<CommentGetByIdResult>> Handle(CommentGetByIdQuery query, CancellationToken cancellationToken)
        {   
            var comment = await _commentRepository.GetByIdAsync(query.Id, cancellationToken);

            if (comment is null)
                //throw new WrongIdException("Pogresan id"); - postaje Result pattern, jer je biznis greska
                return Result<CommentGetByIdResult>.Fail("Pogresan id");

            // Mapiram Comment Entity u DTO i rati ga u GetById endpoint u CommentController 
            return Result<CommentGetByIdResult>.Success(new CommentGetByIdResult(comment.ToCommentDTOResponse()));
        }
    }
}
