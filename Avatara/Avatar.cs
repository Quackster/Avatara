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
        public string Action; // stand
        public string Gesture;
        public int Frame;

        public Image<Rgba32> BodyCanvas;
        public Image<Rgba32> FaceCanvas;

        public int CANVAS_WIDTH = 110;
        public int CANVAS_HEIGHT = 64;

        public Avatar(string figure, bool isSmall, int bodyDirection, int headDirection, FiguredataReader figuredataReader, string action = "std", string gesture = "sml")
        {
            Figure = figure;
            IsSmall = isSmall;
            BodyDirection = bodyDirection;
            HeadDirection = headDirection;
            FiguredataReader = figuredataReader;

            if (isSmall)
            {
                CANVAS_HEIGHT = CANVAS_HEIGHT / 2;
                CANVAS_WIDTH = CANVAS_WIDTH / 2;
            }

            Action = action;
            Gesture = gesture;

            if (action == "lay")
            {
                var temp = CANVAS_WIDTH;
                CANVAS_WIDTH = CANVAS_HEIGHT;
                CANVAS_HEIGHT = temp;

                this.Gesture = "lay";
                this.Action = "lay";
                this.HeadDirection = this.BodyDirection;
            }

            BodyCanvas = new Image<Rgba32>(CANVAS_HEIGHT, CANVAS_WIDTH, HexToColor("transparent"));
            FaceCanvas = new Image<Rgba32>(CANVAS_HEIGHT, CANVAS_WIDTH, HexToColor("transparent"));
        }

        public byte[] Run()
        {
            var buildQueue = BuildDrawQueue();

            if (buildQueue == null)
                return null;

            return DrawImage(buildQueue);
        }

        private byte[] DrawImage(List<AvatarAsset> buildQueue)
        {
            using (var bodyCanvas = this.BodyCanvas)
            {
                using (var faceCanvas = this.BodyCanvas)
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

                        try
                        {
                            if (this.IsHead(asset.Part.Type))
                            {
                                faceCanvas.Mutate(ctx =>
                                {
                                    ctx.DrawImage(image, new SixLabors.ImageSharp.Point(faceCanvas.Width - asset.ImageX, faceCanvas.Height - asset.ImageY), graphicsOptions);
                                });
                            }
                            else
                            {
                                bodyCanvas.Mutate(ctx =>
                                {
                                    ctx.DrawImage(image, new SixLabors.ImageSharp.Point(bodyCanvas.Width - asset.ImageX, bodyCanvas.Height - asset.ImageY), graphicsOptions);
                                });
                            }
                        }
                        catch { }
                    }

                    if (BodyDirection == 4 || BodyDirection == 6 || BodyDirection == 5)
                    {
                        bodyCanvas.Mutate(ctx =>
                        {
                            ctx.Flip(FlipMode.Horizontal);
                        });
                    }

                    if (HeadDirection == 4 || HeadDirection == 6 || HeadDirection == 5)
                    {
                        faceCanvas.Mutate(ctx =>
                        {
                            ctx.Flip(FlipMode.Horizontal);
                        });

                       
                    }

                    bodyCanvas.Mutate(ctx =>
                    {
                        ctx.DrawImage(faceCanvas, 1f);
                    });

                    using (Bitmap tempBitmap = bodyCanvas.ToBitmap())
                    {
                        // Crop the image
                        //using (Bitmap croppedBitmap = //ImageUtil.TrimBitmap(tempBitmap, HexToColor("transparent")))
                        {
                            return RenderImage(tempBitmap);
                        }
                    }
                }
            }
        }

        private byte[] RenderImage(Bitmap croppedBitmap)
        {
            return croppedBitmap.ToByteArray();
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

            int direction;
            string gesture;

            if (IsHead(part.Type))
            {
                direction = HeadDirection;
                gesture = Gesture;

                if (HeadDirection == 4)
                    direction = 2;

                if (HeadDirection == 6)
                    direction = 0;

                if (HeadDirection == 5)
                    direction = 1;
            } else
            {
                direction = BodyDirection;
                gesture = Action;

                if (BodyDirection == 4)
                    direction = 2;

                if (BodyDirection == 6)
                    direction = 0;

                if (BodyDirection == 5)
                    direction = 1;
            }

            var asset = LocateAsset((this.IsSmall ? "sh" : "h") + "_" + gesture + "_" + part.Type + "_" + part.Id + "_" + direction + "_" + Frame, document, parts, part, set);

            if (asset == null)
                asset = LocateAsset((this.IsSmall ? "sh" : "h") + "_" + "std" + "_" + part.Type + "_" + part.Id + "_" + direction + "_" + Frame, document, parts, part, set);

            if (IsSmall)
            {
                if (asset == null)
                    asset = LocateAsset((this.IsSmall ? "sh" : "h") + "_" + "std" + "_" + part.Type + "_" + 1 + "_" + direction + "_" + Frame, document, parts, part, set);
            }

            return asset;
        }

        public bool IsHead(string figurePart)
        {
            return figurePart.Contains("hr") ||
                figurePart.Contains("hd") ||
                figurePart.Contains("he") ||
                figurePart.Contains("ha") ||
                figurePart.Contains("ea") ||
                figurePart.Contains("fa") ||
                figurePart.Contains("ey") ||
                figurePart.Contains("fc");
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

                    return new AvatarAsset(this.IsSmall, Action, name, FileUtil.SolveFile("figuredata/" + document.FileName + "/", name), int.Parse(offsets[0]), int.Parse(offsets[1]), part, set, CANVAS_WIDTH, CANVAS_HEIGHT, parts);
                }
            }

            return null;
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
            catch (Exception)
            {
            }

            return SixLabors.ImageSharp.Color.FromRgb(254, 254, 254);
        }


    }
}
