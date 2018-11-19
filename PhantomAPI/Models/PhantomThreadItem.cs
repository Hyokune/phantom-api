using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PhantomAPI.Models
{
    public class PhantomThreadItem
    {
        public string Title { get; set; }
        public IFormFile Image { get; set; }
        public string Content { get; set; }
    }
}
