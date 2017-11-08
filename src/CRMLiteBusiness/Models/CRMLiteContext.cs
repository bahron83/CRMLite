using Microsoft.EntityFrameworkCore;
using System;


namespace CRMLiteBusiness
{
    public class CRMLiteContext : DbContext
    {        
        public string ConnectionString { get; set; }

        public CRMLiteContext(DbContextOptions options) : base(options)
        {         
        }

        public DbSet<Company> Companies { get; set; }
        public DbSet<Contact> Contacts { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<ProductAttributeDecimal> ProductAttributeDecimal { get; set; }
        public DbSet<ProductAttributeInt> ProductAttributeInt { get; set; }
        public DbSet<ProductAttributeVarchar> ProductAtttributeVarchar { get; set; }
        public DbSet<Category> Categories { get; set; }        
        public DbSet<EavAttributeOption> EavAttributeOptions { get; set; }
        public DbSet<Contract> Contracts { get; set; }
        public DbSet<ContractItem> ContractItems { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Call> Calls { get; set; }
        public DbSet<Installation> Installations { get; set; }
        public DbSet<Visit> Visits { get; set; }


        protected override void OnModelCreating(ModelBuilder builder)
        {         
            base.OnModelCreating(builder);
        }


        //protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        //{
        //    base.OnConfiguring(optionsBuilder);

        //    if (optionsBuilder.IsConfigured)
        //        return;

        //    // Auto configuration
        //    ConnectionString = Configuration.GetValue<string>("Data:AlbumViewer:ConnectionString");
        //    optionsBuilder.UseSqlServer(ConnectionString);
        //}

    }
}