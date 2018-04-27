namespace Example.Business.Logic.Models
{
    public class SearchResultItem
    {
        // //I will need ranking and sorting
        //Accurate search results, ranked by closest match to keyword(s)

        public int Rank { get; set; }

        public string Heading { get; set; }

        public string Description { get; set; }

        public string Url { get; set; }
    }
}