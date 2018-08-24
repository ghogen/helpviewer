using LuceneIndexer;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
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

namespace OfflineViewer
{
    public class RepoInfo 
    {
        public string Name { get; set; }

        public string Description { get; set; }
        public string LastUpdated { get; set; }
        public string Url { get; set; }
    }

    /// <summary>
    /// Interaction logic for MainPage.xaml
    /// </summary>
    public partial class MainPage : Page
    {
        private static HttpClient httpClient = new HttpClient();
        public ObservableCollection<RepoInfo> repos_available_for_download = new ObservableCollection<RepoInfo>();
        public static ObservableCollection<RepoInfo> repos_installed;
        ~MainPage()
        {
            httpClient.Dispose();
        }

        public MainPage()
        {
            InitializeComponent();
            // Add an Accept header for JSON format.
            httpClient.DefaultRequestHeaders.Accept.Add(
            new MediaTypeWithQualityHeaderValue("application/json"));
            //GitHub requires a user agent
            httpClient.DefaultRequestHeaders.Add("User-Agent", "mikeblome");

            PopulateDownloadList();
            lv_for_download.ItemsSource = repos_available_for_download;

            repos_installed = new ObservableCollection<RepoInfo>();
            GetInstalledRepos();
            lv_installed_repos.ItemsSource = repos_installed;
        }

        /// <summary>
        ///   Get all the repos from MicrosoftDocs. Since 
        ///  live retrieval via REST is gated without authetication, we can't 
        ///  get all of them at once. hence stored in a local file. Also put the
        ///  common ones on top for easy testing/access.
        /// </summary>
        private void PopulateDownloadList()
        {
            repos_available_for_download.Add(new RepoInfo { Name = "cpp-docs", Description = "Visual C++", LastUpdated = "n/a", Url = "https://github.com/MicrosoftDocs/cpp-docs.git" });
            repos_available_for_download.Add(new RepoInfo { Name = "sql-docs", Description = "SQL", LastUpdated = "n/a", Url = "https://github.com/MicrosoftDocs/sql-docs.git" });
            repos_available_for_download.Add(new RepoInfo { Name = "windows-uwp", Description = "Windows UWP", LastUpdated = "n/a", Url = "https://github.com/MicrosoftDocs/windows-uwp.git" });
            repos_available_for_download.Add(new RepoInfo { Name = "dotnet", Description = ".NET", LastUpdated = "n/a", Url = "https://github.com/dotnet/docs.git" });
            repos_available_for_download.Add(new RepoInfo { Name = "windows-desktop-docs", Description = "Windows Desktop", LastUpdated = "n/a", Url = "https://github.com/MicrosoftDocs/windows-desktop-docs.git" });
            repos_available_for_download.Add(new RepoInfo { Name = "windows-desktop-docs-api", Description = "Windows Desktop APIs", LastUpdated = "n/a", Url = "https://github.com/MicrosoftDocs/windows-desktop-docs-api.git" });

            var cached_repos = File.ReadAllLines(@"..\..\msdoc_repos_final.txt");
            foreach (var line in cached_repos)
            {
                var x = line.Split('\t');
                repos_available_for_download.Add(new RepoInfo { Name = x[0], Description = x[1], LastUpdated = x[2], Url = x[3] });
            }
        }

        /// <summary>
        /// Get the repos under local offline help folder OfflineHelp2
        /// </summary>
        private void GetInstalledRepos()
        {
            string repo_root = @"%USERPROFILE%\OfflineHelp2\";
            var filePath = Environment.ExpandEnvironmentVariables(repo_root);
            var di = new System.IO.DirectoryInfo(filePath);
            if (!di.Exists)
            {
                System.IO.Directory.CreateDirectory(filePath);
            }
            var dirs = System.IO.Directory.GetDirectories(filePath, "*.*", System.IO.SearchOption.TopDirectoryOnly);
            foreach (var dir in dirs)
            {
                if (!dir.Contains("\\index")) // skip lucene folder
                {
                    System.IO.DirectoryInfo info = new System.IO.DirectoryInfo(dir);
                    string lastUpdatedFile = System.IO.Path.Combine(dir, @".git/FETCH_HEAD"); //date of this file reflects last pull
                    
                    // We would like to display the repo description but there's no
                    // reliable way to get it from the REST data or from local readme. 
                    // MicrosoftDocs repos are totally inconsistent with their metadata.
                    string readme = System.IO.Path.Combine(dir, @"Readme.md");
                    string description = "";
                    if (File.Exists(readme))
                    {
                        string contents = File.ReadAllText(readme);
                        // Find the level 1 heading
                        var match = Regex.Match(contents, @"\n# (.*?)\r?\n");
                        if (match.Success)
                        {
                            description = match.Groups[1].Value; //who knows what this will be
                        }
                    }
                    FileInfo fi = new FileInfo(lastUpdatedFile);
                    repos_installed.Add(new RepoInfo() { Name = info.Name, Description = description, LastUpdated = fi.CreationTime.ToShortDateString(), Url = "URL tbd" });
                }
            }
        }

        /// <summary>
        ///  Call into GitHub REST APIs to get the repo info.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void GetRepoInfo_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Live syncing temporarily disabled. See source code.");

#if false
            // GitHub gates anonymous requests such that we can't get all MicrosoftDocs repos in real time
            // without setting up authentication for the app. As a workaround we use a local text file 
            // of repos and build the list from that.

            string url = "https://api.github.com/organizations/22479449/repos?type=%22public%22%3Fper_page%3D100";
            // string url = "https://api.github.com/orgs/MicrosoftDocs/repos?type=\"public\"";
            List<string> parts = new List<string>();
            bool canContinue = true;
            do
            {
                string linkHeader = await CreateGetRepoInfoAsync(url);
                if (!linkHeader.Contains("Error status code"))
                {

                    var temp = linkHeader.Split(',');
                    if (temp.Count() > 1)
                    {
                        foreach (var header in temp)
                            if (header.Contains("rel=\"next\""))
                            {
                                var match = Regex.Match(header, "<(.*?)>.*");
                                url = match.Groups[1].ToString();
                                break;
                            }
                    }
                    await Task.Delay(5000);
                }
                else
                {
                    canContinue = false;
                }
            } while (canContinue);
#endif
        }

        /// <summary>
        ///  Process return values from REST calls
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        private async Task<string> CreateGetRepoInfoAsync(string url)
        {
            // MainPage data response.
            HttpResponseMessage response = new HttpResponseMessage();

            try
            {
                response = await httpClient.GetAsync(url);
            }
            catch (HttpRequestException ex)
            {
                return ex.InnerException.Message;
            }
            if (response != null && response.IsSuccessStatusCode)
            {

                // Parse the response body.
                var result = await response.Content.ReadAsStringAsync();
                var objs = JArray.Parse(result);
                StringBuilder sb = new StringBuilder();

                foreach (var o in objs)
                {
                    var x = JObject.FromObject(o);
                    var ri = new RepoInfo();
                    ri.Name = x.GetValue("name").ToString();
                    ri.Description = x.GetValue("description").ToString();
                    ri.LastUpdated = x.GetValue("updated_at").ToString();
                    ri.Url = x.GetValue("clone_url").ToString();
                    repos_available_for_download.Add(ri);
                }

                var vals = response.Headers.GetValues("Link");
                foreach (var v in vals)
                {
                    sb.Append(v);
                }
                return sb.ToString();
            }
            else
            {

                string logFile = Environment.ExpandEnvironmentVariables(@"%USERPROFILE%/documents/git-log.txt");
                StringBuilder sb = new StringBuilder();
                foreach (var repo in repos_available_for_download)
                {
                    sb.AppendFormat("{0}\t{1}\t{2}\t{3}\n", repo.Name, repo.Description, repo.LastUpdated, repo.Url);
                }
                sb.AppendFormat("{0} ({1})\n", (int)response.StatusCode, response.ReasonPhrase);
                File.WriteAllText(logFile, sb.ToString());
                return "Error status code";
            }
        }


        private void CloneRepo_Click(object sender, RoutedEventArgs e)
        {
            List<string> selected_items = new List<string>();
            foreach (var item in lv_for_download.SelectedItems)
            {
                RepoInfo ri = (RepoInfo)item;
                selected_items.Add(ri.Url);
                StartClone(ri);
            }
        }

        /// <summary>
        ///  Use the Git Windows cmd console to clone selected repos.
        /// </summary>
        /// <param name="ri"></param>
        private void StartClone(RepoInfo ri)
        {
            var pathWithEnv = @"%USERPROFILE%\OfflineHelp2\";
            var filePath = Environment.ExpandEnvironmentVariables(pathWithEnv);
            var gitPath = Environment.ExpandEnvironmentVariables(@"C:\Program Files\Git\git-cmd.exe");
            string clone = "git clone " + ri.Url + " " + filePath + ri.Name;
            System.Diagnostics.Process.Start(gitPath, clone);
        }

        /// <summary>
        ///     Delete an installed repo. This does not auto-update the index.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void RemoveRepo_Click(object sender, RoutedEventArgs e)
        {
            var pathWithEnv = @"%USERPROFILE%\OfflineHelp2\";
            var filePath = Environment.ExpandEnvironmentVariables(pathWithEnv);
            List<RepoInfo> repos_to_remove = new List<RepoInfo>();

            foreach (var item in lv_installed_repos.SelectedItems)
            {
                RepoInfo ri = (RepoInfo)item;
                string path = filePath + ri.Name;
                if (Directory.Exists(path))
                {
                    var files = Directory.GetFiles(path, "*.*", SearchOption.AllDirectories);
                    foreach (var file in files)
                    {
                        FileInfo fi = new FileInfo(file);
                        if (fi.IsReadOnly)
                        {
                            File.SetAttributes(file, FileAttributes.Normal);
                        }
                        File.Delete(file);
                    }

                    Directory.Delete(path, true);
                    repos_to_remove.Add(ri); //don't delete while iterating
                }
                else
                {
                    MessageBox.Show("Not found: " + path);
                }
            }

            foreach (var ri in repos_to_remove)
            {
                repos_installed.Remove(ri);
            }
        }

        /// <summary>
        ///     Use the Git windows cmd window to pull from remote.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void UpdateRepo_Click(object sender, RoutedEventArgs e)
        {

            var pathWithEnv = @"%USERPROFILE%\OfflineHelp2\";
            var filePath = Environment.ExpandEnvironmentVariables(pathWithEnv);
            var gitPath = Environment.ExpandEnvironmentVariables(@"C:\Program Files\Git\git-cmd.exe");
            List<string> selected_items = new List<string>();

            foreach (var item in lv_installed_repos.SelectedItems)
            {
                RepoInfo ri = (RepoInfo)item;
                ProcessStartInfo info = new ProcessStartInfo();
                info.FileName = gitPath;
                info.WorkingDirectory = filePath + ri.Name;
                Process proc = new Process();
                if (ri.Name.Contains("sql-docs"))
                {
                    info.Arguments = @"git pull origin live";
                }
                else
                {
                    info.Arguments = @"git pull origin master";
                }
                proc.StartInfo = info;
                proc.Start();
            }
        }

        private void RefreshUI_Click(object sender, RoutedEventArgs e)
        {
            repos_installed.Clear();
            GetInstalledRepos();
        }

        /// <summary>
        ///     Re-create the Lucene index from scratch. Probably not the most
        ///     efficient way to do this. Also Should be async but
        ///     the books are not really usable anyway while this is running.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void UpdateIndex_Click(object sender, RoutedEventArgs e)
        {
            string repo_root = @"%USERPROFILE%\OfflineHelp2\";
            var filePath = Environment.ExpandEnvironmentVariables(repo_root);
            var di = new System.IO.DirectoryInfo(filePath);

            LuceneIndexer.OfflineIndexer.ClearLuceneIndex();
            OfflineIndexer.AddUpdateLuceneIndex(di);
        }

        private void SearchButton_Click(object sender, RoutedEventArgs e)
        {
            var mainWin = (NavigationWindow)Application.Current.MainWindow;
            mainWin.Navigate(new Search());
        }

        private void btnFilter_Click(object sender, RoutedEventArgs e)
        {
            var filtered = (from repo in repos_available_for_download
                            where repo.Name.Contains(tbFilter.Text)
                            select repo).ToList();
            ObservableCollection<RepoInfo> temp = new ObservableCollection<RepoInfo>(filtered);
            lv_for_download.ItemsSource = temp;
        }

        /// <summary>
        /// Filter the list of available repos. For example, enter fr-fr to show
        /// only repos in French.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void tbFilter_KeyDown(object sender, KeyEventArgs e)
        {
            var filtered = (from repo in repos_available_for_download
                            where repo.Name.Contains(tbFilter.Text)
                            select repo).ToList();
            ObservableCollection<RepoInfo> temp = new ObservableCollection<RepoInfo>(filtered);
            lv_for_download.ItemsSource = temp;
        }

        /// <summary>
        /// Necessary to handle case of backspacing back to empty text box.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void tbFilter_KeyUp(object sender, KeyEventArgs e)
        {
            // Handle final backspace that clears text box
            if (tbFilter.Text.Length == 0)
            {
                lv_for_download.ItemsSource = repos_available_for_download;
            }
        }
    }
}
