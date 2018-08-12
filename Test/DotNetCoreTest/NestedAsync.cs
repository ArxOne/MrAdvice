#region Mr. Advice

// Mr. Advice
// A simple post build weaving package
// http://mradvice.arxone.com/
// Released under MIT license http://opensource.org/licenses/mit-license.php

#endregion

namespace DotNetCoreTest
{
    using System;
    using System.Net.Http;
    using System.Threading.Tasks;

    [EmptyAsyncAdvice]
    public class NestedAsync
    {
        public void TM1()
        {
        }

        //public async Task TM1Async()
        //{
        //    HttpClient hc = new HttpClient();
        //    var res = await hc.GetAsync("http://mradvice.arxone.com/");
        //    if(res.StatusCode == System.Net.HttpStatusCode.OK)
        //    {
        //        var content = await res.Content.ReadAsStringAsync();
        //    }
        //}

        public async Task<string> TM2Async()
        {
            try
            {
                return await NestedAsync.TM3Async();
            }
            catch (Exception ex)
            {
                //!!!!!!! here will be the exception
                Console.WriteLine(ex.ToString());
                throw ex;
            }
        }

        private static async Task<string> TM3Async()
        {
            var s1 = await NestedAsync.GetFile();
            return "OK";
        }

        private static async Task<string> GetFile()
        {
            //without next 2 lines (when just return String.Empty; - also doesn't work)
            HttpClient hc = new HttpClient();
            var res = await hc.GetAsync("http://mradvice.arxone.com/");
            return String.Empty;
        }
    }
}