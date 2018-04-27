using System.Collections.Generic;
using Business.Logic.Models;

namespace Example.App.Config.Models
{
    public class SiteSearchVm
    {
        public string SearchedTerm { get; set; }
        public string SpellCheckerSuggestionWord { get; set; }
        public List<SearchResultItem> SearcResults { get; set; }
    }
}