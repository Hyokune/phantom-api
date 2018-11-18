using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PhantomAPI.Models
{
    public class PhantomThread
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Url { get; set; }
        public string Uploaded { get; set; }
        public string Width { get; set; }
        public string Height { get; set; }
        public string Content { get; set; }
        public string User { get; set; }
    }
}
