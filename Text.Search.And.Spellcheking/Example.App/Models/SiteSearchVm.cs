using System.Collections.Generic;
using Example.Business.Logic.Models;

namespace Example.App.Models
{
    public class SiteSearchVm
    {
        public string SearchedTerm { get; set; }
        public string SpellCheckerSuggestionWord { get; set; }
        public List<SearchResultItem> SearcResults { get; set; }
    }
}