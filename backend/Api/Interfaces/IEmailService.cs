namespace Api.Interfaces
{
    public interface IEmailService
    {
        Task SendEmailAsync(string primalac, string subject, string message);

    }
}
