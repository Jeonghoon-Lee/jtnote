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
        public int Id { get; set; }
        public int UserId { get; set; }
        public string Title { get; set; }
        public string Content { get; set; } // TODO: May have to change typing as this will be XML for RTB?
        public int? NotebookId { get; set; } = null;
        public bool IsDeleted { get; set; } = false;
        public DateTime LastUpdatedDate { get; set; } = DateTime.Today;

        public Note(int id, int userId, string title, string content, int? notebookId, bool isDeleted, DateTime lastUpdatedDate)
        {
            if (title == null || title == "")
                throw new ArgumentException("Error loading data: Title must contain text."); // Title cannot be blank, there is an error if so

            Id = id;
            Title = title;
            Content = content;
            NotebookId = notebookId;
            IsDeleted = isDeleted;
            LastUpdatedDate = lastUpdatedDate;
        }

        public void ReloadNote()
        {
            Note updatedInfo = Globals.Db.GetNoteById(Id);

            Title = updatedInfo.Title;
            Content = updatedInfo.Content;
            NotebookId = updatedInfo.NotebookId;
            IsDeleted = updatedInfo.IsDeleted;
            LastUpdatedDate = updatedInfo.LastUpdatedDate;
        }

        // TODO: Testing listview with this, delete later
        public override string ToString()
        {
            return "Test value, title: " + Title;
        }
    }
}
