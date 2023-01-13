using Avatara.Util;
using Flazzy;
using Flazzy.Tags;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;

namespace Avatara
{
    public class FlashExtractor
    {
        #region Fields

        public static readonly FlashExtractor Instance = new FlashExtractor();

        #endregion

        // public static Dictionary<string, FigureAssetEntry> Parts;
        public Dictionary<string, string> Parts;

        public void Load()
        {
            Parts = new Dictionary<string, string>();

            if (Parts == null)
                Parts = new Dictionary<string, string>();

            if (!Directory.Exists(@"figuredata/xml"))
                Directory.CreateDirectory(@"figuredata/xml");

            if (!Directory.Exists(@"figuredata/images"))
                Directory.CreateDirectory(@"figuredata/images");

            if (!Directory.GetFiles("figuredata/xml").Any() || 
                !Directory.GetFiles("figuredata/images").Any())
            {
                foreach (var file in Directory.GetFiles("figuredata/compiled"))
                {
                    string fileName = Path.GetFileNameWithoutExtension(file);

                    //if (Directory.Exists(@"furni_export\" + fileName))
                    //    return false;


                    var flash = new ShockwaveFlash(file);
                    flash.Disassemble();

                    var symbolClass = flash.Tags.Where(t => t.Kind == TagKind.SymbolClass).Cast<SymbolClassTag>().First();
                    var imageTags = flash.Tags.Where(t => t.Kind == TagKind.DefineBitsLossless2).Cast<DefineBitsLossless2Tag>();
                    var dataTags = flash.Tags.Where(t => t.Kind == TagKind.DefineBinaryData).Cast<DefineBinaryDataTag>();

                    foreach (var data in dataTags)
                    {
                        var name = symbolClass.Names[symbolClass.Ids.IndexOf(data.Id)];
                        var type = name.Split('_')[name.Split('_').Length - 1];
                        var txt = Encoding.Default.GetString(data.Data);

                        if (!File.Exists(@"figuredata/xml/" + fileName + ".xml"))
                            File.WriteAllText(@"figuredata/xml/" + fileName + ".xml", txt);
                    }

                    var symbolsImages = new Dictionary<int, DefineBitsLossless2Tag>();

                    foreach (var image in imageTags)
                    {
                        symbolsImages[image.Id] = image;
                    }

                    foreach (var symbol in symbolClass.Names)
                    {
                        //Console.WriteLine(symbolClass.Names.IndexOf(symbol) + " / " + symbol + " / " + symbolClass.Ids[symbolClass.Names.IndexOf(symbol)]);

                        int symbolId = symbolClass.Ids[symbolClass.Names.IndexOf(symbol)];

                        if (!symbolsImages.ContainsKey(symbolId))
                            continue;

                        string name = symbol;

                        var image = symbolsImages[symbolId];
                        var xmlName = name.Substring(fileName.Length + 1);

                        WriteImage(image, @"figuredata/images/" + xmlName + ".png");
                    }
                }
            }

            foreach (var manfiest in Directory.GetFiles("figuredata/xml"))
            {
                ParseXML(manfiest);
            }
        }

        private void ParseXML(string fileName)
        {
            var xmlFile = FileUtil.ReadeXmlFile(fileName);
            var list = xmlFile.SelectNodes("//manifest/library/assets/asset");

            for (int i = 0; i < list.Count; i++)
            {
                var asset = list.Item(i);

                if (asset.Attributes.GetNamedItem("name") == null)
                    continue;

                var name = asset.Attributes.GetNamedItem("name").InnerText;

                if (name.Split("_").Length < 3)
                    continue;

                var offsets = asset.ChildNodes.Item(0)?.Attributes.GetNamedItem("value")?.InnerText;

                /*if (Parts.ContainsKey(name.Split("_")[2] + (fileName.Contains("_50_") ? "_sh" : "_h")))
                    continue;

                var key = name.Split("_")[2] + (fileName.Contains("_50_") ? "_sh" : "_h");*/

                var key = "";

                if (fileName.StartsWith("hh_ "))
                {
                    key = name.Split("_")[2];// + "_" + name.Split("_")[0];
                }
                else
                {
                    key = name.Split("_")[2];// + (fileName.Contains("_50_") ? "_sh" : "_h");
                }

                /*
                if (!Parts.ContainsKey(key))
                {
                    Parts.Add(key, new List<FigureDocument>());
                }*/

                if (name != null && offsets != null && !Parts.ContainsKey(name))
                    Parts.Add(name, offsets);//new FigureDocument(fileName, xmlFile));
            }
        }

        private void WriteImage(DefineBitsLossless2Tag image, string path)
        {
            if (File.Exists(path))
                return;

            System.Drawing.Color[,] table = image.GetARGBMap();

            int width = table.GetLength(0);
            int height = table.GetLength(1);
            using (var payload = new Image<Rgba32>(width, height))
            {
                for (int y = 0; y < height; y++)
                {
                    for (int x = 0; x < width; x++)
                    {
                        System.Drawing.Color pixel = table[x, y];
                        payload[x, y] = new Rgba32(pixel.R, pixel.G, pixel.B, pixel.A);
                    }
                }

                using (var output = new StreamWriter(path))
                {
                    payload.SaveAsPng(output.BaseStream);
                }
            }
        }
    }
}