using Api.Data;
using MassTransit;
using MassTransit.Configuration;

namespace Api.MessageBroker
{
    public static class MassTransitExtensions
    {
        public static IServiceCollection AddMassTransitRabbitMQAndOutbox(this IServiceCollection services, IConfiguration configuration)
        {   
            services.AddMassTransit(x =>
            {
                // Outbox/Inbox pattern jer Api projekat je publisher, ali Inbox cu da iskljucim jer mi to treba za Consumer
                x.AddEntityFrameworkOutbox<ApplicationDBContext>(o =>
                {
                    o.QueryDelay = TimeSpan.FromSeconds(5); // Koliko cesto proverava Outbox table
                    o.UseSqlServer(); // Jer koristim SQL Server
                    o.DisableInboxCleanupService(); // Jer mi je ovo publisher microservice, dok u consumer microservicu ovo ne bih pisao 
                });
                // Nakon ovoga, dopuni OnModelCreating, pa uradi Migraciju sada da bi se Inbox/Outbox tabele napravile u bazi

                // Bez consumer za sada, samo publisher imam u solution

                x.SetKebabCaseEndpointNameFormatter();

                x.UsingRabbitMq((context, cfg) =>
                {
                    cfg.Host(new Uri(configuration["MessageBroker:Host"]!), host =>
                    {
                        host.Username(configuration["MessageBroker:UserName"]!);
                        host.Password(configuration["MessageBroker:Password"]!);
                    });

                    cfg.ConfigureEndpoints(context); // Pravi queueu za svakog consumera
                });
            });

            return services;
        }
    }
}
