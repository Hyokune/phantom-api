using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace PhantomAPI.Models
{
    public class PhantomAPIContext : DbContext
    {
        public PhantomAPIContext (DbContextOptions<PhantomAPIContext> options)
            : base(options)
        {
        }

        public DbSet<PhantomAPI.Models.PhantomThread> PhantomThread { get; set; }
    }
}
