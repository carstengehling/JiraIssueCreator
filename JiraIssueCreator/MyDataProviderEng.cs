using System.Collections.Generic;
using System.Linq;
using WpfAutoComplete;

namespace JiraIssueCreator
{
    class MyDataProviderEng : ISearchDataProvider
    {
        public WpfAutoComplete.SearchResult DoSearch(string searchTerm)
        {
            return new WpfAutoComplete.SearchResult
            {
                SearchTerm = searchTerm,
                Results = dict.Where(item => item.Value.ToUpperInvariant().Contains(searchTerm.ToUpperInvariant())).ToDictionary(v => v.Key, v => v.Value)                
            };
        }

        public WpfAutoComplete.SearchResult SearchByKey(object Key)
        {
            return new WpfAutoComplete.SearchResult
            {
                SearchTerm = null,
                Results = dict.Where(item => item.Key.ToString()==Key.ToString()).ToDictionary(v => v.Key, v => v.Value)
            };            
         }

        private readonly Dictionary<object, string> dict = new Dictionary<object, string> {
            { 1, "The badger knows something"},
            { 2, "Your head looks something like a pineapple"},
            { 3, "Crazy like a box of green frogs"},
            { 4, "The billiard table has green cloth"},
            { 5, "The sky is blue"},
            { 6, "We're going to need some golf shoes"},
            { 7, "This is going straight to the pool room"},
            { 8, "We're going to  Bonnie Doon"},
            { 9, "Spring forward - Fall back"},
            { 10, "Gerry had a plan which involved telling all"},
            { 11, "When is the summer coming"},
            { 12, "Take you time and tell me what you saw"},
            { 13, "All hands on deck"}
        };
    }
}
