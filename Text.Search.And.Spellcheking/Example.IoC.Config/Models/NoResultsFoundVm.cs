using System.Collections.Generic;

namespace Example.App.Config.Models
{
    public class NoResultsFoundVm
    {
        public string SearchedTerm { get; set; }
        public List<string> Keywords { get; set; }
    }
}