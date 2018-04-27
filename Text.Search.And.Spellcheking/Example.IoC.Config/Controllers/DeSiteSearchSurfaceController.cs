using Example.BusinessLogic.Services;
using Example.BusinessLogic.Umbraco_Extensions;

namespace Example.App.Controllers
{
    public class DeSiteSearchSurfaceController : BaseSiteSearchSurfaceController
    {
        public DeSiteSearchSurfaceController(ISiteSearchService siteSearchService, IUmbracoSpellChecker spellChecker) : base(siteSearchService, spellChecker)
        {
        }
    }
}