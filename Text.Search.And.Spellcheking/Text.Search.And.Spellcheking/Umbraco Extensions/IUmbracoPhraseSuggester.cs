using Text.Search.And.Spellcheking.Models;

namespace Text.Search.And.Spellcheking.Umbraco_Extensions
{
    public interface IUmbracoPhraseSuggester
    {
        RankedPhrase GetTopPhraseAndResults(string keywords);
    }
}