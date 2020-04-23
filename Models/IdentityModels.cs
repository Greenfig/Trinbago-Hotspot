using System.Data.Entity;
using Microsoft.AspNet.Identity.EntityFramework;
using Trinbago_MVC5.Migrations;

namespace Trinbago_MVC5.Models
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext()
            : base("ConnectionA", throwIfV1Schema: false)
        {
            Database.SetInitializer(new MigrateDatabaseToLatestVersion<ApplicationDbContext, Configuration>());
        }

        // From new build
        public static ApplicationDbContext Create()
        {
            return new ApplicationDbContext();
        }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Region>().Property(x => x.Lat).HasPrecision(18, 6);
            modelBuilder.Entity<Region>().Property(x => x.Lng).HasPrecision(18, 6);
            base.OnModelCreating(modelBuilder);
        }


        public DbSet<Info> InfosDB { get; set; }
        public DbSet<AdInfoTemplate> TemplateDB { get; set; }
        public DbSet<ClassifiedAdPhoto> AdPhotosDB { get; set; }
        public DbSet<ClassifiedAd> ClassifiedDB { get; set; }
        public DbSet<ClassifiedAdReport> ReportDB { get; set; }
        public DbSet<Country> CountryDB { get; set; }
        public DbSet<Region> RegionDB { get; set; }
        public DbSet<MiscInfo> MiscInfoDB { get; set; }
        public DbSet<Category> CategoryDB { get; set; }
        public DbSet<SubCategory> SubCategoryDB { get; set; }
        public DbSet<AdInfoString> AdInfoStringDB { get; set; }
        public DbSet<AdPromotion> AdPromotionDB { get; set; }
        public DbSet<SponsoredAd> SponsoredAdDB { get; set; }
        public DbSet<SponsoredPhoto> SponsoredAdPhotoDB { get; set; }
        public DbSet<GenericMessage> GenMessageDB { get; set; }
        public DbSet<PremiumUserData> PremUserDataDB { get; set; }
        public DbSet<PremiumUserInfo> PremUserInfoDB { get; set; }
        public DbSet<PremiumUserPhoto> PremUserPhotoDB { get; set; }
        public DbSet<PremiumUserReview> PremUserReviewDB { get; set; }
        public DbSet<Promotion> PromotionsDB { get; set; }
        public DbSet<AdViews> AdViewsDB { get; set; }
        public DbSet<CartItem> PromotionCartDB { get; set; }
        public DbSet<PromotionInfo> PromotionInfoDB { get; set; }
        public DbSet<Order_History> UserOrdersDB { get; set; }
        public DbSet<AnonymousUser> AnonymousUserDB { get; set; }
        public DbSet<Favourited> FavouirtedDb { get; set; }
    }    
}