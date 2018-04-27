using Example.Business.Logic.Services;
using Example.Business.Logic.Umbraco_Extensions;

namespace Example.App.Controllers
{
    public class DeSiteSearchSurfaceController : BaseSiteSearchSurfaceController
    {
        public DeSiteSearchSurfaceController(ISiteSearchService siteSearchService, IUmbracoSpellChecker spellChecker) : base(siteSearchService, spellChecker)
        {
        }
    }
}