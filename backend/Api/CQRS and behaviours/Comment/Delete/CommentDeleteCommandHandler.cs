using System.Windows.Input;
using Api.CQRS;
using Api.DTOs.CommentDTOs;
using Api.Exceptions;
using Api.Exceptions_i_Result_pattern;
using Api.Exceptions_i_Result_pattern.Exceptions;
using Api.Interfaces;
using Api.Mapper;
using Api.Models;
using FluentValidation;
using Microsoft.AspNetCore.Identity;

namespace Api.CQRS_and_Validation.Comment.Delete
{
    // Command mora imati polja istog tipa i imena kao Request ako zelim da Mapster automatski mapira. Isto vazi i za Result i Response.

    public record CommentDeleteCommand(int Id, string userName) : ICommand<CommentDeleteResult>;
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
        // CQRS Handler poziva Repository, a ne service, jer ako radim CQRS, ne koristim Service.
        private readonly ICommentRepository _commentRepository;
        private readonly UserManager<AppUser> _userManager;
        public CommentDeleteCommandHandler(ICommentRepository commentRepository, UserManager<AppUser> userManager)
        {
            _commentRepository = commentRepository; 
            _userManager = userManager;
        }

        // Mora metoda zbog interface
        public async Task<CommentDeleteResult> Handle(CommentDeleteCommand command, CancellationToken cancellationToken)
        {
            // Authorization kako user moze samo svoj komentar brisati 

            // Pronadji zeljeni komentar u bazi 
            var comment = await _commentRepository.GetByIdAsync(command.Id, cancellationToken);
            if (comment is null)
                throw new CommentNotFoundException("Comment not found");

            // Pronadji trenutnog usera koji oce da obrise comment
            var appUser = await _userManager.FindByNameAsync(command.userName);

            // User moze obrisati samo svoj komentar
            if (comment.AppUserId != appUser.Id)
                throw new NotYourCommentException("You can only delete your own comments"); // Exception, jer nije biznis logika da oces tudje komentare brisati 

            // Obrisi svoj komentar 
            var deletedComment = await _commentRepository.DeleteAsync(command.Id, cancellationToken);
            if (deletedComment is null)
                throw new CommentNotFoundException("Comment does not exist"); // Exception, jer nije biznis logika, vec neocekivana greska

            // Mapiram Comment Entity to DTO 
            return new CommentDeleteResult(comment.ToCommentDTOResponse());  // Vrati u Delete endpoint u CommentController 
        }
    }
}
