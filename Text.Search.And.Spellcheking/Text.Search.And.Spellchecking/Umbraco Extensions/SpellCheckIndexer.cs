using System.Collections.Generic;
using System.Globalization;
using System.Web;
using Examine;
using Examine.LuceneEngine.Config;
using Lucene.Net.Documents;
using Lucene.Net.Index;
using Umbraco.Core;
using UmbracoExamine;
using UmbracoExamine.Config;

namespace Business.Logic.Umbraco_Extensions
{
    public class SpellCheckIndexer : BaseUmbracoIndexer
    {
        // May be extended to find words from more types
        protected override IEnumerable<string> SupportedTypes
        {
            get
            {
                yield return IndexTypes.Content;
            }
        }

        protected override void AddDocument(Dictionary<string, string> fields, IndexWriter writer, int nodeId, string type)
        {
            var doc = new Document();
            List<string> cleanValues = new List<string>();
            // This example just cleans HTML, but you could easily clean up json too
            CollectCleanValues(fields, cleanValues);
            var allWords = string.Join(" ", cleanValues);
            // Make sure you don't stem the words. You want the full terms, but no whitespace or punctuation.
            doc.Add(new Field(SpellCheckerConstants.SpellCheckerKey, allWords, Field.Store.NO, Field.Index.ANALYZED));
            writer.UpdateDocument(new Term("__id", nodeId.ToString(CultureInfo.InvariantCulture)), doc);
        }

        protected override IIndexCriteria GetIndexerData(IndexSet indexSet)
        {
            return indexSet.ToIndexCriteria(DataService);
        }

        private void CollectCleanValues(Dictionary<string, string> fields, ICollection<string> cleanValues)
        {
            foreach (var value in fields.Values)
            {
                cleanValues.Add(CleanValue(value));
            } 
        }

        private static string CleanValue(string value)
        {
            return HttpUtility.HtmlDecode(value.StripHtml());
        }
    }
}