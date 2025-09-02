using System;
using System.IO;
using System.Text;
using System.Linq;
using Newtonsoft.Json.Linq;

namespace Alcosmos.Figure
{
    public static class FigureConverter
    {
        public static string ConvertOldToNew(string oldFigure)
        {
            int start = 0;
            string[] partsString = new string[10];

            for (int i = 0; i < 10; i++)
            {
                int length = (i == 0 || i == 2 || i == 4 || i == 6 || i == 8) ? 3 : 2;
                partsString[i] = oldFigure.Substring(start, length);
                start += length;
            }

            int[] parts = Array.ConvertAll(partsString, int.Parse);

            var buildFigure = new StringBuilder();

            buildFigure.Append("hr-");
            buildFigure.Append(parts[0]);
            buildFigure.Append("-");
            buildFigure.Append(ConvertOldColorToNew("hr", parts[0], parts[1], GetFileAsString));
            buildFigure.Append(".hd-");
            buildFigure.Append(parts[2]);
            buildFigure.Append("-");
            buildFigure.Append(ConvertOldColorToNew("hd", parts[2], parts[3], GetFileAsString));
            buildFigure.Append(".ch-");
            buildFigure.Append(parts[8]);
            buildFigure.Append("-");
            buildFigure.Append(ConvertOldColorToNew("ch", parts[8], parts[9], GetFileAsString));
            buildFigure.Append(".lg-");
            buildFigure.Append(parts[4]);
            buildFigure.Append("-");
            buildFigure.Append(ConvertOldColorToNew("lg", parts[4], parts[5], GetFileAsString));
            buildFigure.Append(".sh-");
            buildFigure.Append(parts[6] == 730 ? 3206 : parts[6]);
            buildFigure.Append("-");
            buildFigure.Append(ConvertOldColorToNew("sh", parts[6], parts[7], GetFileAsString));
            buildFigure.Append(TakeCareOfHats(parts[0], int.Parse(ConvertOldColorToNew("hr", parts[0], parts[1], GetFileAsString))));

            return buildFigure.ToString();
        }

        private static string GetFileAsString(string path)
        {
            try
            {
                return File.ReadAllText(path);
            }
            catch (IOException) { }

            return null;
        }

        private static string GetOldColorFromFigureList(string part, int spriteId, int colorIndex, Func<string, string> fileReader)
        {
            string oldFigureData = fileReader("figuredata/converter/oldfiguredata.json");
            if (oldFigureData == null) return null;

            JObject colorsJson = JObject.Parse(oldFigureData);
            var genders = colorsJson["genders"];

            foreach (var gender in genders)
            {
                foreach (var partType in gender.Children<JProperty>())
                {
                    if (partType.Name == part)
                    {
                        var partArray = partType.Value as JArray;
                        foreach (var dataArray in partArray)
                        {
                            foreach (var data in dataArray)
                            {
                                int spriteID = data.Value<int?>("s") ?? -1;
                                if (spriteID == spriteId)
                                {
                                    var colors = JArray.Parse(data.Value<string>("c"));
                                    return colors[colorIndex - 1].ToString();
                                }
                            }
                        }
                    }
                }
            }
            return "1";
        }

        private static string ConvertOldColorToNew(string part, int spriteId, int colorIndex, Func<string, string> fileReader)
        {
            string newFigureData = fileReader("figuredata/converter/newfiguredata.json");
            string oldColor = GetOldColorFromFigureList(part, spriteId, colorIndex, fileReader);

            JObject paletteJson = JObject.Parse(newFigureData);
            var palette = paletteJson["palette"];

            foreach (var group in palette)
            {
                foreach (var colorEntry in group.First)
                {
                    var color = colorEntry.First["color"]?.ToString();
                    if (color == oldColor)
                    {
                        return colorEntry.Path.Split('.').Last();
                    }
                }
            }
            return "1";
        }

        private static string TakeCareOfHats(int spriteID, int colorID)
        {
            switch (spriteID)
            {
                case 120:
                    return ".ha-1001-0";
                case 525:
                case 140:
                    return $".ha-1002-{colorID}";
                case 150:
                case 535:
                    return $".ha-1003-{colorID}";
                case 160:
                case 565:
                    return $".ha-1004-{colorID}";
                case 570:
                    return $".ha-1005-{colorID}";
                case 585:
                case 175:
                    return ".ha-1006-0";
                case 580:
                case 176:
                    return ".ha-1007-0.fa-1202-70";
                case 590:
                case 177:
                    return ".ha-1008-0";
                case 595:
                case 178:
                    return ".ha-1009-1321";
                case 130:
                    return $".ha-1010-{colorID}";
                case 801:
                    return $".hr-829-{colorID}.fa-1201-62.ha-1011-{colorID}";
                case 800:
                case 810:
                    return $".ha-1012-{colorID}";
                case 802:
                case 811:
                    return $".ha-1013-{colorID}";
                default:
                    return $".ha-0-{colorID}";
            }
        }
    }
}
