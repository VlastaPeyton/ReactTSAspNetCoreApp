using System.Windows.Input;
using Api.CQRS;
using Api.DTOs.CommentDTOs;
using Api.Exceptions;
using Api.Interfaces;
using Api.Mapper;
using FluentValidation;

namespace Api.CQRS_and_Validation.Comment.Delete
{
    // Command mora imati polja istog tipa i imena kao Request ako zelim da Mapster automatski mapira. Isto vazi i za Result i Response.

    public record CommentDeleteCommand(int Id) : ICommand<CommentDeleteResult>;
    public record CommentDeleteResult(CommentDTOResponse commentDTOResponse);

    // Validacija u MediatR pipeline samo za Command mora
    public class CommentDeleteCommandValidator : AbstractValidator<CommentDeleteCommand>
    {
        public CommentDeleteCommandValidator()
        {
            RuleFor(x =>  x.Id).NotEmpty();
        }
    }

    public class CommentDeleteCommandHandler : ICommandHandler<CommentDeleteCommand, CommentDeleteResult>
    {
        private readonly ICommentRepository _commentRepository;
        public CommentDeleteCommandHandler(ICommentRepository commentRepository)
        {
            _commentRepository = commentRepository; //Trebao bi ceo Repository izbaciti iz Controller, ali samo ocu za 2 metode CQRS tako da nema v
        }

        // Mora metoda zbog interface
        public async Task<CommentDeleteResult> Handle(CommentDeleteCommand command, CancellationToken cancellationToken)
        {
            var comment = await _commentRepository.DeleteAsync(command.Id, cancellationToken);
            if (comment is null)
                throw new CommentNotFoundException("Comment does not exist");

            return new CommentDeleteResult(comment.ToCommentDTOResponse());  // Posalje u Delete endpoint u CommentController 
        }
    }
}
