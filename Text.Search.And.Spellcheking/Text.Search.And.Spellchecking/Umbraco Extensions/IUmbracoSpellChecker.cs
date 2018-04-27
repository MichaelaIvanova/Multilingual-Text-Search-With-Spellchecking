using System.Collections.Generic;

namespace Text.Search.And.Spellchecking.Umbraco_Extensions
{
    public interface IUmbracoSpellChecker
    {
        string Check(string value);

        List<string> GetTopSuggestions(string value, int numberOfItems);
    }
}