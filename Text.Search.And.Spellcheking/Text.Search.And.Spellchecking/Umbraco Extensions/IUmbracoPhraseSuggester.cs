using Example.BusinessLogic.Models;

namespace Example.BusinessLogic.Umbraco_Extensions
{
    public interface IUmbracoPhraseSuggester
    {
        RankedPhrase GetTopPhraseAndResults(string keywords);
    }
}