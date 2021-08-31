using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UpdateZipmodManifest
{
    class Program
    {
        const string MANIFEST_FILE_NAME = "manifest.xml";
#if DEBUG
        const string PATH = @"D:\KoikatsuSunshine\mods";
#else
        const string PATH = @".\\mods";
#endif


        static void Main(string[] args)
        {
            Console.WriteLine($"置換対象フォルダ {Path.GetFullPath(PATH)}");
            Console.WriteLine("Enterキーで置換開始 (Ctrl+Cで中断)");
            Console.ReadLine();
            Console.WriteLine("置換開始");
            try
            {
                var filePaths = Directory.GetFiles(PATH, "*.zipmod", SearchOption.AllDirectories);
                foreach (var path in filePaths)
                {
                    using (var archive = ZipFile.Open(path, ZipArchiveMode.Update))
                    {
                        var entry = archive.Entries.FirstOrDefault((e) => e.FullName.Equals(MANIFEST_FILE_NAME));
                        if (entry != null)
                        {
                            string original;
                            using (var sr = new StreamReader(entry.Open(), Encoding.UTF8))
                            {
                                original = sr.ReadToEnd();
                            }

                            var replaced = original.Replace("<game>Koikatsu</game>", "<game>Koikatsu Sunshine</game>");

                            if (original != replaced)
                            {
                                entry.Delete();

                                var newEntry = archive.CreateEntry(MANIFEST_FILE_NAME);
                                using (var sw = new StreamWriter(newEntry.Open(), Encoding.UTF8))
                                {
                                    sw.Write(replaced);
                                }

                                Console.WriteLine($"Replaced {path}");
                            }
                        }
                    }
                }
                Console.WriteLine("置換完了");
                Console.WriteLine("");
                Console.WriteLine("終了するにはキーを押してください...");
                Console.Read();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }
    }
}
