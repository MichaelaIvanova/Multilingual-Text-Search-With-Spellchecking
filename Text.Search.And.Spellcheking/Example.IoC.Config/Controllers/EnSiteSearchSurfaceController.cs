using Business.Logic.Helpers;
using Business.Logic.Services;
using Business.Logic.Umbraco_Extensions;
using Umbraco.Web.PublishedCache;

namespace Example.App.Config.Controllers
{
    public class EnSiteSearchSurfaceController : BaseSiteSearchSurfaceController
    {
        public EnSiteSearchSurfaceController(ISiteSearchService siteSearchService, IUmbracoSpellChecker spellChecker) : base(siteSearchService, spellChecker)
        {
        }

        public EnSiteSearchSurfaceController(ISiteSearchService siteSearchService, IUmbracoSpellChecker spellChecker, IUmbracoPhraseSuggester phraseSuggester,  IPublishedContentCache contentCache, IInputSanitiser inputSanitiser) : base(siteSearchService, spellChecker, phraseSuggester, contentCache, inputSanitiser)
        {
        }
    }
}