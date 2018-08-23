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
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace OfflineViewer5
{
    /// <summary>
    /// Interaction logic for Search.xaml
    /// </summary>
    public partial class Search : Page
    {

        private void Search_filter_list_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            int x = 0;
        }

        public class SearchFilter : INotifyPropertyChanged
        {
            public SearchFilter(string name)
            {
                Name = name;
            }

            bool _checked = false;

            public event PropertyChangedEventHandler PropertyChanged;

            // This method is called by the Set accessor of each property.  
            // The CallerMemberName attribute that is applied to the optional propertyName  
            // parameter causes the property name of the caller to be substituted as an argument.  
            private void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }

            public string Name { get; set; }
            public bool Checked
            {
                get
                {
                    return _checked;
                }

                set 
                {
                    _checked = value;
                    NotifyPropertyChanged();
                }
            }
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
            search_filter_list.CollectionChanged += Search_filter_list_CollectionChanged;
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
            foreach (var v in search_filter_list)
            {
                v.Checked = false;
            }

        }

        private void btnSelectAll_Click(object sender, RoutedEventArgs e)
        {
            foreach (var v in search_filter_list)
            {
                v.Checked = true;
            }
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

