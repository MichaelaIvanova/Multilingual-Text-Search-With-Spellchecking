using System.Collections.Generic;
using Business.Logic.Helpers;
using Business.Logic.Services;
using Business.Logic.Umbraco_Extensions;
using Examine;
using Moq;
using NUnit.Framework;

namespace Business.LogicTests.Umbraco_Extensions
{
    [TestFixture]
    public class UmbracoPhraseSuggesterTests
    {
        private Mock<ISiteSearchService> _siteSearchService;
        private Mock<IUmbracoSpellChecker> _spellChecker;
        private Mock<IInputSanitiser> _inputSanitiser;
        private UmbracoPhraseSuggester _phraseSuggester;
        private readonly List<string> _wordsInput = new List<string> { "web", "from", "something" };
        private readonly List<string> _suggestionWithStopWords = new List<string> { "web", "from", "form", "with", "something" };
        private readonly List<string> _realWords = new List<string> { "web", "form", "something" };

        [SetUp]
        public void Init()
        {
            _siteSearchService = new Mock<ISiteSearchService>();
            _spellChecker = new Mock<IUmbracoSpellChecker>();
            _inputSanitiser = new Mock<IInputSanitiser>();
        }

        [Test]
        public void GetTopPhraseAndResults_Should_Sanitise_Keywords_And_Pick_Up_To_3_Words()
        {
            _spellChecker.Setup(x => x.GetTopSuggestions(It.IsAny<string>(), It.IsAny<int>()))
                .Returns(new List<string>());
            _phraseSuggester = new UmbracoPhraseSuggester(_siteSearchService.Object, _spellChecker.Object);

            const string keywords = "    keyword1...,, ,  keyword2, keyword3, keyword4, : ,,,,, ;";
            const string sanitizedFistWord = "keyword1";
            _phraseSuggester.GetTopPhraseAndResults(keywords);

            _spellChecker.Verify(x => x.GetTopSuggestions(It.IsAny<string>(), It.IsAny<int>()), Times.AtMost(3));
            _spellChecker.Verify(x => x.GetTopSuggestions(sanitizedFistWord, It.IsAny<int>()), Times.Once);
        }

        [Test]
        public void GetTopPhraseAndResults_Should_Check_For_Real_Words_Only()
        {
            _inputSanitiser.Setup(x => x.ExtractWordsFromInput(It.IsAny<string>()))
                .Returns(_wordsInput);//need this for the foreach loop to run 3 times
            _inputSanitiser.Setup(x => x.GetRealWordsOnly(_suggestionWithStopWords)).Returns(_realWords);

            _spellChecker.Setup(x => x.GetTopSuggestions(It.IsAny<string>(), It.IsAny<int>()))
                .Returns(_suggestionWithStopWords);

            _phraseSuggester = new UmbracoPhraseSuggester(_siteSearchService.Object, _spellChecker.Object, _inputSanitiser.Object);
            _phraseSuggester.GetTopPhraseAndResults("does not really matter what is here");

            _inputSanitiser.Verify(x => x.GetRealWordsOnly(_suggestionWithStopWords), Times.Exactly(_wordsInput.Count));
        }

        [Test]
        public void GetTopPhraseAndResults_Should_Perform_Ranking_For_Real_Words_Phrases_Only()
        {
            _inputSanitiser.Setup(x => x.ExtractWordsFromInput(It.IsAny<string>()))
                .Returns(_wordsInput);//need this for the foreach loop to run 3 times
            _inputSanitiser.Setup(x => x.GetRealWordsOnly(_suggestionWithStopWords)).Returns(_realWords);

            _spellChecker.Setup(x => x.GetTopSuggestions(It.IsAny<string>(), It.IsAny<int>()))
                .Returns(_suggestionWithStopWords);

            _phraseSuggester = new UmbracoPhraseSuggester(_siteSearchService.Object, _spellChecker.Object, _inputSanitiser.Object);
            _phraseSuggester.GetTopPhraseAndResults("does not really matter what is here");

            var numberOfValidCombination = _realWords.Count * _realWords.Count * _wordsInput.Count; //number of times loop will iterate

            _siteSearchService.Verify(x => x.GetRawResults(It.IsAny<string>(), false), Times.Exactly(numberOfValidCombination));
        }

        [Test]
        public void GetTopPhraseAndResults_Should_Return_The_Phrase_With_The_Highest_Number_Of_Content_Matches()
        {
            _inputSanitiser.Setup(x => x.ExtractWordsFromInput(It.IsAny<string>()))
                .Returns(_wordsInput);//need this for the foreach loop to run 3 times
            _inputSanitiser.Setup(x => x.GetRealWordsOnly(_suggestionWithStopWords)).Returns(_realWords);

            _spellChecker.Setup(x => x.GetTopSuggestions(It.IsAny<string>(), It.IsAny<int>()))
                .Returns(_suggestionWithStopWords);

            //this will the the top phrase because only this will have any content matches
            var searchResults = new List<SearchResult> { new SearchResult(), new SearchResult() };
            const string phraseContent = "something web form";
            _siteSearchService.Setup(x => x.GetRawResults(phraseContent, false)).Returns(searchResults);

            _phraseSuggester = new UmbracoPhraseSuggester(_siteSearchService.Object, _spellChecker.Object, _inputSanitiser.Object);
            var topPhrase = _phraseSuggester.GetTopPhraseAndResults("does not really matter what is here");

            Assert.AreEqual(searchResults.Count, topPhrase.Rank);
            Assert.AreEqual( phraseContent, topPhrase.Content);
        }
    }
}