using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Avatara.Figure;
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

        private static JObject _oldFigureData;
        private static readonly object _oldLock = new object();

        // Private constructor for singleton
        private FigureConverter() { }

        /// <summary>
        /// Converts an old figure format string to the new avatarimage format.
        /// </summary>
        public string ConvertOldToNew(string oldFigure)
        {
            if (string.IsNullOrEmpty(oldFigure) || oldFigure.Length < 22)
                throw new ArgumentException("Invalid figure string", nameof(oldFigure));

            // Parse figure parts: 5 pairs of (3-digit setId, 2-digit colorIndex)
            // Positions 0-1 are always hr, 2-3 are always hd
            // Positions 4-5, 6-7, 8-9 are ch/lg/sh but order varies by source
            var partsString = new string[10];
            int start = 0;
            for (int i = 0; i < 10; i++)
            {
                int length = (i == 0 || i == 2 || i == 4 || i == 6 || i == 8) ? 3 : 2;
                partsString[i] = oldFigure.Substring(start, length);
                start += length;
            }

            var parts = Array.ConvertAll(partsString, int.Parse);

            // Detect correct mapping for positions 4/6/8 by looking up set types
            int chIdx, lgIdx, shIdx;
            DetectBodyOrder(parts, out chIdx, out lgIdx, out shIdx);

            // Assemble new figure string
            string hrColor = ConvertOldColorToNew("hr", parts[0], parts[1]);
            int shSetId = parts[shIdx] == 730 ? 3206 : parts[shIdx];
            var result =
                $"hr-{parts[0]}-{hrColor}" +
                $".hd-{parts[2]}-{ConvertOldColorToNew("hd", parts[2], parts[3])}" +
                $".ch-{parts[chIdx]}-{ConvertOldColorToNew("ch", parts[chIdx], parts[chIdx + 1])}" +
                $".lg-{parts[lgIdx]}-{ConvertOldColorToNew("lg", parts[lgIdx], parts[lgIdx + 1])}" +
                $".sh-{shSetId}-{ConvertOldColorToNew("sh", parts[shIdx], parts[shIdx + 1])}" +
                TakeCareOfHats(parts[0], int.Parse(hrColor));

            return result;
        }

        private void DetectBodyOrder(int[] parts, out int chIdx, out int lgIdx, out int shIdx)
        {
            // Map each position (4, 6, 8) to its actual part type using figuredata.xml
            var sets = FiguredataReader.Instance.FigureSets;
            var mapping = new Dictionary<string, int>();

            foreach (int idx in new[] { 4, 6, 8 })
            {
                string setId = parts[idx].ToString();
                if (sets.ContainsKey(setId))
                    mapping[sets[setId].SetType] = idx;
            }

            chIdx = mapping.ContainsKey("ch") ? mapping["ch"] : 4;
            lgIdx = mapping.ContainsKey("lg") ? mapping["lg"] : 6;
            shIdx = mapping.ContainsKey("sh") ? mapping["sh"] : 8;
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

            var reader = FiguredataReader.Instance;
            if (!reader.FigureSetTypes.ContainsKey(part)) return null;

            var paletteId = reader.FigureSetTypes[part].PaletteId;
            if (!reader.FigurePalettes.ContainsKey(paletteId)) return null;

            var match = reader.FigurePalettes[paletteId].FirstOrDefault(c => c.HexColor == oldColor);
            return match?.ColourId;
        }

        private string ConvertHrColorToHaColor(int hrColorId)
        {
            var reader = FiguredataReader.Instance;
            if (!reader.FigureSetTypes.ContainsKey("hr") || !reader.FigureSetTypes.ContainsKey("ha"))
                return hrColorId.ToString();

            var hrPaletteId = reader.FigureSetTypes["hr"].PaletteId;
            var haPaletteId = reader.FigureSetTypes["ha"].PaletteId;

            if (hrPaletteId == haPaletteId)
                return hrColorId.ToString();

            if (!reader.FigurePalettes.ContainsKey(hrPaletteId) || !reader.FigurePalettes.ContainsKey(haPaletteId))
                return hrColorId.ToString();

            var hrColor = reader.FigurePalettes[hrPaletteId].FirstOrDefault(c => c.ColourId == hrColorId.ToString());
            if (hrColor == null)
                return hrColorId.ToString();

            var haColor = reader.FigurePalettes[haPaletteId].FirstOrDefault(c => c.HexColor == hrColor.HexColor);
            return haColor?.ColourId ?? hrColorId.ToString();
        }

        private string TakeCareOfHats(int spriteId, int colorId)
        {
            string haColor = ConvertHrColorToHaColor(colorId);

            switch (spriteId)
            {
                case 120: return ".ha-1001-0";
                case 525:
                case 140: return $".ha-1002-{haColor}";
                case 150:
                case 535: return $".ha-1003-{haColor}";
                case 160:
                case 565: return $".ha-1004-{haColor}";
                case 570: return $".ha-1005-{haColor}";
                case 585:
                case 175: return ".ha-1006-0";
                case 580:
                case 176: return ".ha-1007-0.fa-1202-70";
                case 590:
                case 177: return ".ha-1008-0.fa-1202-1294";
                case 595:
                case 178: return ".ha-1009-1321";
                case 130: return $".ha-1010-{haColor}";
                case 801: return $".hr-829-{colorId}.fa-1201-62.ha-1011-{haColor}";
                case 800:
                case 810: return $".ha-1012-{haColor}";
                case 802:
                case 811: return $".ha-1013-{haColor}";
                default: return $".ha-0-{haColor}";
            }
        }
    }
}
