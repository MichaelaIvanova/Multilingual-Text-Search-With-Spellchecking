using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Web;
using Newtonsoft.Json;
using Umbraco.Core.Logging;

namespace Text.Search.And.Spellchecking.Helpers
{
    public class InputSanitiser : IInputSanitiser
    {
        private readonly string _pathToJson = "~/App_Data/SiteSearch/stop-words.json";

        //cannot use regex w+ because of multilingual content and unicode chars
        public IEnumerable<string> ExtractWordsFromInput(string input)
        {
            var trimmerSpecialChars = new[] { ' ', ',', '.', ';', '?', '!', '@', '#', '$', '%', '^', '*', '(', ')' };
            var splitterSpecialChars = new[] { ' ', ',', '.', ';' };
            var words = input.Split(splitterSpecialChars, StringSplitOptions.RemoveEmptyEntries);

            return words.Select(x => x.Trim(trimmerSpecialChars));
        }

        public List<string> GetRealWordsOnly(List<string> topSpellCheckerSuggestions)
        {
            var stopWords = GetStopWords();

            return topSpellCheckerSuggestions.Where(x => !stopWords.Contains(x)).ToList();
        }

        public HashSet<string> GetStopWords()
        {
            var items = new HashSet<string>();

            try
            {
                var path = HttpContext.Current.Server.MapPath(_pathToJson); //moved in the try catch block to avoid exception in unit testing
                using (StreamReader r = new StreamReader(path))
                {
                    items = JsonConvert.DeserializeObject<HashSet<string>>(r.ReadToEnd());
                }
            }
            catch (Exception ex)
            {
                LogHelper.Error(MethodBase.GetCurrentMethod().DeclaringType, ex.Message, ex);
            }

            return items;
        }
    }
}