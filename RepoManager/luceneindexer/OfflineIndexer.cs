using System;
using System.IO;
using System.Text.RegularExpressions;
using Lucene.Net.Analysis.Standard;
using Lucene.Net.Documents;
using Lucene.Net.Index;
using Lucene.Net.QueryParsers;
using Lucene.Net.Search;
using Lucene.Net.Store;
using Version = Lucene.Net.Util.Version;

namespace LuceneIndexer
{
    /// <summary>
    ///  Creates a new index of all the docs under OfflineHelp2
    /// </summary>
    public class OfflineIndexer
    {
        private static string doc_root = Environment.ExpandEnvironmentVariables(@"%USERPROFILE%\OfflineHelp2\");
        private static string _luceneDir = @"%USERPROFILE%\OfflineHelp2\index";
        static string indexPath = Environment.ExpandEnvironmentVariables(_luceneDir);
        private static FSDirectory _directoryTemp;
        private static FSDirectory _directory
        {
            get
            {
                if (_directoryTemp == null) _directoryTemp = FSDirectory.Open(new DirectoryInfo(indexPath));
                if (IndexWriter.IsLocked(_directoryTemp)) IndexWriter.Unlock(_directoryTemp);
                var lockFilePath = Path.Combine(indexPath, "write.lock");
                if (File.Exists(lockFilePath)) File.Delete(lockFilePath);
                return _directoryTemp;
            }
        }
        private static void _addToLuceneIndex(string filename, IndexWriter writer)
        {
            if (filename.Contains(@"\includes\"))
            {
                return;
            }

            var docsetmatch = Regex.Match(filename, @"OfflineHelp2\\(.*?)\\");
            var docset = docsetmatch.Groups[1].ToString();
            // remove older index entry
            var searchQuery = new TermQuery(new Term("FileName", filename));
            writer.DeleteDocuments(searchQuery);

            // add new index entry
            var doc = new Document();
            string text = File.ReadAllText(filename);
            string lede = "";
            if (!filename.Contains("TOC.md") && text.Length > 4) //ensure next call doesn't throw out of range
            {
                int start = text.IndexOf("---", 4); //skip over first metadata mark and find end of metadata                
                if (start >= 0 && text.Length > start + 3)
                {
                    start = text.IndexOf("\n#", start + 3); //find start of H1 heading, skipping over newline at end of metadata
                    if (start > 0 && text.Length > start + 3)
                    {
                        start = text.IndexOf("\n", start + 3); // move to end of heading
                        string sub = text.Substring(start); //get first para
                        var m = Regex.Match(sub, @"\w.*?\n");
                        if (m.Length > 0)
                        {
                            lede = m.ToString();
                        }
                    }
                }               

               
                var match = Regex.Match(text, @"title: ""?(.*?)( ?\||""|\n)");
                var title = match.Groups[1].ToString();

                // add lucene fields mapped to db fields
                doc.Add(new Field("FileName", filename, Field.Store.YES, Field.Index.NOT_ANALYZED));
                doc.Add(new Field("Lede", lede, Field.Store.YES, Field.Index.NOT_ANALYZED));
                doc.Add(new Field("Content", text, Field.Store.NO, Field.Index.ANALYZED));
                
                Field _t = new Field("Title", title, Field.Store.YES, Field.Index.ANALYZED);
                _t.Boost = 4.0f;
                doc.Add(_t);

                // Boost docset highest. Might not be needed since we have a separate filter on docset field
                Field ds = new Field("DocSet", docset, Field.Store.YES, Field.Index.NOT_ANALYZED);
                ds.Boost = 5.0f;
                doc.Add(ds);
                // add entry to index
                writer.AddDocument(doc);
            }
        }

        public static void AddUpdateLuceneIndex(DirectoryInfo dir)
        {
            // init lucene
            var analyzer = new StandardAnalyzer(Version.LUCENE_30);
            using (var writer = new IndexWriter(_directory, analyzer, IndexWriter.MaxFieldLength.UNLIMITED))
            {
                // add data to lucene search index (replaces older entry if any)
                foreach (var file in dir.EnumerateFiles("*.md", SearchOption.AllDirectories))
                {

                    _addToLuceneIndex(file.FullName, writer);
                }

                // close handles
                analyzer.Close();
                writer.Dispose();
            }
        }

        public static void ClearLuceneIndexRecord(int record_id)
        {
            // init lucene
            var analyzer = new StandardAnalyzer(Version.LUCENE_30);
            using (var writer = new IndexWriter(_directory, analyzer, IndexWriter.MaxFieldLength.UNLIMITED))
            {
                // remove older index entry
                var searchQuery = new TermQuery(new Term("FileName", record_id.ToString()));
                writer.DeleteDocuments(searchQuery);

                // close handles
                analyzer.Close();
                writer.Dispose();
            }
        }

        public static bool ClearLuceneIndex()
        {
            try
            {
                var analyzer = new StandardAnalyzer(Version.LUCENE_30);
                using (var writer = new IndexWriter(_directory, analyzer, true, IndexWriter.MaxFieldLength.UNLIMITED))
                {
                    // remove older index entries
                    writer.DeleteAll();

                    // close handles
                    analyzer.Close();
                    writer.Dispose();
                }
            }
            catch (Exception)
            {
                return false;
            }
            return true;
        }

        public static void Optimize()
        {
            var analyzer = new StandardAnalyzer(Version.LUCENE_30);
            using (var writer = new IndexWriter(_directory, analyzer, IndexWriter.MaxFieldLength.UNLIMITED))
            {
                analyzer.Close();
                writer.Optimize();
                writer.Dispose();
            }
        }
    }
}
