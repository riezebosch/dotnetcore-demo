using System;
using Microsoft.EntityFrameworkCore;


namespace magazijn_service.Model
{
    public class VoorraadContext : DbContext
    {
        public VoorraadContext(DbContextOptions<VoorraadContext> options)
            : base(options)
        {
        }

        public DbSet<Artikel> Artikelen { get; set; }
    }
}