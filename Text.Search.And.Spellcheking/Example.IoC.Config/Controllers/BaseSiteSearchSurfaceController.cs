using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Example.App.Models;
using Example.BusinessLogic.Helpers;
using Example.BusinessLogic.Models;
using Example.BusinessLogic.Services;
using Example.BusinessLogic.Umbraco_Extensions;
using Umbraco.Web.Mvc;
using Umbraco.Web.PublishedCache;

namespace Example.App.Controllers
{
    public abstract class BaseSiteSearchSurfaceController : SurfaceController
    {
        private readonly ISiteSearchService _siteSearchService;
        private readonly IUmbracoSpellChecker _spellChecker;
        private readonly IUmbracoPhraseSuggester _phraseSuggester;
        private readonly IPublishedContentCache _contentCache;//added for unit testing purposes
        private readonly IInputSanitiser _inputSanitiser;


        //Basic constructor to be used by Autofac
        protected BaseSiteSearchSurfaceController(ISiteSearchService siteSearchService, IUmbracoSpellChecker spellChecker)
        {
            _siteSearchService = siteSearchService;
            _spellChecker = spellChecker;
        }

        //Complex constructor to be used in unit testing to allow mocking of componets
        protected BaseSiteSearchSurfaceController(ISiteSearchService siteSearchService, IUmbracoSpellChecker spellChecker,
            IUmbracoPhraseSuggester phraseSuggester, IPublishedContentCache contentCache, IInputSanitiser inputSanitiser)
        {
            _siteSearchService = siteSearchService;
            _spellChecker = spellChecker;
            _phraseSuggester = phraseSuggester;
            _contentCache = contentCache;
            _inputSanitiser = inputSanitiser;
        }

        //Navigation Properties added for unit testing purposes
        private IUmbracoPhraseSuggester PhraseSuggester => _phraseSuggester ?? new UmbracoPhraseSuggester(_siteSearchService, _spellChecker);
        private IPublishedContentCache ContentCache => _contentCache ?? UmbracoContext.ContentCache.InnerCache;
        private IInputSanitiser InputSanitiser => _inputSanitiser ?? new InputSanitiser();

        public ActionResult GetResult(string keywords, bool? isstrict, string topSearchKeywords)
        {
            bool isStrictQuery = isstrict ?? false;
            //TODO: use Input Sanitizer instead
            var keywordsCollection = InputSanitiser.ExtractWordsFromInput(keywords).ToList(); //sanitise input
            bool isPhrase = keywordsCollection.Count > 1;
            var sanitisedKeywordsAsString = string.Join(" ", keywordsCollection);
            var didYouMean = sanitisedKeywordsAsString;

            //First search exact word
            var results = _siteSearchService.GetRawResults(sanitisedKeywordsAsString, false).ToList();

            //If no reasults and q is not strict - search fuzzy but display the corrected word
            if (!results.Any() && !isStrictQuery && keywordsCollection.Any())
            {
                if (isPhrase)
                {
                    RankedPhrase topPhrase = PhraseSuggester.GetTopPhraseAndResults(sanitisedKeywordsAsString);
                    if (topPhrase != null)
                    {
                        didYouMean = topPhrase.Content;
                        results = topPhrase.Results.ToList();
                    }
                }
                else
                {
                    didYouMean = _spellChecker.Check(sanitisedKeywordsAsString);
                    results = _siteSearchService.GetRawResults(sanitisedKeywordsAsString, true).ToList();
                }

            }

            //Finally if still no results return the partial with top search words suggestions
            if (!results.Any())
            {
                List<string> topSearchedTerms = topSearchKeywords.Split(',').ToList();
                var vm = new NoResultsFoundVm { SearchedTerm = sanitisedKeywordsAsString, Keywords = topSearchedTerms };

                return PartialView("_NoSearchResultsAndSuggestions", vm);
            }
            else
            {
                var contentResults = _siteSearchService.ConvertSearchResultToPublishedContentWithTemplate(results, ContentCache, UmbracoContext);
                var mappedResults = _siteSearchService.MapToCustomResults(contentResults);

                var vm = new SiteSearchVm
                {
                    SearchedTerm = sanitisedKeywordsAsString,
                    SpellCheckerSuggestionWord = didYouMean,
                    SearcResults = mappedResults,
                };

                return PartialView("_SiteSearchResultsAndSuggestionsPartial", vm);
            }
        }
    }
}