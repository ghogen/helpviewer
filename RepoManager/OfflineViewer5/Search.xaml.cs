using LuceneSearch;
using LuceneIndexer;

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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace OfflineViewer5
{
    /// <summary>
    /// Interaction logic for Search.xaml
    /// </summary>
    public partial class Search : Page
    {
        public Search()
        {
            InitializeComponent();
        }

        private void Go_Click(object sender, RoutedEventArgs e)
        {
            var results = LuceneSearch.DocSearcher.Search(tbSearchTerm.Text);
            lvResults.Items.Clear();
            List<string> docs_to_search = new List<string>();
            if ((bool) cb_cpp.IsChecked)
            {
                docs_to_search.Add("cpp-docs");
            }
            if ((bool)cb_visualstudio.IsChecked)
            {
                docs_to_search.Add("visualstudio-docs");
            }
            if ((bool)cb_dotnet.IsChecked)
            {
                docs_to_search.Add("dotnet");
            }
            if ((bool)cb_sql.IsChecked)
            {
                docs_to_search.Add("sql-docs");
            }

            foreach (var item in results)
            {
                if (docs_to_search.Contains(item.DocSet))
                {
                    lvResults.Items.Add(item);
                }
            }
        }
    }
}
