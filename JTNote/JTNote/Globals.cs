using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JTNote
{
    class Globals
    {
        public static Database Db;

        public static User LoginUser;
        public static List<Tag> TagList;

        public static void ReloadTagList()
        {
            TagList.Clear();
            TagList.AddRange(Db.GetTagsByUserId(LoginUser.Id));
        }
    }
}
