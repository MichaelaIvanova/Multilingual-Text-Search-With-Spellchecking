using Business.Logic.Services;
using Business.Logic.Umbraco_Extensions;

namespace Example.App.Config.Controllers
{
    public class DeSiteSearchSurfaceController : BaseSiteSearchSurfaceController
    {
        public DeSiteSearchSurfaceController(ISiteSearchService siteSearchService, IUmbracoSpellChecker spellChecker) : base(siteSearchService, spellChecker)
        {
        }
    }
}