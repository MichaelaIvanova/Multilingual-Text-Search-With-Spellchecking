using Business.Logic.Models;

namespace Business.Logic.Umbraco_Extensions
{
    public interface IUmbracoPhraseSuggester
    {
        RankedPhrase GetTopPhraseAndResults(string keywords);
    }
}