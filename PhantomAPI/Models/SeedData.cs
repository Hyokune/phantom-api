using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PhantomAPI.Models
{
    public class SeedData
    {
        public static void Initialize(IServiceProvider serviceProvider)
        {
            using (var context = new PhantomAPIContext(
                serviceProvider.GetRequiredService<DbContextOptions<PhantomAPIContext>>()))
            {
                // Look for any phantom threads.
                if (context.PhantomThread.Count() > 0)
                {
                    return;   // DB has been seeded
                }

                context.PhantomThread.AddRange(
                    new PhantomThread
                    {
                        Title = "Welcome to PHANTOM",
                        Url = "https://i.imgur.com/Fs9wzno.png",
                        Uploaded = "01-01-00 00:00:00",
                        Width = "200",
                        Height = "200",
                        Content = "Welcome to p h a n t o m.",
                        User = "Hades"
                    }
                );

                context.SaveChanges();
            }
        }
    }
}
