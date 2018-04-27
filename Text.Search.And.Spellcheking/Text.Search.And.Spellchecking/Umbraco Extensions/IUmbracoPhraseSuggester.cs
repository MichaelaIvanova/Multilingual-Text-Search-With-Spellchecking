using Text.Search.And.Spellchecking.Models;

namespace Text.Search.And.Spellchecking.Umbraco_Extensions
{
    public interface IUmbracoPhraseSuggester
    {
        RankedPhrase GetTopPhraseAndResults(string keywords);
    }
}