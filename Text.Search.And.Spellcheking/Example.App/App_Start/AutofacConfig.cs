using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Reflection;
using System.Web.Http;
using System.Web.Mvc;
using Autofac;
using Autofac.Integration.Mvc;
using Autofac.Integration.WebApi;
using Examine;
using Examine.LuceneEngine.Providers;
using Example.App.Controllers;
using Example.Business.Logic.Services;
using Example.Business.Logic.Umbraco_Extensions;
using Umbraco.Core;
using Umbraco.Core.Services;
using Umbraco.Web;

namespace Example.App
{
    public static class AutofacConfig
    {
        //Call method in app start
        public static void RegisterDependencies(ApplicationContext applicationContext)
        {
            var builder = new ContainerBuilder();

            // Register Umbraco Context
            builder.Register(c => UmbracoContext.Current).InstancePerRequest();

            //Register Umbraco back office API Controllers
            builder.RegisterApiControllers(typeof(UmbracoApplication).Assembly);
            builder.RegisterControllers(typeof(UmbracoApplication).Assembly);

            //Register custom created MVC and API controllers
            //builder.RegisterControllers(typeof(SomeController).Assembly); // your controller here
            //builder.RegisterApiControllers(typeof(SearchController).Assembly); //your controller here

            //Register all classes by default, whose names correspond to naming convension SomeClass to ISomeClass.
            //Note: You only need to register one assembly type, this will register all the classes with interfaces.
            var servicesAssembly = Assembly.GetAssembly(typeof(SpellCheckIndexer));
            builder.RegisterAssemblyTypes(servicesAssembly).AsImplementedInterfaces();

            //Register Umbraco services, override default binding set up above
            builder.RegisterInstance(applicationContext.Services.ContentService).As<IContentService>();
            builder.RegisterInstance(applicationContext.Services.LocalizationService).As<ILocalizationService>();
            builder.RegisterInstance(applicationContext.Services.DataTypeService).As<IDataTypeService>();
            builder.RegisterInstance(applicationContext.Services.FileService).As<IFileService>();
            builder.RegisterInstance(applicationContext.Services.MediaService).As<IMediaService>();
            builder.RegisterInstance(applicationContext.Services.MemberService).As<IMemberService>();
            builder.RegisterInstance(applicationContext.Services.MemberTypeService).As<IMemberTypeService>();

            //Register UmbracoHelper and content query
            builder.Register(x => new UmbracoHelper(UmbracoContext.Current)).InstancePerRequest();
            builder.RegisterInstance(new UmbracoHelper(UmbracoContext.Current).ContentQuery).As<ITypedPublishedContentQuery>();

            //TODO: add all controllers and searcher here
            RegiterSpellChekersAndSearcherPerSearchController(builder);

            // Set up MVC to use Autofac as a dependency resolver
            var container = builder.Build();
            var resolver = new AutofacWebApiDependencyResolver(container);
            GlobalConfiguration.Configuration.DependencyResolver = resolver;
            DependencyResolver.SetResolver(new AutofacDependencyResolver(container));
        }

        public static void RegiterSpellChekersAndSearcherPerSearchController(ContainerBuilder builder)
        {
            ExamineManager.Instance.IndexProviderCollection.ToList().ForEach(index => index.RebuildIndex());
            var indexSets = "indexSets";

            //TODO: add all controllers and searcher here
            var searcherAndIndexersForControllers = new List<SearchersAndIndexersPerSearchController>()
            {
                new SearchersAndIndexersPerSearchController()
                {
                    SpellCheckerSearcherName = "SpellCheckSearcherEN",
                    SearcherName = "SiteSearchSearcherUK",
                    IndexSets = new NameValueCollection
                    {
                        { indexSets, "SiteSearchIndexSetEN" },// you need to create these set in ExamineSettings.config & ExamineIndex.config
                        { indexSets, "SiteSearchIndexSetSiteResources" },
                        { indexSets, "SiteSearcIndexSetMicrosites"},
                        { indexSets, "SiteSearchIndexSetComplienceEN"},
                        { indexSets, "SiteSearchIndexSetDefaultFAQsEN"},
                        { indexSets, "SiteSearchIndexSetMexicoFAQsEN"},
                        { indexSets, "SiteSearchIndexSetIndiaFAQsEN"},
                        { indexSets, "SiteSearchIndexSetBrazilFAQsEN"}
                    },
                    ControllerType = typeof(EnSiteSearchSurfaceController)//you EN controller here
                },

                new SearchersAndIndexersPerSearchController()
                {
                    SpellCheckerSearcherName = "SpellCheckSearcherDE",
                    SearcherName = "SiteSearchSearcherDE",
                    IndexSets = new NameValueCollection
                    {
                        { indexSets, "SiteSearchIndexSetDE" },
                        { indexSets, "SiteSearchIndexSetComplienceDE"},
                        { indexSets, "SiteSearchIndexSetDefaultFAQsDE"},
                    },
                    ControllerType = typeof(DeSiteSearchSurfaceController)//your DE controller here
                },
               //list other here
            };

            foreach (var entry in searcherAndIndexersForControllers)
            {

                builder.RegisterType(entry.ControllerType).WithParameter("spellChecker",
                    new UmbracoSpellChecker(
                        (BaseLuceneSearcher)ExamineManager.Instance.SearchProviderCollection[entry.SpellCheckerSearcherName]))
                            .WithParameter("siteSearchService",
                            new SiteSearchService((BaseLuceneSearcher)ExamineManager.Instance.SearchProviderCollection[entry.SearcherName], entry.IndexSets))
                            .InstancePerRequest();
            }
        }

        private class SearchersAndIndexersPerSearchController
        {
            internal string SpellCheckerSearcherName { get; set; }

            internal string SearcherName { get; set; }

            internal NameValueCollection IndexSets { get; set; }

            internal System.Type ControllerType { get; set; }
        }
    }
}