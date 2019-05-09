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
    public enum ETagNotebookDlgType { CreateTag, UpdateTag, CreateNotebook, UpdateNotebook };
    /// <summary>
    /// Interaction logic for TagDialog.xaml
    /// </summary>
    public partial class TagNotebookDialog : Window
    {
        ETagNotebookDlgType dialogType;
        Object currentObj;

        public TagNotebookDialog(Window owner, ETagNotebookDlgType type = ETagNotebookDlgType.CreateTag, Object modifingObj = null)
        {
            InitializeComponent();

            Owner = owner;

            dialogType = type;
            currentObj = modifingObj;

            switch (dialogType)
            {
                case ETagNotebookDlgType.CreateTag:
                    Title = "Create tag";
                    btCreateUpdate.Content = "Create";
                    break;
                case ETagNotebookDlgType.UpdateTag:
                    Title = "Rename tag";
                    btCreateUpdate.Content = "Update";
                    tbTagNotebook.Text = ((Tag)currentObj).Name;
                    break;
                case ETagNotebookDlgType.CreateNotebook:
                    Title = "Create new notebook";
                    btCreateUpdate.Content = "Create";
                    break;
                case ETagNotebookDlgType.UpdateNotebook:
                    Title = "Rename notebook";
                    btCreateUpdate.Content = "Update";
                    tbTagNotebook.Text = ((Notebook)currentObj).Name;
                    break;
                default:
                    throw new ArgumentOutOfRangeException("Invalid argument for creating TagDialog");
            }
        }

        private void ButtonCreateUpdate_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string newName = tbTagNotebook.Text;

                // FIXME: Error handling
                // check if tag is already exist in the list of tags
                // Create string array to compare case-sensitive
                if (dialogType == ETagNotebookDlgType.CreateTag || dialogType == ETagNotebookDlgType.UpdateTag)
                {
                    if (Globals.LoginUser.Tags.Select(tag => tag.Name).ToList().Contains(newName))
                    {
                        string errMessage = string.Format("Tag \"{0}\" already exists.\nPlease enter a different tag name", newName);
                        MessageBox.Show(errMessage, "JTNotes", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }
                }
                else
                {
                    if (Globals.LoginUser.Notebooks.Select(tag => tag.Name).ToList().Contains(newName))
                    {
                        string errMessage = string.Format("Nookbook \"{0}\" already exists.\nPlease enter a different notebook name", newName);
                        MessageBox.Show(errMessage, "JTNotes", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }
                }

                // FIXME: Need to implement error handling
                switch (dialogType)
                {
                    case ETagNotebookDlgType.CreateTag:
                        Globals.Ctx.Tags.Add(new Tag() { Name = newName, UserId = Globals.LoginUser.Id });
                        break;
                    case ETagNotebookDlgType.UpdateTag:
                        ((Tag)currentObj).Name = newName;      // update tag name
                        Globals.Ctx.Tags.Where(tag => tag.Id == ((Tag)currentObj).Id).Single().Name = newName;
                        break;
                    case ETagNotebookDlgType.CreateNotebook:
                        Globals.Ctx.Notebooks.Add(new Notebook() { Name = newName, UserId = Globals.LoginUser.Id });
                        break;
                    case ETagNotebookDlgType.UpdateNotebook:
                        ((Notebook)currentObj).Name = newName;      // update tag name
                        Globals.Ctx.Notebooks.Where(tag => tag.Id == ((Notebook)currentObj).Id).Single().Name = newName;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException("Invalid argument for creating TagDialog");
                }
                Globals.Ctx.SaveChanges();
                DialogResult = true;
            }
            catch (SqlException ex)
            {
                MessageBox.Show(string.Format("Fatal error: unable to create tag.{0}{1}", Environment.NewLine, ex.Message), "JTNote", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void TbTag_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (tbTagNotebook.Text == "")
            {
                btCreateUpdate.IsEnabled = false;
                return;
            }
            btCreateUpdate.IsEnabled = true;
        }
    }
}
