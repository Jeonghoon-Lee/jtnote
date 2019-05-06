using System;
using System.Collections.Generic;
using System.Data.SqlClient;
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
    public enum TagDialogType { Create, Update };
    /// <summary>
    /// Interaction logic for TagDialog.xaml
    /// </summary>
    public partial class TagDialog : Window
    {
        TagDialogType dialogType;
        Tag currentTag;

        public TagDialog(Window owner, TagDialogType type = TagDialogType.Create, Tag tag = null)
        {
            InitializeComponent();

            Owner = owner;

            dialogType = type;
            currentTag = tag;

            switch (dialogType)
            {
                case TagDialogType.Create:
                    Title = "Create tag";
                    btCreateUpdate.Content = "Create";
                    break;
                case TagDialogType.Update:
                    Title = "Update tag";
                    btCreateUpdate.Content = "Update";
                    break;
                default:
                    throw new ArgumentOutOfRangeException("Invalid argument for creating TagDialog");
            }
        }

        private void ButtonCreateUpdate_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string tagName = tbTag.Text;
                using (var ctx = new JTNoteContext())
                {
                    // check if tag is already exist in the list of tags
                    // if (Globals.TagList.Exists(tag => tag.Name == tagName))
                    if (ctx.Tags.Where(tag => tag.Name == tagName).Count() > 0)
                    {
                        string errMessage = string.Format("Tag \"{0}\" already exists.\nPlease enter a different tag name", tagName);
                        MessageBox.Show(errMessage, "JTNotes", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }

                    switch (dialogType)
                    {
                        case TagDialogType.Create:
                            ctx.Tags.Add(new Tag() { Name = tagName, UserId = Globals.LoginUser.Id });
                            break;
                        case TagDialogType.Update:
                            currentTag.Name = tagName;      // update tag name
                            ctx.SaveChanges();
                            break;
                        default:
                            throw new ArgumentOutOfRangeException("Invalid argument for creating TagDialog");
                    }

                }
                /*
                            // check if tag is already exist in the list of tags
                            if (Globals.TagList.Exists(tag => tag.Name == tagName))
                            {
                                string errMessage = string.Format("Tag \"{0}\" already exists.\nPlease enter a different tag name", tagName);
                                MessageBox.Show(errMessage, "JTNotes", MessageBoxButton.OK, MessageBoxImage.Warning);
                                return;
                            }

                            switch (dialogType)
                            {
                                case TagDialogType.Create:
                                    if (Globals.Db.CreateTag(new Tag() { Name = tagName, UserId = Globals.LoginUser.Id }) < 1)
                                    {
                                        MessageBox.Show("[Error]Unable to create new tag.\nPlease try it again", "JTNotes", MessageBoxButton.OK, MessageBoxImage.Error);
                                    }
                                    break;
                                case TagDialogType.Update:
                                    currentTag.Name = tagName;      // update tag name
                                    if (!Globals.Db.UpdateTag(currentTag))
                                    {
                                        MessageBox.Show("[Error]Unable to update tag name.\nPlease try it again", "JTNotes", MessageBoxButton.OK, MessageBoxImage.Error);
                                    }
                                    break;
                                default:
                                    throw new ArgumentOutOfRangeException("Invalid argument for creating TagDialog");
                            }
                */
                DialogResult = true;
            }
            catch (SqlException ex)
            {
                MessageBox.Show(string.Format("Fatal error: unable to create tag.{0}{1}", Environment.NewLine, ex.Message), "JTNote", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void TbTag_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (tbTag.Text == "")
            {
                btCreateUpdate.IsEnabled = false;
                return;
            }
            btCreateUpdate.IsEnabled = true;
        }
    }
}
