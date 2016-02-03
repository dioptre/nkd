using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ionic.Zip;
using System.IO;
namespace zip
{
    class Program
    {
        static void Main(string[] args)
        {
            using (ZipFile zip = new ZipFile())
            {
                string[] files = Directory.GetFiles(@".");
                files = (from o in files where !o.Contains(".z") && !o.Contains("zip.exe") select o).ToArray();
                zip.AddFiles(files);
                zip.Comment = "This zip was created at " + System.DateTime.Now.ToString("G");
                zip.MaxOutputSegmentSize = 99 * 1024 * 1024;   // 2mb
                zip.Save("zip.zip");
                //Console.ReadLine();
            }
        }
    }
}
