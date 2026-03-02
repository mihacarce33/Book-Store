using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BookStore.Models
{
    public class GutendexBook
    {
        public int id { get; set; }
        public string title { get; set; }
        public string media_type { get; set; }
        public List<GutendexAuthor> authors { get; set; }
        public List<string> subjects { get; set; }
        public List<string> summaries { get; set; }
        public List<string> bookshelves { get; set; }
        public List<string> languages { get; set; }
        public bool? copyright { get; set; }
        public int? download_count { get; set; }
        public List<GutendexAuthor> translators { get; set; }
        public Dictionary<string, string> formats { get; set; }
    }
}