using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web.Mvc;
using Examine;
using Example.App.Controllers;
using Example.App.Models;
using Example.BusinessLogic.Helpers;
using Example.BusinessLogic.Models;
using Example.BusinessLogic.Services;
using Example.BusinessLogic.Umbraco_Extensions;
using Example.UnitTesting.Utilities;
using Moq;
using NUnit.Framework;
using Umbraco.Core.Models;
using Umbraco.Web;
using Umbraco.Web.PublishedCache;

namespace Example.App.Tests.Controllers
{
    [TestFixture]
    public class GbSiteSearchSurfaceControllerTests
    {
        private Mock<ISiteSearchService> _siteSearchService;
        private Mock<IUmbracoSpellChecker> _spellChecker;
        private Mock<IUmbracoPhraseSuggester> _phraseSuggester;
        private Mock<IPublishedContentCache> _contentCache;
        private ContextMocker _contextMocker; //REQUIRED
        private EnSiteSearchSurfaceController _controller;
        private IInputSanitiser _inputSanitiser;
        private Mock<IInputSanitiser> _mockedInputSanitiser; //used to verify call of method only
        private const string TopKeywords = "someword1,someword2";
        private const string Keyword = "keyword";
        private readonly List<string> _propertyNames = new List<string> { "SiteSearchHelper", "PhraseSuggester", "ContentCache", "InputSanitiser" };

        [SetUp]
        public void SetUp()
        {
            _siteSearchService = new Mock<ISiteSearchService>();
            _spellChecker = new Mock<IUmbracoSpellChecker>();
            _phraseSuggester = new Mock<IUmbracoPhraseSuggester>();
            _contentCache = new Mock<IPublishedContentCache>();
            _inputSanitiser = new InputSanitiser(); //I will use the real instance since there are no or complex functionality
            _mockedInputSanitiser = new Mock<IInputSanitiser>(); //used to verify call of method only
            _contextMocker = new ContextMocker();
        }

        //Constructor and Properties Tests
        //Basic Constructor properties tests
        [Test]
        public void Should_Init_New_Instances_If_Some_Property_Is_Not_Inited_In_Constructor()
        {
            _controller = GetController();
            var privateBaseTypeProperties = GetPrivateBaseTypeProperties(_controller);

            foreach (var property in privateBaseTypeProperties)
            {
                if (property.Name == "ContentCache")//throws because Context Mocker does not have PublishedCache property mocked and it's almost imposible to create it, 
                                                    //but in the real app running this is fine because we have real UmbracoContext.Current will all internal and private properties stubbed
                {
                    Assert.That(() => property.GetValue(_controller), Throws.Exception.InnerException
                    .TypeOf<InvalidOperationException>()
                        .With.Message.EqualTo("Exception has been thrown by the target of an invocation.")
                    .With.InnerException
                        .Message.EqualTo("Current has not been initialized on Umbraco.Web.PublishedCache.PublishedCachesResolver. You must initialize Current before trying to read it."));
                }
                else
                {
                    var value = property.GetValue(_controller);
                    Assert.IsNotNull(value);
                }
            }
        }

        //Complex Constructor properties tests
        [Test]
        public void Should_Use_Instances_From_Constuctor_If_They_Are_Passed_Instead_Of_Deafult_Ones()
        {
            _controller = GetController(true);
            var privateBaseTypeProperties = GetPrivateBaseTypeProperties(_controller);

            foreach (var property in privateBaseTypeProperties)
            {
                var value = property.GetValue(_controller);

                //here I want to see it's picked up the fake injected instances instead of the default ones
                //and to ensure in the future we can replace and extend just by using the correposding interfaces
                if (property.Name == "PhraseSuggester")
                {
                    Assert.AreSame(_phraseSuggester.Object, value);
                }
                else if (property.Name == "ContentCache")
                {
                    Assert.AreSame(_contentCache.Object, value);
                }
                else if (property.Name == "InputSanitiser")
                {
                    Assert.AreSame(_inputSanitiser, value);
                }

                Assert.IsNotNull(value);
            }
        }

        //Functionality Tests and proper usage of services and helpers Tests
        [Test]
        public void Should_First_Search_For_Exact_Words_Or_Phrase()
        {

            _controller = GetController(true);
            _controller.GetResult(Keyword, null, TopKeywords);

            _siteSearchService.Verify(x => x.GetRawResults(Keyword, false), Times.Once);
        }

        [Test]
        public void Should_Next_Search_For_Close_Matches_For_Single_Word_If_First_Search_Does_Not_Return_Anything()
        {
            var fuzzySearchResults = new List<SearchResult> { new SearchResult { Id = 123 } };
            _siteSearchService.Setup(y => y.GetRawResults(Keyword, false)).Returns(new List<SearchResult>());//return empty list
            _siteSearchService.Setup(y => y.GetRawResults(Keyword, true)).Returns(fuzzySearchResults);

            _controller = GetController(true);
            _controller.GetResult(Keyword, null, TopKeywords);

            _siteSearchService.Verify(x => x.GetRawResults(Keyword, false), Times.Once);
            _siteSearchService.Verify(x => x.GetRawResults(Keyword, true), Times.Once);
        }

        [Test]
        public void Should_Use_Phrase_Suggester_If_Is_Phrase_And_First_Search_Does_Not_Return_Anything_And_Suggested_Phrase_Is_Not_Null()
        {
            const string wrongPhrase = "web from";
            const string correctPhrase = "web form";
            var correctPhraseSearchResults = new List<SearchResult> { new SearchResult { Id = 123 }, new SearchResult { Id = 345 } };
            var topPhrase = new RankedPhrase
            {
                Rank = correctPhraseSearchResults.Count,
                Content = correctPhrase,
                Results = correctPhraseSearchResults
            };

            _siteSearchService.Setup(y => y.GetRawResults(wrongPhrase, false)).Returns(new List<SearchResult>());//return empty list
            _phraseSuggester.Setup(x => x.GetTopPhraseAndResults(wrongPhrase)).Returns(topPhrase);

            _controller = GetController(true);

            var result = _controller.GetResult(wrongPhrase, null, TopKeywords) as PartialViewResult;
            SiteSearchVm vm = (SiteSearchVm)result.Model;

            _phraseSuggester.Verify(x => x.GetTopPhraseAndResults(wrongPhrase), Times.Once);
            Assert.AreEqual(vm.SpellCheckerSuggestionWord, topPhrase.Content);
            Assert.AreEqual(vm.SearchedTerm, wrongPhrase);
        }


        [Test]
        public void Should_Map_Raw_Results_To_IPublishedContent_With_Template_If_Something_Found()
        {
            var searchResults = new List<SearchResult> { new SearchResult { Id = 123 } };
            _siteSearchService.Setup(y => y.GetRawResults(It.IsAny<string>(), It.IsAny<bool>())).Returns(searchResults);

            _controller = GetController(true);
            _controller.GetResult(Keyword, null, TopKeywords);
            _siteSearchService.Verify(x => x.ConvertSearchResultToPublishedContentWithTemplate(searchResults, _contentCache.Object, UmbracoContext.Current), Times.Once);
        }

        [Test]
        public void Should_Map_IPublishedContent_With_Template_To_Custom_Model_If_Something_Found()
        {
            var searchResults = new List<SearchResult> { new SearchResult { Id = 123 } };
            var mappedIpubishedContentResults = new List<IPublishedContent> { new Mock<IPublishedContent>().Object };
            var searchResultItem = new SearchResultItem { Heading = "Heading", Description = "Description" };
            var mappedToCustomResults = new List<SearchResultItem> { searchResultItem };

            _siteSearchService.Setup(y => y.GetRawResults(It.IsAny<string>(), It.IsAny<bool>())).Returns(searchResults);
            _siteSearchService.Setup(x => x.ConvertSearchResultToPublishedContentWithTemplate(searchResults, _contentCache.Object, UmbracoContext.Current))
                .Returns(mappedIpubishedContentResults);
            _siteSearchService.Setup(x => x.MapToCustomResults(mappedIpubishedContentResults))
                .Returns(mappedToCustomResults);

            _controller = GetController(true);

            var result = _controller.GetResult(Keyword, null, TopKeywords) as PartialViewResult;
            SiteSearchVm vm = (SiteSearchVm)result.Model;
            var firstMappedResult = vm.SearcResults.First();

            _siteSearchService.Verify(x => x.MapToCustomResults(mappedIpubishedContentResults), Times.Once);
            Assert.AreEqual(firstMappedResult.Heading, searchResultItem.Heading);
            Assert.AreEqual(firstMappedResult.Description, searchResultItem.Description);
        }

        //Response Models and Partial Viws Tests
        [Test]
        public void Should_Return_Results_And_Suggestions_Partial_If_Something_Found()
        {
            var searchResults = new List<SearchResult> { new SearchResult { Id = 123 } };
            _siteSearchService.Setup(y => y.GetRawResults(It.IsAny<string>(), It.IsAny<bool>())).Returns(searchResults);

            _controller = GetController(true);

            var result = _controller.GetResult(Keyword, null, TopKeywords) as PartialViewResult;

            Assert.IsInstanceOf(typeof(SiteSearchVm), result.Model);
            Assert.AreEqual("_SiteSearchResultsAndSuggestionsPartial", result.ViewName);
        }

        [Test]
        public void Should_Return_No_Results_Partial_With_Listed_Top_Searches_If_Nothing_Found()
        {
            _siteSearchService.Setup(y => y.GetRawResults(It.IsAny<string>(), false)).Returns(new List<SearchResult>());//return empty list
            _siteSearchService.Setup(y => y.GetRawResults(It.IsAny<string>(), true)).Returns(new List<SearchResult>());//return empty list

            _controller = GetController(true);

            var result = _controller.GetResult(Keyword, null, TopKeywords) as PartialViewResult;

            Assert.IsInstanceOf(typeof(NoResultsFoundVm), result.Model);
            Assert.AreEqual("_NoSearchResultsAndSuggestions", result.ViewName);

            NoResultsFoundVm vm = (NoResultsFoundVm)result.Model;
            Assert.AreEqual(vm.Keywords, TopKeywords.Split(',').ToList());
        }

        private EnSiteSearchSurfaceController GetController(bool includePhraseSearch = false)
        {
            if (includePhraseSearch)
            {
                return new EnSiteSearchSurfaceController(_siteSearchService.Object,
                    _spellChecker.Object, _phraseSuggester.Object, _contentCache.Object, _inputSanitiser);
            }

            return new EnSiteSearchSurfaceController(_siteSearchService.Object, _spellChecker.Object);
        }

        private List<PropertyInfo> GetPrivateBaseTypeProperties(EnSiteSearchSurfaceController controller)
        {
            return controller.GetType().BaseType
                .GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                .Where(x => _propertyNames.Contains(x.Name)).ToList();
        }
    }
}