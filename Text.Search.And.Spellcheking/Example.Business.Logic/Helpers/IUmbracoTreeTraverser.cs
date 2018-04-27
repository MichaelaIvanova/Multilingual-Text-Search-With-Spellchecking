using Umbraco.Core.Models;

namespace Example.Business.Logic.Helpers
{
    public interface IUmbracoTreeTraverser
    {
        IPublishedContent GetCurrentLanguageRootNode(IPublishedContent node);
        IPublishedContent GetFirstParentWithTemplate(IPublishedContent content);
    }
}