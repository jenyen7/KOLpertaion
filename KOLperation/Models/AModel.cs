using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.Linq;

namespace KOLperation.Models
{
    public partial class AModel : DbContext
    {
        public AModel()
            : base("name=AModel")
        {
        }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
        }

        public virtual DbSet<Coop> Coops { get; set; }
        public virtual DbSet<MessageHistory> MessageHistories { get; set; }
        public virtual DbSet<MessageHistoryContent> MessageHistoryContents { get; set; }
        public virtual DbSet<SponsoredContent> SponsoredContents { get; set; }
        public virtual DbSet<TagChannel> TagChannels { get; set; }
        public virtual DbSet<TagSector> TagSectors { get; set; }
        public virtual DbSet<UserCompany> UserCompanies { get; set; }
        public virtual DbSet<UserCompanyFavoriteKOL> CompanyFavoriteKOLs { get; set; }
        public virtual DbSet<UserKOL> UserKOLs { get; set; }
        public virtual DbSet<UserKOLChannelDetail> UserKOLChannelDetails { get; set; }
        public virtual DbSet<UserKOLFavoriteCompany> KOLFavoriteCompanies { get; set; }
        public virtual DbSet<UserKOLFavoriteSC> KOLFavoriteSCs { get; set; }
    }
}