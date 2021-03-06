﻿using System.Collections.Generic;
using Examine;
using Example.Business.Logic.Models;
using Umbraco.Core.Models;
using Umbraco.Web;
using Umbraco.Web.PublishedCache;

namespace Example.Business.Logic.Services
{
    public interface ISiteSearchService
    {
        IEnumerable<SearchResult> GetRawResults(string term, bool isFuzzy);

        IEnumerable<IPublishedContent> ConvertSearchResultToPublishedContentWithTemplate(IEnumerable<SearchResult> rawSearchResults,
            IPublishedContentCache cache, UmbracoContext context);

        List<SearchResultItem> MapToCustomResults(IEnumerable<IPublishedContent> results);
    }
}