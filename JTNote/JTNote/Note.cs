using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Data.Entity.Spatial;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.RegularExpressions;

/*
    using System;
    using System.Collections.Generic;
*/

namespace JTNote
{
    public partial class Note
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public Note()
        {
            SharedNotes = new HashSet<SharedNote>();
            Tags = new HashSet<Tag>();
        }

        public Note(int id, int userId, string title, string content, Notebook notebook, bool isDeleted, DateTime lastUpdatedDate)
        {
            if (title == null || title == "")
                throw new ArgumentException("Error loading data: Title must contain text."); // Title cannot be blank, there is an error if so

            Id = id;
            UserId = userId;
            Title = title;
            Content = content;
            Notebook = notebook;
            IsDeleted = (byte)(isDeleted == true ? 1 : 0);
            LastUpdatedDate = lastUpdatedDate;
        }

        public int Id { get; set; }

        [NotMapped]
        public string ContentPlaintext
        {
            get
            {
                string outputString = Content;
                outputString = Regex.Replace(outputString, @"^\{(.+)|^\\(.+)|(\}*)", "");
                return outputString;
            }
            set { }
        }

        [NotMapped]
        public string ContentTruncated
        {
            get
            {
                string outputContent = ContentPlaintext;
                if (outputContent.Length > 100)
                    return outputContent.Substring(0, 100) + "...";
                else
                    return outputContent;
            }
            set { }
        }

        [NotMapped]
        public string TagsString
        {
            get
            {
                if (Tags != null && Tags.Count > 0)
                    return string.Join(", ", Tags.ToList());
                else
                    return "No tags added.";
            }
            private set { }
        }

        public int UserId { get; set; }

        [Required]
        [StringLength(128)]
        public string Title { get; set; }

        public int? NotebookId { get; set; }

        public string Content { get; set; }

        public byte IsDeleted { get; set; }

        public DateTime LastUpdatedDate { get; set; }

        public virtual Notebook Notebook { get; set; }

        public virtual User User { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<SharedNote> SharedNotes { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Tag> Tags { get; set; }
    }
}
