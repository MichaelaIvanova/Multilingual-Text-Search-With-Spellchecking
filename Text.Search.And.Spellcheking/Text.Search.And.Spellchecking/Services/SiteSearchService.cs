using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Web;
using Examine;
using Examine.LuceneEngine.Providers;
using Examine.SearchCriteria;
using Text.Search.And.Spellchecking.Helpers;
using Text.Search.And.Spellchecking.Models;
using Umbraco.Core;
using Umbraco.Core.Models;
using Umbraco.Web;
using Umbraco.Web.PublishedCache;

namespace Text.Search.And.Spellchecking.Services
{
    public class SiteSearchService : ISiteSearchService
    {
        private readonly BaseLuceneSearcher _searcher;
        private readonly IAppSettingsHelper _configHelper;
        private readonly IUmbracoTreeTraverser _umbracoTree;

        public SiteSearchService(BaseLuceneSearcher searcher, NameValueCollection entryIndexSets = null,
            IAppSettingsHelper configHelper = null, IUmbracoTreeTraverser umbracoTree = null)
        {
            _searcher = entryIndexSets != null
                ? InitMultiIndexSearcher(searcher, entryIndexSets)
                : searcher;
            _configHelper = configHelper ?? new AppSettingsHelper();
            _umbracoTree = umbracoTree ?? new UmbracoTreeTraverser();
        }

        public IEnumerable<SearchResult> GetRawResults(string searchTerm, bool isFuzzy)
        {
            searchTerm = searchTerm.Trim(); //sanitise input
            var searchCriteria = _searcher.CreateSearchCriteria(BooleanOperation.Or);
            bool isPhrase = searchTerm.Contains(' ');
            string luceneRawQuery;
            if (isFuzzy && !isPhrase)//fuzzy single word
            {
                luceneRawQuery = searchTerm + '~';
            }
            else if (isFuzzy)//fuzzy multiple words
            {
                //More info about grouping https://lucene.apache.org/core/2_9_4/queryparsersyntax.html
                var searchedTerms = searchTerm.Split(' ', ',', '.');
                var luceneString = new StringBuilder();

                for (var i = 0; i < searchedTerms.Length; i++)
                {
                    var word = searchedTerms[i];
                    luceneString.Append(word + "~");
                    if (i != searchedTerms.Length - 1)
                    {
                        luceneString.Append(" AND ");
                    }
                }

                luceneRawQuery = luceneString.ToString();
            }
            else //not fuzzy, exact word or phrase
            {
                luceneRawQuery = "\"" + searchTerm + "\"" + "~1"; //one word proximity/distance
            }

            var query = searchCriteria.RawQuery(luceneRawQuery);
            var results = _searcher.Search(query).OrderByDescending(x => x.Score);
            return isFuzzy ? results.TakeWhile(x => x.Score > 0.05f) : results;
        }

        public IEnumerable<IPublishedContent> ConvertSearchResultToPublishedContentWithTemplate(
            IEnumerable<SearchResult> rawSearchResults, IPublishedContentCache cache, UmbracoContext context)
        {
            //remove duplicated nodes/ids - fixed with hashset
            var nodesWithOutDuplicates = new HashSet<IPublishedContent>();
            var excludedDocTypesAliases = new HashSet<string>(_configHelper.GetValue("SiteSearchExcludedDocTypes").Split(','));

            //I would know that it's under the correct Language because of the parent ID of the IndexSet
            var searchResults = rawSearchResults.ToList();

            foreach (var r in searchResults)
            {
                var content = cache.GetById(context, false, r.Id);
                if (content != null)
                {
                    IPublishedContent nodeToAdd = content;

                    if (content.TemplateId == 0)
                    {

                        nodeToAdd = _umbracoTree.GetFirstParentWithTemplate(content);
                    }

                    if (nodeToAdd != null && !excludedDocTypesAliases.Contains(nodeToAdd.DocumentTypeAlias))
                    {
                        nodesWithOutDuplicates.Add(nodeToAdd);
                    }
                }
            }

            return nodesWithOutDuplicates.ToList();
        }

        public List<SearchResultItem> MapToCustomResults(IEnumerable<IPublishedContent> results)
        {
            var mappedResults = new List<SearchResultItem>();
            const string ellipsis = "...";
            const int maxExcerptLength = 250;
            var headingFields = _configHelper.GetValue("SiteSearchResultsHeadingFields").Split(',').ToList();
            var descriptionFields = _configHelper.GetValue("SiteSearchResultsDescriptionFields").Split(',').ToList();

            foreach (var r in results)
            {
                var headingValue = GetFieldValue(r, headingFields);
                var heading = !string.IsNullOrEmpty(headingValue) ? headingValue : r.Name;

                var descriptionValue = GetFieldValue(r, descriptionFields);
                var description = !string.IsNullOrEmpty(descriptionValue) ? descriptionValue : string.Empty;

                //clean from html tags encodes etc.
                var sanitisedDescription = HttpUtility.HtmlDecode(description.StripHtml());
                if (sanitisedDescription != null && sanitisedDescription.Length > maxExcerptLength)
                {
                    sanitisedDescription = sanitisedDescription.Remove(maxExcerptLength) + " " + ellipsis;
                }

                var url = r.Url;

                var item = new SearchResultItem
                {
                    Heading = heading,
                    Url = url,
                    Description = sanitisedDescription
                };

                mappedResults.Add(item);
            }

            return mappedResults;
        }

        internal BaseLuceneSearcher InitMultiIndexSearcher(BaseLuceneSearcher searcher, NameValueCollection entryIndexSets)
        {
            var searchProvider = new MultiIndexSearcher();
            searchProvider.Initialize(searcher.Name, entryIndexSets);

            return searchProvider;
        }

        private static string GetFieldValue(IPublishedContent result, IEnumerable<string> fields)
        {
            return fields.Where(result.HasValue).Select(result.GetPropertyValue<string>).FirstOrDefault();
        }
    }
}