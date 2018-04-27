using System.Collections.Generic;
using Examine;

namespace Text.Search.And.Spellchecking.Models
{
    public class RankedPhrase
    {
        public int Rank { get; set; }

        public IEnumerable<SearchResult> Results { get; set; }

        public string Content { get; set; }
    }
}