using System.Collections.Generic;
using System.Collections.ObjectModel;
using mvc_demo.Services;

namespace mvc_demo.Models.HomeViewModels
{
    public class IndexViewModel
    {
        public IList<Person> People { get; internal set; }
        public IList<string> Values { get; set; }
    }
}