using System.Collections.Generic;
using System;

namespace WpfAutoComplete
{
    public struct SearchResult
    {
        public string SearchTerm { get; set; }
        public Dictionary<object, string> Results { get; set; }
    }
    
    public interface ISearchDataProvider
    {
        SearchResult DoSearch(string searchTerm);
        SearchResult SearchByKey(object Key);
    }
}
