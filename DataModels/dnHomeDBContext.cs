using Microsoft.EntityFrameworkCore;
using DataModels;

namespace DataModels {
    public partial class dnHomeDBContext : DbContext {        
        public DbSet<BoilerSample> Boiler {get;set;}
        public DbSet<DHWSample> DHW {get; set;}
        public DbSet<LuftdatenSample> LuftdatenStation {get;set;}

        public DbSet<LatestBoilerSample> LatestBoiler {get;set;}
        public DbSet<LatestDHWSample> LatestDHW {get;set;}
        public DbSet<LatestLuftdatenSample> LatestLuftdaten {get; set;}
        
        public DbSet<DHWHeatingSample> DHWHeating { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {           
            optionsBuilder.UseNpgsql(@"Host=localhost;Database=dnhomeautomation;Username=pi;Password=sl0jn4pi");
        }
    }
}
