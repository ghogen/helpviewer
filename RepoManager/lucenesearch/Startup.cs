using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace LuceneSearch
{
    public static class Startup
    {

        public static async Task<object> Invoke(object input)
        {
            return await Task.Run(() => DocSearcher.Search((string)input));
        }
    }
}
