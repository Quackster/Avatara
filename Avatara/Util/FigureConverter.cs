/*

From: https://github.com/Alcosmos/habbo-old-figure-converter

Ported to C# by Quackster

Disassembles a given old Habbo figure String (v17<) and structures it with the v18+ figure format and colors. Useful if you want to use an old figure for Habbo's current avatarimage.

Java port of webbanditten's PHP converter with some corrections.

October 16, 2020 - v1: Initial release. December 21, 2020 - v2 changes: Remaining unsupported clothes added. It should now work with any given pre-v18 figure.

Distributed under Apache License 2.0. Do whatever you want with it, but credit me for the converter. Also credit webbanditten as the creator of the original one.

JSON JAR license is bundled within the JAR itself; don't forget to keep it.
*/
using Newtonsoft.Json.Linq;
using System.Text;

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
            buildFigure.Append(ConvertOldColorToNew("hr", parts[0], parts[1]));
            buildFigure.Append(".hd-");
            buildFigure.Append(parts[2]);
            buildFigure.Append("-");
            buildFigure.Append(ConvertOldColorToNew("hd", parts[2], parts[3]));
            buildFigure.Append(".ch-");
            buildFigure.Append(parts[8]);
            buildFigure.Append("-");
            buildFigure.Append(ConvertOldColorToNew("ch", parts[8], parts[9]));
            buildFigure.Append(".lg-");
            buildFigure.Append(parts[4]);
            buildFigure.Append("-");
            buildFigure.Append(ConvertOldColorToNew("lg", parts[4], parts[5]));
            buildFigure.Append(".sh-");
            buildFigure.Append(parts[6] == 730 ? 3206 : parts[6]);
            buildFigure.Append("-");
            buildFigure.Append(ConvertOldColorToNew("sh", parts[6], parts[7]));
            buildFigure.Append(TakeCareOfHats(parts[0], int.Parse(ConvertOldColorToNew("hr", parts[0], parts[1]))));

            return buildFigure.ToString();
        }

        private static string GetOldColorFromFigureList(string part, int spriteId, int colorIndex)
        {
            string oldFigureData = GetFileAsString("figuredata/converter/oldfiguredata.json");
            if (oldFigureData == null) return null;

            JObject colorsJson = JObject.Parse(oldFigureData);
            var genders = colorsJson["genders"];

            foreach (var gender in genders)
            {
                foreach (var partType in gender.First.Children<JProperty>())
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

        private static string ConvertOldColorToNew(string part, int spriteId, int colorIndex)
        {
            string newFigureData = GetFileAsString("figuredata/converter/newfiguredata.json");
            string oldColor = GetOldColorFromFigureList(part, spriteId, colorIndex);

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
            return spriteID switch
            {
                120 => ".ha-1001-0",
                525 or 140 => $".ha-1002-{colorID}",
                150 or 535 => $".ha-1003-{colorID}",
                160 or 565 => $".ha-1004-{colorID}",
                570 => $".ha-1005-{colorID}",
                585 or 175 => ".ha-1006-0",
                580 or 176 => ".ha-1007-0.fa-1202-70",
                590 or 177 => ".ha-1008-0",
                595 or 178 => ".ha-1009-1321",
                130 => $".ha-1010-{colorID}",
                801 => $".hr-829-{colorID}.fa-1201-62.ha-1011-{colorID}",
                800 or 810 => $".ha-1012-{colorID}",
                802 or 811 => $".ha-1013-{colorID}",
                _ => $".ha-0-{colorID}",
            };
        }

        private static string GetFileAsString(string path)
        {
            try
            {
                return File.ReadAllText(path);
            }
            catch (IOException ex)
            {
                Console.WriteLine(ex.Message);
                return null;
            }
        }
    }
}