using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace UpdateZipmodManifest
{
    class Program
    {
        const string MANIFEST_FILE_NAME = "manifest.xml";
        const string ROOT_ELEMENT = "manifest";
        const string GAME_ELEMENT = "game";
        const string KK_GAME_NAME = "Koikatsu";
        const string KKS_GAME_NAME = "Koikatsu Sunshine";
#if DEBUG
        const string PATH = @"D:\KoikatsuSunshine\mods";
#else
        const string PATH = @".\\mods";
#endif


        static void Main(string[] args)
        {
            try
            {
                Console.WriteLine($@"UpateZipmodManifest

※注意※
バグや実行途中の中断などでzipmodファイルが使えなくなることがあります。
バックアップを取ってから実行するようにして下さい。

[更新対象フォルダ]
{Path.GetFullPath(PATH)}

[実行オプション]
1) コイカツ用のzipmodを、コイカツサンシャイン用に更新する 

2) コイカツ用のzipmodを、コイカツ・コイカツサンシャイン共用に更新する

3) コイカツサンシャイン用のzipmodを、コイカツ用に更新する 

9) コイカツ用・コイカツサンシャイン用のzipmodを、どのゲームでも利用できるようにする
   ※ゲーム指定が削除されるため、指定を元に戻すことができません

実行したいオプションの数字を入力し、Enterキーを押して下さい。");

                var selection = Console.ReadLine();
                bool targetKK = false;
                bool targetKKS = false;
                bool convertKK = false;
                bool convertKKS = false;
                switch (selection)
                {
                    case "1":
                        targetKK = true;
                        convertKKS = true;
                        break;
                    case "2":
                        targetKK = true;
                        convertKK = true;
                        convertKKS = true;
                        break;
                    case "3":
                        targetKKS = true;
                        convertKK = true;
                        break;
                    case "9":
                        targetKK = true;
                        targetKKS = true;
                        break;
                    default:
                        throw new Exception("範囲外の選択肢です");
                }


                Console.WriteLine("更新開始");
                var filePaths = Directory.GetFiles(PATH, "*.zipmod", SearchOption.AllDirectories);
                foreach (var path in filePaths)
                {
                    var toUpdate = false;
                    using (var archive = ZipFile.OpenRead(path))
                    {
                        var entry = archive.Entries.FirstOrDefault((e) => e.FullName.Equals(MANIFEST_FILE_NAME));
                        if (entry == null)
                        {
                            continue;
                        }

                        var xmlObj = XElement.Load(entry.Open());
                        if (!xmlObj.Name.LocalName.Equals(ROOT_ELEMENT))
                        {
                            continue;
                        }

                        var gameElements = xmlObj.Elements().Where(e => e.Name.LocalName.Equals(GAME_ELEMENT)).ToList();
                        if (gameElements.Count() == 0)
                        {
                            continue;
                        }

                        if (targetKK && gameElements.Exists(ge => ge.Value.Equals(KK_GAME_NAME)))
                        {
                            toUpdate = true;
                        }
                        else if (targetKKS && gameElements.Exists(ge => ge.Value.Equals(KKS_GAME_NAME)))
                        {
                            toUpdate = true;
                        }
                    }
                    if (toUpdate)
                    {
                        using (var archive = ZipFile.Open(path, ZipArchiveMode.Update))
                        {
                            var entry = archive.Entries.FirstOrDefault((e) => e.FullName.Equals(MANIFEST_FILE_NAME));

                            XElement xmlObj;
                            using (var stream = entry.Open())
                            {
                                xmlObj = XElement.Load(stream);
                            }

                            xmlObj.Elements().Where(e => e.Name.LocalName.Equals(GAME_ELEMENT)).ToList().ForEach(gameElement =>
                            {
                                gameElement.Remove();
                            });

                            if (convertKK)
                            {
                                xmlObj.Add(new XElement(GAME_ELEMENT, KK_GAME_NAME));
                            }
                            if (convertKKS)
                            {
                                xmlObj.Add(new XElement(GAME_ELEMENT, KKS_GAME_NAME));
                            }

                            entry.Delete();

                            var newEntry = archive.CreateEntry(MANIFEST_FILE_NAME);
                            using (var stream = newEntry.Open())
                            {
                                xmlObj.Save(stream);
                            }

                            Console.WriteLine($"Updated {path}");
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
                Console.WriteLine(e.Message);
                Console.WriteLine("終了するにはキーを押してください...");
                Console.Read();
            }
        }
    }
}
