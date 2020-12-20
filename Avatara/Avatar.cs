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

        public Avatar(string figure, bool isSmall, int bodyDirection, int headDirection, FiguredataReader figuredataReader)
        {
            Figure = figure;
            IsSmall = isSmall;
            BodyDirection = bodyDirection;
            HeadDirection = headDirection;
            FiguredataReader = figuredataReader;
            DrawingCanvas = new Image<Rgba32>(CANVAS_HEIGHT, CANVAS_WIDTH, HexToColor("transparent"));
        }

        public void Run()
        {
            bool isValid = ValidateFigure();
            DrawFigure();
        }

        private void DrawFigure()
        {
            List<string> sets = new List<string>();
            String[] figureData = Figure.Split(".");

            if (figureData.Length == 0)
            {
                return;
            }


            foreach (string data in figureData)
            {
                String[] parts = data.Split("-");

                if (parts.Length < 2 || parts.Length > 3)
                {
                    return;
                }

                sets.Add(parts[0]);
            }

            using (var canvas = this.DrawingCanvas)
            {

                foreach (string data in figureData)
                {
                    string[] parts = data.Split("-");

                    if (parts.Length < 2 || parts.Length > 3)
                    {
                        return;
                    }

                    var set = FiguredataReader.FigureSets.Values.FirstOrDefault(x => x.Id == parts[1]);

                    if (set == null)
                    {
                        return;
                    }

                    var part = set.FigureParts.FirstOrDefault(x => x.Type == parts[0]);

                    if (part == null)
                    {
                        return;
                    }

                    var asset = LoadFigureAsset(parts, part, set);

                    if (asset == null)
                        return;

                    var graphicsOptions = new GraphicsOptions();
                    graphicsOptions.ColorBlendingMode = PixelColorBlendingMode.Normal;

                    /*
                    if ((asset.Ink == "ADD" || asset.Ink == "33"))
                    {
                        graphicsOptions.ColorBlendingMode = PixelColorBlendingMode.Add;
                    }
                    else
                    {
                        graphicsOptions.ColorBlendingMode = PixelColorBlendingMode.Normal;
                    }
                    */

                    var image = SixLabors.ImageSharp.Image.Load<Rgba32>(asset.FileName);


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
                        croppedBitmap.Save("temp.png");
                    }
                }
            }

        }

        private AvatarAsset LoadFigureAsset(string[] parts, FigurePart part, FigureSet set)
        {
            var key = parts[0] + (IsSmall ? "_sh" : "_h");
            var document = FigureExtractor.Parts.ContainsKey(key) ? FigureExtractor.Parts[key] : null;

            if (document == null)
                return null;

            string assetName = (this.IsSmall ? "sh" : "h") + "_" + Action + "_" + part.Type  + "_" + part.Id + "_" + BodyDirection + "_" + Frame;
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

                    return new AvatarAsset(name, FileUtil.SolveFile("figuredata/" + document.FileName + "/", name), int.Parse(offsets[0]), int.Parse(offsets[1]), CANVAS_WIDTH, CANVAS_HEIGHT);
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
