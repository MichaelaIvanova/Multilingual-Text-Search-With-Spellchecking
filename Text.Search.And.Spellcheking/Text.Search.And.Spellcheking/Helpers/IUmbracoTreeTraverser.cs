using Umbraco.Core.Models;

namespace Text.Search.And.Spellcheking.Helpers
{
    public interface IUmbracoTreeTraverser
    {
        IPublishedContent GetCurrentLanguageRootNode(IPublishedContent node);
        IPublishedContent GetFirstParentWithTemplate(IPublishedContent content);
    }
}