using System.Collections.Generic;

namespace Example.Business.Logic.Helpers
{
    public interface IInputSanitiser
    {
        IEnumerable<string> ExtractWordsFromInput(string input);
        List<string> GetRealWordsOnly(List<string> topSpellCheckerSuggestions);
        HashSet<string> GetStopWords();
    }
}