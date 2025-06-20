﻿using Api.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Api.Data
{   
    // Definisem imena tabela u bazi
    public class ApplicationDBContext : IdentityDbContext<AppUser>
    {   // Umesto DbContext, koristim IdentityDbContext zbog Login/Register of AppUser(IdentityUser). Kao i DbContext, takodje u Progrma.cs registrujem IdentityDbContext

        public ApplicationDBContext(DbContextOptions<ApplicationDBContext> options) : base(options)
        {
        }

        // Moraju tabele default! da Compiler moze da garantuje da nisu null tokom inicijalizacije njihove. 
        public DbSet<Stock> Stocks { get; set; } = default!;
        public DbSet<Comment> Comments { get; set; } = default!;
        public DbSet<Portfolio> Portfolios { get; set; } = default!;

        // Seeds data into AspNetRoles table only when i run Migration from Package Manager Console and define FK-PK relatiosnhips for Entities
        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Portfolios tabela imace PK kao kombinaciju AppUserId i StockId zato sto Portfolio.cs tako napravljen jer u Portfolio.cs ne mogu compositni PK da napravim, nego ovde moram
            builder.Entity<Portfolio>(x => x.HasKey(p => new { p.AppUserId, p.StockId }));
            
            builder.Entity<Portfolio>().HasOne(u => u.AppUser) // 1 Portfolio belongs to 1 AppUser (Portfolio ima AppUser polje)
                                       .WithMany(u => u.Portfolios) // 1 AppUser has many Portfolios (AppUser ima List<Portfolio> Portfolios polje) 
                                       .HasForeignKey(p => p.AppUserId); // FK in Portfolio is AppUserId koji automatski gadja AppUser.Id (na osnovu imena EF ih mapira ), jer Porftolio ima AppUserId polje
            
            builder.Entity<Portfolio>().HasOne(u => u.Stock) // 1 Portfolio bolongs to 1 Stock (Porftolio ima Stock polje)
                                       .WithMany(u => u.Portfolios) // 1 Stock can belong to many Portfolios (Stock ima list<Portoflio> Portfolios polje)
                                       .HasForeignKey(p => p.StockId); // FK in Porftolio is StockId koji automatski gadaj Stock.Id (na osnovu imena EF ih mapira), jer Portfolio ima StockId polje
            
            // Zbog ova 2 iznad + List<Portfolios> u AppUser/Stock, kada radim LINQ za AppUser/Stock, pomocu Include dohvatam i Portfolio

            /* Objasnjene: Neka postoji AppUser1, AppUser2, Stock1, Stock2 i Stock3. 
                            AppUser1 ima Stock1 i Stock2. 
                            AppUser2 ima Stock1.
             
                            Portfolio1 {AppUserId1, StockId1, AppUser1, Stock1}
                            Portfolio2 {AppUserId1, StockId2, AppUser1, Stock2}
                            Portfolio1 i Portfolio2 pripadaju AppUser1. 
                    
                            Portfolio3 {AppUserId2, StockId1, AppUser2, Stock1}
                            Portfolio3 pripada AppUser2. 
                            
                           + procitaj objasnjene u AppUser i Stock za Portfolios polja i bice jasno + procitaj objasnjene u Portfolio.
            */

            /* Nisam setovao kao iznad relationship za PK-FK for Stock-Comment, jer u Comment postoji StockId i Stock polje, pa ce EF sam da zakljuci da 1 Stock can have Many Comments. EF zna automatski, zbog imena polja, da mapira StockId u Comment sa Id u Stock.
            
               Nisam setovao kao iznad relationship za PK-FK for AppUser-Comment iako Comment ima AppUserId i AppUser polja, jer EF zakljuci da 1 AppUser can have Many Comments. EF  zna automatski, zbog imena polja, da mapira AppUserId u Comment sa Id u AppUser.
                
               Nisam ni morao napisati ona 2 iznad za Portfolio-AppUser/Comment relatioship, jer na osnovu imena polja, EF bi sam to napravio obzriom da je jednostavna veza.
             */

            // IdentityRole moze imati proizvoljno name of Role
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

            builder.Entity<IdentityRole>().HasData(roles); // Seeds data in Migration to AspNetRoles only if it is empty 
        }
    }
}
