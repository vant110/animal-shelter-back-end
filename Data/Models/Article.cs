using System;
using System.Collections.Generic;

namespace vant110.AnimalShelter.Data.Models
{
    public partial class Article
    {
        public short ArticleId { get; set; }
        public string Title { get; set; } = null!;
        public string Description { get; set; } = null!;
        public string Content { get; set; } = null!;
    }
}
