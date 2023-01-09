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
    public class FigureExtractor
    {
        public static Dictionary<string, List<FigureDocument>> Parts;

        public static void Parse()
        {
            if (Parts == null)
                Parts = new Dictionary<string, List<FigureDocument>>();


            if (Parts.Count == 0 && Directory.GetDirectories("figuredata/compiled").Length > 1)
            {
                foreach (var file in Directory.GetFiles("figuredata/compiled"))
                {
                    string fileName = Path.GetFileNameWithoutExtension(file);
                    ParseXML(fileName, file);
                }
            }

            foreach (var file in Directory.GetFiles("figuredata/compiled"))
            {
                string fileName = Path.GetFileNameWithoutExtension(file);

                //if (Directory.Exists(@"furni_export\" + fileName))
                //    return false;


                if (!Directory.Exists(@"figuredata/" + fileName))
                    Directory.CreateDirectory(@"figuredata/" + fileName);

                var flash = new ShockwaveFlash(file);
                flash.Disassemble();

                if (!Directory.Exists(@"figuredata/" + fileName + "/xml"))
                    Directory.CreateDirectory(@"figuredata/" + fileName + "/xml");

                var symbolClass = flash.Tags.Where(t => t.Kind == TagKind.SymbolClass).Cast<SymbolClassTag>().First();
                var imageTags = flash.Tags.Where(t => t.Kind == TagKind.DefineBitsLossless2).Cast<DefineBitsLossless2Tag>();
                var dataTags = flash.Tags.Where(t => t.Kind == TagKind.DefineBinaryData).Cast<DefineBinaryDataTag>();

                foreach (var data in dataTags)
                {
                    var name = symbolClass.Names[symbolClass.Ids.IndexOf(data.Id)];
                    var type = name.Split('_')[name.Split('_').Length - 1];
                    var txt = Encoding.Default.GetString(data.Data);

                    if (!File.Exists(@"figuredata/" + fileName + "/xml/" + type + ".xml"))
                        File.WriteAllText(@"figuredata/" + fileName + "/xml/" + type + ".xml", txt);
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

                    WriteImage(image, @"figuredata/" + fileName + "/" + xmlName + ".png");
                }

                ParseXML(fileName, file);
            }
        }

        private static void ParseXML(string fileName, string file)
        {
            var xmlFile = FileUtil.SolveXmlFile("figuredata/" + fileName + "/xml/", "manifest");
            var list = xmlFile.SelectNodes("//manifest/library/assets/asset");

            for (int i = 0; i < list.Count; i++)
            {
                var asset = list.Item(i);

                if (asset.Attributes.GetNamedItem("name") == null)
                    continue;

                var name = asset.Attributes.GetNamedItem("name").InnerText;

                if (name.Split("_").Length < 3)
                    continue;

                /*if (Parts.ContainsKey(name.Split("_")[2] + (fileName.Contains("_50_") ? "_sh" : "_h")))
                    continue;

                var key = name.Split("_")[2] + (fileName.Contains("_50_") ? "_sh" : "_h");*/

                var key = "";

                if (fileName.StartsWith("hh_ "))
                {
                    key = name.Split("_")[2] + "_" + name.Split("_")[0];
                }
                else
                {
                    key = name.Split("_")[2] + (fileName.Contains("_50_") ? "_sh" : "_h");
                }

                if (!Parts.ContainsKey(key))
                {
                    Parts.Add(key, new List<FigureDocument>());
                }

                Parts[key].Add(new FigureDocument(fileName, xmlFile));
            }
        }

        private static void WriteImage(DefineBitsLossless2Tag image, string path)
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