using System.Reflection.Emit;
using System.Runtime.ConstrainedExecution;
using Api.Models;
using Api.Value_Objects;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Api.Data
{   
    // DbContext sluzi za definisanje imena tabela, a u OnModelCreating pravim PK, FK, PK-FK relacije, Seedujem tabelu ako treba, definisem uslove za zeljene kolone tabele i Indexiranje tabele ako treba.
    public class ApplicationDBContext : IdentityDbContext<AppUser>
    {   // Umesto DbContext, koristim IdentityDbContext zbog Login/Register of AppUser(IdentityUser). Kao i DbContext, takodje u Progrma.cs registrujem IdentityDbContext

        public ApplicationDBContext(DbContextOptions<ApplicationDBContext> options) : base(options) {}

        // Moraju tabele default! da Compiler moze da garantuje da nisu null tokom inicijalizacije njihove. 
        public DbSet<Stock> Stocks { get; set; } = default!; // Moglo je i DbSet<Stock> => Set<Stock>() jer EF Core automatski setuje.
        public DbSet<Comment> Comments { get; set; } = default!;
        public DbSet<Portfolio> Portfolios { get; set; } = default!;

        // Seeds data into AspNetRoles table only when i run Migration from Package Manager Console and define FK-PK relatiosnhips for Entities
        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Comment configuration stavljam u blok jer lakse je za pratiti 
            builder.Entity<Comment>(entity =>
            {   /* Dok je Id iz Comment/Stock primary type, nisam morao setovao PK za Comment, AppUser i Stock, jer EF Core automatski zna da je Id kolona PK obzirom da je u svakoj klasi Id Guid ili int tip, i prikom Create Comment/Stock EF Core ce automatski dodeliti vrednost to Id.
                Da je Id bar u jednoj klasi bio custom type (npr CommentId.cs - pogledaj EShopMicroservices Ordering service) morao bih da rucno definisem PK i HasConversion i potencijalno ValueGeneratedOnAdd ako zelim da automatski baza generise vrednost u Id obzirom da kod custom type EF Core ne generise automtaski novi Id.
                Samo za Comment sam stavio custom type for Id da vidimo kako to izgleda, jer nema potrebe za sad i u Stock to raditi. */
                entity.Property(c => c.Id).HasConversion(
                    id => id.Value,                 // Write to DB
                    value => CommentId.Of(value)    // Read from DB
                ).ValueGeneratedOnAdd(); // U CommentRepository CreateComment metodi automatski se generise Id vrednost kao dok je Id of Comment bio int tipa.
            });

            // Portfolio configuration stavljam u blok jer lakse je za pratiti
            builder.Entity<Portfolio>(entity =>
            {   // entity = builder.Entity<Portfolio>

                // Portfolios tabela imace PK kao kombinaciju AppUserId i StockId zato sto Portfolio.cs tako napravljen i jer u Portfolio.cs ne mogu compositni PK da napravim, nego ovde moram
                entity.HasKey(p => new { p.AppUserId, p.StockId }); // Ovo je samo po sebi index za ovaj composite PK
                                                                    // Zbog ovoga, moram prvo dodati AppUser u bazu i Stock, kako bi tokom AddAsync(portfolio) u PortfolioRepository mogao da doda ga u bazu, jer composite PK ne moze baza da popuni sama kao obican Id PK, vec to moram da osiguram prethodno

                // Zbog explicitno defisanja PK za Portfolio, moram definisati 1-to-many AppUser/Stock-Portfolio veze

                entity.HasOne(u => u.AppUser) // 1 Portfolio belongs to 1 AppUser (Portfolio ima AppUser polje)
                      .WithMany(u => u.Portfolios) // 1 AppUser has many Portfolios (AppUser ima List<Portfolio> Portfolios polje) 
                      .HasForeignKey(p => p.AppUserId); // FK in Portfolio is AppUserId koji automatski gadja AppUser.Id (na osnovu imena EF ih mapira ), jer Porftolio ima AppUserId polje

                entity.HasOne(u => u.Stock) // 1 Portfolio bolongs to 1 Stock (Porftolio ima Stock polje)
                      .WithMany(u => u.Portfolios) // 1 Stock can belong to many Portfolios (Stock ima list<Portoflio> Portfolios polje)
                      .HasForeignKey(p => p.StockId); // FK in Porftolio is StockId koji automatski gadaj Stock.Id (na osnovu imena EF ih mapira), jer Portfolio ima StockId polje
               
                // Zbog ova 2 iznad + List<Portfolios> u AppUser/Stock, kada radim LINQ za AppUser/Stock, pomocu Include dohvatam i Portfolio. Ovo se zove Eager loading kad Include radim.
            });

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
               
               DB Indexing:
                  Svaka Id (PK) je autoamtski Index, pa u LINQ query by Id je brz veoma tj najbrzi moguci jer najbrze pretrazuje by integer. 
                  Onaj Endpoint koji se, zbog FE, veoma cesto koristi, a pritom njegov LINQ queries DB by column which is not PK, mogu setovati tu kolonu kao Index da bi bazu na osnovu toga brze pretrazio. 
                Ovo ima smisla uvek, ali narocito ako se cesto koristi taj Endpoint. 
                  Kada je neka kolona postavljena kao Index, napravi se data structure (lookup table) koja vrsti binary search O(logn) trajanja, dok ako pretrazujem bazu preko te kolone to je O(n) jer prolazi kroz sve vrste.
                  Indexed column se koristi u LINQ samo kad imamo == ili StartsWith, dok za Contains or EndsWith Indexing string column ne pomaze.
             */

            // U FE cesto brisem portfolio(stock) koji sam added to my portfolios, pa se DeletePortfolio metoda cesto koristi,a tamo LINQ by Stock.Symbol i zato mu dodelim index da brze pretrazuje. Isto vazi i za GetAllAsync jer cesto gledam Company profile za neki stock pa ovaj Endpoint cesto pokrecem.
            builder.Entity<Stock>()
                   .HasIndex(s => s.Symbol)
                   .IsUnique();

            // IdentityRole table configuration zbog AppUser:IdentityUser
            builder.Entity<IdentityRole>(entity =>
            {
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

                entity.HasData(roles);// Seeds data in Migration to AspNetRoles only if it is empty 
            });
        }
    }
}
