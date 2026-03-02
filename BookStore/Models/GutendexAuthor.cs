using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BookStore.Models
{
    public class GutendexAuthor
    {
        public string name { get; set; }
        public int? birth_year { get; set; }
        public int? death_year { get; set; }
    }
}