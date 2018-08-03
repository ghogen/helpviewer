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
using System.Collections.ObjectModel;

namespace OfflineViewer5
{
    /// <summary>
    /// Interaction logic for Search.xaml
    /// </summary>
    public partial class Search : Page
    {

        public class SearchFilter
        {
            public SearchFilter(string name)
            {
                Name = name;
            }
            public string Name { get; set; }
            public bool Checked { get; set; }
        }

        ObservableCollection<SearchFilter> search_filter_list = new ObservableCollection<SearchFilter>();
        List<string> checked_doc_sets = new List<string>();
        public Search()
        {
            InitializeComponent();
            foreach (var v in MainPage.repos_installed)
            {
                search_filter_list.Add(new SearchFilter(v.Name));
            }
            lvFilters.ItemsSource = search_filter_list;

        }

        private void Go_Click(object sender, RoutedEventArgs e)
        {
            SetFilters();
            var results = LuceneSearch.DocSearcher.Search(tbSearchTerm.Text, checked_doc_sets.ToArray());
            checked_doc_sets.Clear();

            lvResults.Items.Clear();
            foreach (var item in results)
            {
                lvResults.Items.Add(item);
            }

        }


        private void SetFilters()
        {

            foreach (var item in search_filter_list)
            {

                if (item.Checked)
                {
                    checked_doc_sets.Add(item.Name);
                }
            }
        }
        private void Page_Loaded(object sender, RoutedEventArgs e)
        {

        }

        private void btnClearAll_Click(object sender, RoutedEventArgs e)
        {

        }

        private void btnSelectAll_Click(object sender, RoutedEventArgs e)
        {

        }



        public static T FindVisualChild<T>(DependencyObject depObj) where T : DependencyObject
        {
            if (depObj != null)
            {
                for (int i = 0; i < VisualTreeHelper.GetChildrenCount(depObj); i++)
                {
                    DependencyObject child = VisualTreeHelper.GetChild(depObj, i);
                    if (child != null && child is T)
                    {
                        return (T)child;
                    }

                    T childItem = FindVisualChild<T>(child);
                    if (childItem != null) return childItem;
                }
            }
            return null;
        }
    }
}

