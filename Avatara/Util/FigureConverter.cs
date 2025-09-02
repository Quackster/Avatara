using System;
using System.IO;
using Newtonsoft.Json.Linq;

namespace Alcosmos.Figure
{
    public class FigureConverter
    {
        // Singleton instance
        private static readonly Lazy<FigureConverter> _instance = new Lazy<FigureConverter>(() => new FigureConverter());

        public static FigureConverter Instance => _instance.Value;

        // Default paths; change as needed
        private readonly string _oldFigureDataPath = "figuredata/converter/oldfiguredata.json";
        private readonly string _newFigureDataPath = "figuredata/converter/newfiguredata.json";

        private static JObject _oldFigureData;
        private static JObject _newFigureData;
        private static readonly object _oldLock = new object();
        private static readonly object _newLock = new object();

        // Private constructor for singleton
        private FigureConverter() { }

        /// <summary>
        /// Converts an old figure format string to the new avatarimage format.
        /// </summary>
        public string ConvertOldToNew(string oldFigure)
        {
            if (string.IsNullOrEmpty(oldFigure) || oldFigure.Length < 22)
                throw new ArgumentException("Invalid figure string", nameof(oldFigure));

            // Parse figure parts
            var partsString = new string[10];
            int start = 0;
            for (int i = 0; i < 10; i++)
            {
                int length = (i == 0 || i == 2 || i == 4 || i == 6 || i == 8) ? 3 : 2;
                partsString[i] = oldFigure.Substring(start, length);
                start += length;
            }

            var parts = Array.ConvertAll(partsString, int.Parse);

            // Assemble new figure string
            string hrColor = ConvertOldColorToNew("hr", parts[0], parts[1]);
            var result =
                $"hr-{parts[0]}-{hrColor}" +
                $".hd-{parts[2]}-{ConvertOldColorToNew("hd", parts[2], parts[3])}" +
                $".ch-{parts[8]}-{ConvertOldColorToNew("ch", parts[8], parts[9])}" +
                $".lg-{parts[4]}-{ConvertOldColorToNew("lg", parts[4], parts[5])}" +
                $".sh-{(parts[6] == 730 ? 3206 : parts[6])}-{ConvertOldColorToNew("sh", parts[6], parts[7])}" +
                TakeCareOfHats(parts[0], int.Parse(hrColor));

            return result;
        }

        private JObject GetOldFigureData()
        {
            if (_oldFigureData == null)
            {
                lock (_oldLock)
                {
                    if (_oldFigureData == null)
                    {
                        string json = File.ReadAllText(_oldFigureDataPath);
                        _oldFigureData = JObject.Parse(json);
                    }
                }
            }
            return _oldFigureData;
        }

        private JObject GetNewFigureData()
        {
            if (_newFigureData == null)
            {
                lock (_newLock)
                {
                    if (_newFigureData == null)
                    {
                        string json = File.ReadAllText(_newFigureDataPath);
                        _newFigureData = JObject.Parse(json);
                    }
                }
            }
            return _newFigureData;
        }

        private string GetOldColorFromFigureList(string part, int sprite, int colorIndex)
        {
            var colorsJson = GetOldFigureData();
            var genders = (JObject)colorsJson["genders"];

            foreach (var gender in genders.Properties())
            {
                foreach (var partType in gender.Value as JArray)
                {
                    var partTypeObj = partType as JObject;
                    if (partTypeObj == null) continue;
                    if (!partTypeObj.ContainsKey(part)) continue;

                    var partArray = partTypeObj[part] as JArray;
                    if (partArray == null) continue;

                    foreach (var dataArray in partArray)
                    {
                        foreach (var dataObj in dataArray as JArray)
                        {
                            var d = dataObj as JObject;
                            if (d == null) continue;

                            if ((int)d["s"] == sprite)
                            {
                                var spriteColorsArray = JArray.Parse(d["c"].ToString());
                                return spriteColorsArray[colorIndex - 1].ToString();
                            }
                        }
                    }
                }
            }
            return null;
        }

        private string ConvertOldColorToNew(string part, int sprite, int colorIndex)
        {
            var oldColor = GetOldColorFromFigureList(part, sprite, colorIndex);
            if (oldColor == null) return null;

            var paletteJson = GetNewFigureData();
            var palette = (JObject)paletteJson["palette"];

            foreach (var paletteEntry in palette.Properties())
            {
                var pal = paletteEntry.Value as JObject;
                foreach (var colorEntry in pal.Properties())
                {
                    var colorObj = colorEntry.Value as JObject;
                    if (colorObj == null) continue;
                    if (colorObj["color"].ToString() == oldColor)
                        return colorEntry.Name;
                }
            }
            return null;
        }

        private string TakeCareOfHats(int spriteId, int colorId)
        {
            switch (spriteId)
            {
                case 120: return ".ha-1001-0";
                case 525:
                case 140: return $".ha-1002-{colorId}";
                case 150:
                case 535: return $".ha-1003-{colorId}";
                case 160:
                case 565: return $".ha-1004-{colorId}";
                case 570: return $".ha-1005-{colorId}";
                case 585:
                case 175: return ".ha-1006-0";
                case 580:
                case 176: return ".ha-1007-0.fa-1202-70";
                case 590:
                case 177: return ".ha-1008-0";
                case 595:
                case 178: return ".ha-1009-1321";
                case 130: return $".ha-1010-{colorId}";
                case 801: return $".hr-829-{colorId}.fa-1201-62.ha-1011-{colorId}";
                case 800:
                case 810: return $".ha-1012-{colorId}";
                case 802:
                case 811: return $".ha-1013-{colorId}";
                default: return $".ha-0-{colorId}";
            }
        }
    }
}
