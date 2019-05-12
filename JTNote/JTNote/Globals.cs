using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JTNote
{
    class Globals
    {
        public static readonly JTNoteContext Ctx = new JTNoteContext();
        public static User LoginUser;
        public static Note CurrentNote;
    }
}
