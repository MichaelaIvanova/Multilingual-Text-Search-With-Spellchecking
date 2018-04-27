using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Business.Logic.Helpers;
using Business.Logic.Models;
using Business.Logic.Services;

namespace Business.Logic.Umbraco_Extensions
{
    public class UmbracoPhraseSuggester : IUmbracoPhraseSuggester
    {
        private readonly ISiteSearchService _siteSearchService;
        private readonly IUmbracoSpellChecker _spellChecker;
        private readonly IInputSanitiser _inputSanitiser;

        public UmbracoPhraseSuggester(ISiteSearchService siteSearchService, IUmbracoSpellChecker spellChecker)
        {
            _siteSearchService = siteSearchService;
            _spellChecker = spellChecker;
        }

        //Constructor for unit testing
        public UmbracoPhraseSuggester(ISiteSearchService siteSearchService, IUmbracoSpellChecker spellChecker, IInputSanitiser inputSanitiser)
        {
            _siteSearchService = siteSearchService;
            _spellChecker = spellChecker;
            _inputSanitiser = inputSanitiser;
        }

        private IInputSanitiser InputSanitiser => _inputSanitiser ?? new InputSanitiser();

        public RankedPhrase GetTopPhraseAndResults(string keywords)
        {
            var termsInPhrase = InputSanitiser.ExtractWordsFromInput(keywords).Take(3).ToList(); //get up to 3 real words and sanitised
            var termsTop5Candidates = new List<List<string>>();

            foreach (var word in termsInPhrase)
            {
                //if exact match exists it will be included in the top suggestions list as well as the other 5
                var topSpellCheckerSuggestions = _spellChecker.GetTopSuggestions(word, 4);
                var topSpellCheckerSuggestionsStoWordsCleaned = InputSanitiser.GetRealWordsOnly(topSpellCheckerSuggestions);
                termsTop5Candidates.Add(topSpellCheckerSuggestionsStoWordsCleaned);
            }

            //The inutual null is used so that the first cross-join will have something to build on (with strings, null + s = s)
            IEnumerable<string> combinations = new List<string> { null };
            foreach (var list in termsTop5Candidates)
            {
                // cross join the current result with each member of the next list
                combinations = combinations.SelectMany(o => list.Select(s => o + " " + s));
            }

            //for reference: https://stackoverflow.com/questions/12251874/when-to-use-a-parallel-foreach-loop-instead-of-a-regular-foreach
            var phraseseAndResults = new ConcurrentBag<RankedPhrase>();

            Parallel.ForEach(combinations, currentCombination =>
            {
                var trimmedItem = currentCombination.Trim(); //trim white space
                var searchResultsItems = _siteSearchService.GetRawResults(trimmedItem, false).ToList();
                var phrase = new RankedPhrase
                {
                    Rank = searchResultsItems.Count,
                    Results = searchResultsItems,
                    Content = trimmedItem
                };
                phraseseAndResults.Add(phrase);
            });

            return phraseseAndResults.OrderByDescending(x => x.Rank).FirstOrDefault();
        }
    }
}