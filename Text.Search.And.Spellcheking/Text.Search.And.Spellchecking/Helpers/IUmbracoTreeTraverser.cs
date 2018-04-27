using Umbraco.Core.Models;

namespace Example.BusinessLogic.Helpers
{
    public interface IUmbracoTreeTraverser
    {
        IPublishedContent GetCurrentLanguageRootNode(IPublishedContent node);
        IPublishedContent GetFirstParentWithTemplate(IPublishedContent content);
    }
}