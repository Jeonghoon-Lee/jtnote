using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JTNote
{
    class Note
    {
        int Id { get; set; }
        int UserId { get; set; }
        string Title { get; set; }
        string Content { get; set; } // May have to change typing as this will be XML for RTB?
        int? NotebookId { get; set; } = null;
        bool IsDeleted { get; set; } = false;
        DateTime LastUpdatedDate { get; set; } = DateTime.Today;

        public Note(int id)
        {
            // Autoload from DB based on given ID (call ReloadNote()), throw new NullReferenceException? if ID doesn't exist
            Id = id;
            ReloadNote();
        }

        public Note()
        {
            // !! Class used for interface testing only using directly entered data !!
        }

        public void ReloadNote()
        {
            // TODO: Reload note data from DB, use for updates etc
        }
    }
}
