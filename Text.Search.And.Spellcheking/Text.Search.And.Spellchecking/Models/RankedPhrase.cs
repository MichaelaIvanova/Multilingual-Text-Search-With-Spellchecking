using System.Collections.Generic;
using Examine;

namespace Example.BusinessLogic.Models
{
    public class RankedPhrase
    {
        public int Rank { get; set; }

        public IEnumerable<SearchResult> Results { get; set; }

        public string Content { get; set; }
    }
}