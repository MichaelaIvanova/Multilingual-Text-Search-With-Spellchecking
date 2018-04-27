using Example.Business.Logic.Models;

namespace Example.Business.Logic.Umbraco_Extensions
{
    public interface IUmbracoPhraseSuggester
    {
        RankedPhrase GetTopPhraseAndResults(string keywords);
    }
}