using System.Collections.Generic;

namespace Text.Search.And.Spellcheking.Helpers
{
    public interface IInputSanitiser
    {
        IEnumerable<string> ExtractWordsFromInput(string input);
        List<string> GetRealWordsOnly(List<string> topSpellCheckerSuggestions);
        HashSet<string> GetStopWords();
    }
}