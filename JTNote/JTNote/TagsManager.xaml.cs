using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace JTNote
{
    /// <summary>
    /// Interaction logic for TagsManager.xaml
    /// </summary>
    public partial class TagsManager : Window
    {
        // Create lists to hold tags that will be added to note, and available tags (list of all tags not currently added)
        public List<Tag> AddedTags = new List<Tag>();
        public List<Tag> AvailableTags = new List<Tag>();
        public TagsManager(NoteEdit parent)
        {
            InitializeComponent();
            Owner = parent;


            // Load list of all tags belonging to user
            LoadAllTags();
            RefreshBoxes();
        }

        private void RefreshBoxes()
        {
            // Sort data sources
            AddedTags.OrderBy(tag => tag);
            AvailableTags.OrderBy(tag => tag);

            // Clear items sources by setting to null, then set them back and refresh
            lbAddedTags.ItemsSource = null;
            lbAddedTags.ItemsSource = AddedTags;
            lbAvailableTags.ItemsSource = null;
            lbAvailableTags.ItemsSource = AvailableTags;
            lbAddedTags.Items.Refresh();
            lbAvailableTags.Items.Refresh();
        }

        private void LoadAllTags()
        {
            Globals.LoginUser.Tags
                .Where(tag => !AddedTags.Contains(tag)).ToList()
                .ForEach(tag => AvailableTags.Add(tag));
        }

        private void BtnAdd_Click(object sender, RoutedEventArgs e)
        {
            if (lbAvailableTags.SelectedItems != null)
            {
                // Build list of tags to add if selection is not null
                List<Tag> tagsToAdd = new List<Tag>();

                // Convert from IList into List
                foreach (Tag tag in lbAvailableTags.SelectedItems)
                    tagsToAdd.Add(tag);

                // Update added and available tags lists depending on selection
                foreach(Tag tag in tagsToAdd)
                {
                    AddedTags.Add(tag);
                    AvailableTags.Remove(tag);
                    RefreshBoxes();
                }
            }
        }

        private void BtnRemove_Click(object sender, RoutedEventArgs e)
        {
            if (lbAddedTags.SelectedItems != null)
            {
                // Build list of tags to remove if selection is not null
                List<Tag> tagsToRemove = new List<Tag>();

                // Convert from IList into List
                foreach (Tag tag in lbAddedTags.SelectedItems)
                    tagsToRemove.Add(tag);

                // Update added and available tags lists depending on selection
                foreach (Tag tag in tagsToRemove)
                {
                    AvailableTags.Add(tag);
                    AddedTags.Remove(tag);
                    RefreshBoxes();
                }
            }
        }

        private void BtnNew_Click(object sender, RoutedEventArgs e)
        {
            // Show dialog to create new tag
            TagNotebookDialog tagDialog = new TagNotebookDialog(this, ETagNotebookDlgType.CreateTag);
            if (tagDialog.ShowDialog() == true)
            {
                // Reload all tags after new tag created
                AvailableTags.Clear();
                LoadAllTags();

                // Add last item in list, which will be the new tag, directly to added tags and delete from available tags
                AddedTags.Add(AvailableTags[AvailableTags.Count - 1]);
                AvailableTags.Remove(AvailableTags[AvailableTags.Count - 1]);
                RefreshBoxes();
            }
        }

        private void BtnOk_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }
    }
}
