<<<<<<< HEAD
=======
﻿using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

/*
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;
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

        public Note(int? id, int userId, string title, string content, int? notebookId, bool isDeleted, DateTime lastUpdatedDate)
        {
            if (title == null || title == "")
                throw new ArgumentException("Error loading data: Title must contain text."); // Title cannot be blank, there is an error if so
        }

        public int Id { get; set; }

        public string ContentPlaintext
        {
            get
            {
                // Parse raw XML from Content to plain text
                XmlDocument contentRawXml = new XmlDocument();
                contentRawXml.LoadXml("<?xml version=\"1.0\" encoding=\"UTF - 8\"?><note_body>" + Content + "</note_body>");

                StringBuilder sbOutput = new StringBuilder();
                foreach (XmlNode node in contentRawXml.DocumentElement.ChildNodes)
                {
                    sbOutput.Append(node.InnerText);
                }

                return sbOutput.ToString();
            }
        }

        public string ContentTruncated
        {
            get
            {
                string ptxtContent = ContentPlaintext;
                if (ptxtContent.Length > 100)
                    return ptxtContent.Substring(0, 100) + "...";
                else
                    return ptxtContent;
            }
            private set { }
        }

        public int UserId { get; set; }

        [Required]
        [StringLength(128)]
        public string Title { get; set; }

        public int? NotebookId { get; set; }

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
