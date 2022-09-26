namespace UiPathTeam.RulesEngine.Activities.Design.Dialogs
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Globalization;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using System.Windows.Data;
    using System.Windows.Forms;

    public class NodesConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            ObservableCollection<TreeNodeCollection> versions = (ObservableCollection<TreeNodeCollection>)values[0];
            //ObservableCollection<Book> books = (ObservableCollection<Book>)values[1];
            List<object> items = new List<object>();

            //FolderItem folderItemThen = new FolderItem() { Name = "Lieblingsbiere", Items = versions };
            //FolderItem folderItemElse = new FolderItem() { Name = "Lieblingsbücher", Items = books };

            //var nodes = new List<TreeNode>();

            TreeNode versionsNodes = new TreeNode("Versions", versions.OfType<TreeNode>().ToArray());

            //items.Add(folderItemThen);
            items.Add(versionsNodes);

            return items;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException("Cannot be done!");
        }
    }
}
