using System.Collections.Generic;

namespace Business.Logic.Umbraco_Extensions
{
    public interface IUmbracoSpellChecker
    {
        string Check(string value);

        List<string> GetTopSuggestions(string value, int numberOfItems);
    }
}