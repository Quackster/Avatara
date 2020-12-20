using Avatara.Extensions;
using Avatara.Figure;
using Avatara.Util;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Avatara
{
    public class Avatar
    {
        public string Figure;
        public bool IsSmall;
        public int BodyDirection;
        public int HeadDirection;
        public FiguredataReader FiguredataReader;
        public string Action = "std"; // stand
        public int Frame;

        public Image<Rgba32> DrawingCanvas;

        public int CANVAS_WIDTH = 500;
        public int CANVAS_HEIGHT = 500;

        public Avatar(string figure, bool isSmall, int bodyDirection, int headDirection, FiguredataReader figuredataReader, string action = "std")
        {
            Figure = figure;
            IsSmall = isSmall;
            BodyDirection = bodyDirection;
            HeadDirection = headDirection;
            FiguredataReader = figuredataReader;
            DrawingCanvas = new Image<Rgba32>(CANVAS_HEIGHT, CANVAS_WIDTH, HexToColor("transparent"));
            Action = action;
        }

        public byte[] Run()
        {
            bool isValid = ValidateFigure();
            var buildQueue = BuildDrawQueue();

            if (buildQueue == null)
                return null;

            return DrawImage(buildQueue);
        }

        private byte[] DrawImage(List<AvatarAsset> buildQueue)
        {
            using (var canvas = this.DrawingCanvas)
            {
                foreach (var asset in buildQueue)
                {
                    var image = SixLabors.ImageSharp.Image.Load<Rgba32>(asset.FileName);

                    if (buildQueue.Count(x => x.Set.HiddenLayers.Contains(asset.Part.Type)) > 0)
                        continue;

                    if (asset.Part.Type != "ey")
                    {
                        if (asset.Part.Colorable)
                        {
                            string[] parts = asset.Parts;

                            if (parts.Length > 2)
                            {
                                var paletteId = int.Parse(parts[2]);

                                if (!FiguredataReader.FigureSetTypes.ContainsKey(parts[0]))
                                    continue;

                                var figureTypeSet = FiguredataReader.FigureSetTypes[parts[0]];
                                var palette = FiguredataReader.FigurePalettes[figureTypeSet.PaletteId];
                                var colourData = palette.FirstOrDefault(x => x.ColourId == parts[2]);

                                if (colourData == null)
                                {
                                    continue;
                                }

                                TintImage(image, colourData.HexColor, 255);

                            }
                        }
                    }
                    else
                    {
                        TintImage(image, "FFFFFF", 255);
                    }

                    var graphicsOptions = new GraphicsOptions();
                    graphicsOptions.ColorBlendingMode = PixelColorBlendingMode.Normal;

                    canvas.Mutate(ctx =>
                    {
                        ctx.DrawImage(image, new SixLabors.ImageSharp.Point(canvas.Width - asset.ImageX, canvas.Height - asset.ImageY), graphicsOptions);
                    });
                }

                using (Bitmap tempBitmap = canvas.ToBitmap())
                {
                    // Crop the image
                    using (Bitmap croppedBitmap = ImageUtil.TrimBitmap(tempBitmap, HexToColor("transparent")))
                    {
                        return RenderImage(croppedBitmap);
                    }
                }
            }
        }

        private byte[] RenderImage(Bitmap croppedBitmap)
        {
        }


        private void TintImage(Image<Rgba32> image, string colourCode, byte alpha)
        {
            var rgb = HexToColor(colourCode);

            for (int x = 0; x < image.Width; x++)
            {
                for (int y = 0; y < image.Height; y++)
                {
                    var current = image[x, y];

                    if (current.A > 0)
                    {
                        current.R = (byte)(rgb.R * current.R / 255);
                        current.G = (byte)(rgb.G * current.G / 255);
                        current.B = (byte)(rgb.B * current.B / 255);
                        current.A = alpha;
                    }

                    image[x, y] = current;
                }
            }
        }

        private List<AvatarAsset> BuildDrawQueue()
        {
            List<AvatarAsset> queue = new List<AvatarAsset>();
            Dictionary<string, string> figureData = new Dictionary<string, string>();


            foreach (string data in Figure.Split("."))
            {
                string[] parts = data.Split("-");
                figureData.Add(parts[0], string.Join("-", parts));

            }

            foreach (string data in figureData.Values)
            {
                string[] parts = data.Split("-");

                if (parts.Length < 2 || parts.Length > 3)
                {
                    return null;
                }

                var setList = FiguredataReader.FigureSets.Values.Where(x => x.Id == parts[1]);

                foreach (var set in setList)
                {
                    var partList = set.FigureParts;

                    foreach (var part in partList)
                    {
                        var t = LoadFigureAsset(parts, part, set);

                        if (t == null)
                            continue;

                        queue.Add(t);

                    }
                }
            }

            queue = queue.OrderBy(x => x.Part.OrderId).ToList();
            return queue;
        }

        private AvatarAsset LoadFigureAsset(string[] parts, FigurePart part, FigureSet set)
        {
            var key = part.Type + (IsSmall ? "_sh" : "_h");
            var document = FigureExtractor.Parts.ContainsKey(key) ? FigureExtractor.Parts[key] : null;

            if (document == null)
                return null;

            var asset = LocateAsset((this.IsSmall ? "sh" : "h") + "_" + Action + "_" + part.Type + "_" + part.Id + "_" + BodyDirection + "_" + Frame, document, parts, part, set);

            if (asset == null)
                asset = LocateAsset((this.IsSmall ? "sh" : "h") + "_" + "std" + "_" + part.Type + "_" + part.Id + "_" + BodyDirection + "_" + Frame, document, parts, part, set); 

            return asset;
        }

        private AvatarAsset LocateAsset(string assetName, FigureDocument document, string[] parts, FigurePart part, FigureSet set)
        {
            var list = document.XmlFile.SelectNodes("//manifest/library/assets/asset");

            for (int i = 0; i < list.Count; i++)
            {
                var asset = list.Item(i);
                var name = asset.Attributes.GetNamedItem("name").InnerText;

                if (name != assetName)
                    continue;

                var offsetList = asset.ChildNodes;

                for (int j = 0; j < offsetList.Count; j++)
                {
                    var offsetData = offsetList.Item(j);

                    if (offsetData.Attributes.GetNamedItem("key") == null ||
                        offsetData.Attributes.GetNamedItem("value") == null)
                        continue;

                    if (offsetData.Attributes.GetNamedItem("key").InnerText != "offset")
                        continue;

                    var offsets = offsetData.Attributes.GetNamedItem("value").InnerText.Split(',');

                    return new AvatarAsset(name, FileUtil.SolveFile("figuredata/" + document.FileName + "/", name), int.Parse(offsets[0]), int.Parse(offsets[1]), part, set, CANVAS_WIDTH, CANVAS_HEIGHT, parts);
                }
            }

            return null;
        }

        public bool ValidateFigure()
        {
            //System.out.println("Validating: " + figure);
            String[] figureData = Figure.Split(".");

            if (figureData.Length == 0)
            {
                return false;
            }

            List<string> sets = new List<string>();

            foreach (string data in figureData)
            {
                String[] parts = data.Split("-");

                if (parts.Length < 2 || parts.Length > 3)
                {
                    return false;
                }

                sets.Add(parts[0]);
            }

            foreach (var figureSetType in FiguredataReader.FigureSetTypes.Values)
            {
                if (figureSetType.IsMandatory && !sets.Contains(figureSetType.Set))
                {
                    return false;
                }
            }

            foreach (string data in figureData)
            {
                string[] parts = data.Split("-");

                if (parts.Length < 2 || parts.Length > 3)
                {
                    return false;
                }

                String set = parts[0];
                String setId = parts[1];

                var figureSet = FiguredataReader.FigureSets.Values.FirstOrDefault(s =>
                        s.SetType == set &&
                        s.Id == setId);

                if (figureSet == null)
                {
                    return false;
                }


                if (!figureSet.Selectable)
                {
                    return false;
                }

                var figureSetType = FiguredataReader.FigureSetTypes[set];

                if (parts.Length > 2 && parts[2].Length > 0)
                {
                    var paletteId = parts[2];

                    if (FiguredataReader.FigurePalettes[figureSetType.PaletteId].Count(palette => palette.ColourId == paletteId) == 0)
                    {
                        return false;
                    }
                }
            }

            return true;
        }
        public static Rgba32 HexToColor(string hexString)
        {
            if (hexString.ToLower() == "transparent")
            {
                return SixLabors.ImageSharp.Color.Transparent;
            }

            try
            {
                var drawingColor = System.Drawing.ColorTranslator.FromHtml("#" + hexString);
                return SixLabors.ImageSharp.Color.FromRgb(drawingColor.R, drawingColor.G, drawingColor.B);
            }
            catch (Exception ex)
            {
            }

            return SixLabors.ImageSharp.Color.FromRgb(254, 254, 254);
        }


    }
}
