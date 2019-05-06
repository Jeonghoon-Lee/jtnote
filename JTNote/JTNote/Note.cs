using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JTNote
{
    public class Note
    {
        public int? Id { get; set; }
        public int UserId { get; set; }
        public string Title { get; set; }
        public string Content { get; set; } // TODO: May have to change typing as this will be XML for RTB?
        public int? NotebookId { get; set; } = null;
        public bool IsDeleted { get; set; } = false;
        public DateTime LastUpdatedDate { get; set; } = DateTime.Today;

        public string TruncatedContent
        {
            get
            {
                if (Content.Length > 100)
                    return Content.Substring(0, 100) + "...";
                else
                    return Content;
            }
            private set { }
        }

        public Note(int id, int userId, string title, string content, int? notebookId, bool isDeleted, DateTime lastUpdatedDate)
        {
            if (title == null || title == "")
                throw new ArgumentException("Error loading data: Title must contain text."); // Title cannot be blank, there is an error if so

            Id = id;
            UserId = userId;
            Title = title;
            Content = content;
            NotebookId = notebookId;
            IsDeleted = isDeleted;
            LastUpdatedDate = lastUpdatedDate;
        }

        public void ReloadNote()
        {
            if (Id == null)
                throw new ArgumentException("Cannot reload a new note with no Id!");

            Note updatedInfo = Globals.Db.GetNoteById((int)Id);

            Title = updatedInfo.Title;
            Content = updatedInfo.Content;
            NotebookId = updatedInfo.NotebookId;
            IsDeleted = updatedInfo.IsDeleted;
            LastUpdatedDate = updatedInfo.LastUpdatedDate;
        }

        public void DeleteSelfFromDb()
        {
            Globals.Db.DeleteNote((int)Id);
        }

        public void UpdateSelfInDb()
        {
            Globals.Db.UpdateNote(this);
        }

        // TODO: Testing listview with this, delete later
        public override string ToString()
        {
            return "Test value, title: " + Title;
        }
    }
}
