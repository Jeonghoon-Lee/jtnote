namespace JTNote
{
    using System;
    using System.Data.Entity;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Linq;

    public partial class JTNoteContext : DbContext
    {
        public JTNoteContext()
            : base("name=JTNoteDBEntities")
        {
        }

        public virtual DbSet<Notebook> Notebooks { get; set; }
        public virtual DbSet<Note> Notes { get; set; }
        public virtual DbSet<SharedNote> SharedNotes { get; set; }
        public virtual DbSet<Tag> Tags { get; set; }
        public virtual DbSet<User> Users { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Note>()
                .HasMany(e => e.SharedNotes)
                .WithRequired(e => e.Note)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Note>()
                .HasMany(e => e.Tags)
                .WithMany(e => e.Notes)
                .Map(m => m.ToTable("NoteTag").MapLeftKey("NoteId").MapRightKey("TagId"));

            modelBuilder.Entity<User>()
                .Property(e => e.Password)
                .IsFixedLength();

            modelBuilder.Entity<User>()
                .HasMany(e => e.Tags)
                .WithRequired(e => e.User)
                .WillCascadeOnDelete(false);
        }
    }
}
