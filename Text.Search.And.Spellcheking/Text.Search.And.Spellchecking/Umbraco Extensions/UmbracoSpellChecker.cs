using System.Collections.Generic;
using System.Linq;
using Examine.LuceneEngine.Providers;
using Lucene.Net.Index;
using Lucene.Net.Search;
using Lucene.Net.Store;
using SpellChecker.Net.Search.Spell;

namespace Example.BusinessLogic.Umbraco_Extensions
{
    public class UmbracoSpellChecker : IUmbracoSpellChecker
    {
        private readonly SpellChecker.Net.Search.Spell.SpellChecker _checker;
        private readonly IndexReader _indexReader;
        private bool _isIndexed;

        public UmbracoSpellChecker(BaseLuceneSearcher searchProvider)
        {
            var searcher = (IndexSearcher)searchProvider.GetSearcher();
            _indexReader = searcher.GetIndexReader();
            _checker = new SpellChecker.Net.Search.Spell.SpellChecker(new RAMDirectory(), new JaroWinklerDistance());
        }

        public string Check(string value)
        {
            var suggestions = GetTopSuggestions(value, 10);
            return suggestions.FirstOrDefault(); //this will be either the word as it was if correct or the fist with highest metrics
        }

        public List<string> GetTopSuggestions(string value, int numberOfItems)
        {
            EnsureIndexed();
            var suggestionCollection = new List<string>();
            var existing = _indexReader.DocFreq(new Term(SpellCheckerConstants.SpellCheckerKey, value));
            if (existing > 0)// the fist one will be correct of exist
            {
                suggestionCollection.Add(value);
            }

            var suggestions = _checker.SuggestSimilar(value, numberOfItems, null, SpellCheckerConstants.SpellCheckerKey, true);
            var jaro = new JaroWinklerDistance();
            var leven = new LevenshteinDistance();
            var ngram = new NGramDistance();
            var metrics = suggestions.Select(s => new
            {
                word = s,
                freq = _indexReader.DocFreq(new Term(SpellCheckerConstants.SpellCheckerKey, s)),
                jaro = jaro.GetDistance(value, s),
                leven = leven.GetDistance(value, s),
                ngram = ngram.GetDistance(value, s)
            })
            .OrderByDescending(metric => metric.jaro)
            .ThenByDescending(m => m.ngram)
            .ThenByDescending(metric =>
                    (
                        metric.freq / 100f +
                        metric.leven
                    )
                    / 2f
                )
                .ToList();

            var wordsOnly = metrics.Select(m => m.word).ToList();
            suggestionCollection.AddRange(wordsOnly);

            return suggestionCollection;
        }

        private void EnsureIndexed()
        {
            if (!_isIndexed)
            {
                _checker.IndexDictionary(new LuceneDictionary(_indexReader, SpellCheckerConstants.SpellCheckerKey));
                _isIndexed = true;
            }
        }
    }
}
