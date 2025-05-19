using Api.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Api.Data
{   
    // Definisem imena tabela u bazi
    public class ApplicationDBContext : IdentityDbContext<AppUser>
    {   // Umesto DbContext, koristim IdentityDbContext zbog logovanja. Kao DbContext, takodje u Progrma.cs registrujem ga.
        public ApplicationDBContext(DbContextOptions<ApplicationDBContext> options) : base(options)
        {
        }

        // Moraju tabele default! da Compiler moze da garantuje da nisu null tokom inicijalizacije njihove. 
        public DbSet<Stock> Stocks { get; set; } = default!;
        public DbSet<Comment> Comments { get; set; } = default!;
        public DbSet<Portfolio> Portfolios { get; set; } = default!;

        // Moram def Stock PK i Comment FK konekciju ovde pomocu OnModelCreating (ModelBuilder)

        // Seeds data into AspNetRoles table only when i trigger Migration from Package Manager Console
        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Portfolios tabela imace PK kao kombinaciju AppUserId i StockId zato sto Portfolio.cs tako napravljen
            builder.Entity<Portfolio>(x => x.HasKey(p => new { p.AppUserId, p.StockId }));
            
            builder.Entity<Portfolio>().HasOne(u => u.AppUser) // 1 Portfolio belongs to 1 AppUser (Portfolio ima AppUser polje)
                                       .WithMany(u => u.Portfolios) // 1 AppUser has many Portfolios (AppUser ima List<Portolio> Portfolios polje) 
                                       .HasForeignKey(p => p.AppUserId); // FK in Portfolios is AppUserId koji automatski gadja AppUser.Id (na osnovu imena EF spoji), jer Porftolio ima AppUserId polje
            
            builder.Entity<Portfolio>().HasOne(u => u.Stock) // 1 Portfolio bolongs to 1 Stock (Porftolio ima Stock polje)
                                       .WithMany(u => u.Portfolios) // 1 Stock can belong to many Portfolios (Stock ima list<Portoflio> Portfolios polje)
                                       .HasForeignKey(p => p.StockId); // FK in Porftolios is StockId koji automatski gadaj Stock.Id (na osnovu imena EF spoji), jer Portfolio ima StockId polje
            
            /* Objasnjene: AppUser1, AppUser2, Stock1, Stock2 i Stock3. AppUser1 ima Stock1 i Stock2, dok AppUser2 ima Stock1. 
                            Portfolio1 {AppUserId1, StockId1, AppUser1, Stock1}
                            Portfolio2 {AppUserId1, StockId2, AppUser1, Stock2}
                            Portfolio1 i Portfolio2 pripadaju AppUser1. 
                    
                            Portfolio3 {AppUserId2, StockId1, AppUser2, Stock1}
                            Portfolio3 pripada AppUser2. 
                            
                           + procitaj Objasnjene u AppUser i Stock za Portfolios polja i bice jasno + procitaj objasnjene u Portfolio.
            */

            List<IdentityRole> roles = new List<IdentityRole>
            {
                new IdentityRole
                {   
                    Id = "8d04dce2-969a-435d-bba4-df3f325983dc",
                    Name = "Admin",
                    NormalizedName = "ADMIN"
                },

                new IdentityRole
                {   
                    Id = "de1287c0-4b3e-4a3b-a7b5-5e221a57d55d",
                    Name = "User",
                    NormalizedName = "USER"
                }
            };

            builder.Entity<IdentityRole>().HasData(roles); // Seeds data to Migration
        }
    }
}
