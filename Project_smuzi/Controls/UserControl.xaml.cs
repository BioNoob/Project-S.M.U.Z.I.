using Project_smuzi.Classes;
using Project_smuzi.Models;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Project_smuzi.Controls
{
    /// <summary>
    /// Логика взаимодействия для UserControl.xaml
    /// </summary>
    public partial class UserControl : Window
    {

        public UserControl()
        {
            InitializeComponent();
            SharedModel.CloseEvent += SharedModel_CloseEvent;
            this.Closing += UserControl_Closing;
        }

        private void UserControl_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            this.Hide();
            e.Cancel = true;
        }

        private void SharedModel_CloseEvent()
        {
            this.Closing -= UserControl_Closing;
            this.Close();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            NewUserControl nuc = new NewUserControl()
            {
                Owner = this,
                WindowStartupLocation = WindowStartupLocation.CenterOwner
            };
            nuc.ShowDialog();
        }

        private void Group_trv_Expanded(object sender, RoutedEventArgs e)
        {
            var b = e.OriginalSource as TreeViewItem;
            Group_trv.SelectedItem_ = b.Header;

        }
        public ItemsControl GetSelectedTreeViewItemParent(TreeViewItem item)
        {
            DependencyObject parent = VisualTreeHelper.GetParent(item);
            while (!(parent is TreeViewItem || parent is TreeView))
            {
                parent = VisualTreeHelper.GetParent(parent);
            }
            return parent as ItemsControl;
        }

        private void Group_trv_Selected(object sender, RoutedEventArgs e)
        {
            TreeViewItem item = e.OriginalSource as TreeViewItem;
            if (item != null)
            {
                ItemsControl parent = GetSelectedTreeViewItemParent(item);

                TreeViewItem treeitem = parent as TreeViewItem;
                if (treeitem != null)
                    Group_trv.SelectedItem_ = treeitem.Header;
                //var MyValue = treeitem.Header as NpcSector;//Gets you the immediate parent
            }
        }
    }
}
