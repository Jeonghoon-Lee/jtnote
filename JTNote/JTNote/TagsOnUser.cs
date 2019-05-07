using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JTNote
{
    public class TagsOnUser
    {
        public TagsOnUser()
        {
            TagList = new ObservableCollection<Tag>();
            Title = "Tags";
        }
        public string Title { get; set; }

        public ObservableCollection<Tag> TagList { get; set; }
    }
}
