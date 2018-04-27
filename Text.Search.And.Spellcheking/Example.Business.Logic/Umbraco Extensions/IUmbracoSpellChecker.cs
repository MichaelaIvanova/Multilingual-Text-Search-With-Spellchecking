using System.Collections.Generic;

namespace Example.Business.Logic.Umbraco_Extensions
{
    public interface IUmbracoSpellChecker
    {
        string Check(string value);

        List<string> GetTopSuggestions(string value, int numberOfItems);
    }
}