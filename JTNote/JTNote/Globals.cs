using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JTNote
{
    class Globals
    {
        public static User LoginUser;

        public static List<TagsOnUser> TagListView = new List<TagsOnUser>();


        /*
                public static Database Db;

                public static User LoginUser;
                public static List<Tag> TagList = null;

                public static void ReloadTagList()
                {
                    if (TagList == null)
                    {
                        // initialize
                        TagList = Db.GetTagsByUserId(LoginUser.Id);
                    }
                    else
                    {
                        TagList.Clear();
                        TagList.AddRange(Db.GetTagsByUserId(LoginUser.Id));
                    }
                }
        */
    }
}
