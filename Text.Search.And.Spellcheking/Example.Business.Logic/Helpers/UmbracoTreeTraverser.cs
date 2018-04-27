using System.Linq;
using Umbraco.Core.Models;
using Umbraco.Web;

namespace Example.Business.Logic.Helpers
{
    //Introduce for unit testing so calling may be verifiable
    //We cannot test the class itselft because static umbraco PublishedContent Extension
    public class UmbracoTreeTraverser : IUmbracoTreeTraverser
    {
        public IPublishedContent GetCurrentLanguageRootNode(IPublishedContent node)
        {
            //TODO:check for the culture
            IPublishedContent rootNode = null;
            if (node != null)
            {
                var nodeMatchingCulture = node.GetCulture().Name;
                //TODO:if we can inject the Umbraco Helper here, we can use XPath to get the content node, which is faster
                rootNode = node.AncestorsOrSelf().FirstOrDefault(x => x.DocumentTypeAlias == "Content")
                    ?.Children
                    .FirstOrDefault(c => c.GetCulture().Name == nodeMatchingCulture);
            }

            return rootNode;
        }


        public IPublishedContent GetFirstParentWithTemplate(IPublishedContent content)
        {
            return content.AncestorsOrSelf().FirstOrDefault(x => x.TemplateId != 0);
        }
    }
}