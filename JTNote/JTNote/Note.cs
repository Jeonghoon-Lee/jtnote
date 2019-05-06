namespace JTNote
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class Note
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public Note()
        {
            SharedNotes = new HashSet<SharedNote>();
            Tags = new HashSet<Tag>();
        }

        public int Id { get; set; }

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
