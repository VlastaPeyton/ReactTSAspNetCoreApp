using Api.CQRS;
using Api.DTOs.CommentDTOs;
using Api.Interfaces;
using Api.Mapper;

namespace Api.CQRS_and_Validation.Comment
{   
    // Query mora imati polja istog tipa i imena kao Request ako zelim da Mapster automatski mapira. Isto vazi i za Result i Response.

    public record CommentGetByIdQuery(int Id) : IQuery<CommentGetByIdResult>; // Iako CommentGetByIdRequest ne postoji, Query mora postojati 
    public record CommentGetByIdResult(CommentDTOResponse commentDTOResponse);

    public class CommentGetByIdQueryHandler : IQueryHandler<CommentGetByIdQuery, CommentGetByIdResult>
    {   
        // 
        private readonly ICommentRepository _commentRepository; 
        public CommentGetByIdQueryHandler(ICommentRepository commentRepository)
        {
            _commentRepository = commentRepository; // Trebao bi ceo Repository izbaciti iz Controller, ali samo ocu za 2 metode CQRS tako da nema veze 
        }

        // Mora metoda zbog interface
        public async Task<CommentGetByIdResult> Handle(CommentGetByIdQuery query, CancellationToken cancellationToken)
        {
            var comment = await _commentRepository.GetByIdAsync(query.Id, cancellationToken);

            if (comment is null)
                throw new Exception("Pogresan id");

            return new CommentGetByIdResult(comment.ToCommentDTOResponse()); // Vrati ga u GetById endpoint u CommentController 
        }
    }
}
