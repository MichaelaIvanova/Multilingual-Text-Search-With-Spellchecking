using System.Collections.Generic;
using Examine;
using Umbraco.Core.Models;
using Umbraco.Web;
using Umbraco.Web.PublishedCache;
using SearchResultItem = Text.Search.And.Spellcheking.Models.SearchResultItem;

namespace Text.Search.And.Spellcheking.Services
{
    public interface ISiteSearchService
    {
        IEnumerable<SearchResult> GetRawResults(string term, bool isFuzzy);

        IEnumerable<IPublishedContent> ConvertSearchResultToPublishedContentWithTemplate(IEnumerable<SearchResult> rawSearchResults,
            IPublishedContentCache cache, UmbracoContext context);

        List<SearchResultItem> MapToCustomResults(IEnumerable<IPublishedContent> results);
    }
}