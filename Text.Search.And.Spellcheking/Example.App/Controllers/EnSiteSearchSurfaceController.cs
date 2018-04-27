using Example.Business.Logic.Helpers;
using Example.Business.Logic.Services;
using Example.Business.Logic.Umbraco_Extensions;
using Umbraco.Web.PublishedCache;

namespace Example.App.Controllers
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