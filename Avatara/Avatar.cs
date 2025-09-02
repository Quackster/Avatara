// For .NET Standard 2.0
using Alcosmos.Figure;
using Avatara.Extensions;
using Avatara.Figure;
using Avatara.Util;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace Avatara
{
    public class Avatar
    {
        public string Figure;
        public string Size;
        public bool IsSmall;
        public int BodyDirection;
        public int HeadDirection;
        public FiguredataReader FiguredataReader;
        public string Action; // stand
        public string Gesture;
        public int Frame;
        public int CarryDrink;

        public Image<Rgba32> BodyCanvas;
        public Image<Rgba32> FaceCanvas;
        public Image<Rgba32> DrinkCanvas;

        public int CANVAS_HEIGHT = 110;
        public int CANVAS_WIDTH = 64;
        public bool RenderEntireFigure;
        public bool CropImage = false;

        public Avatar(FiguredataReader figuredataReader, string figure, string size, int bodyDirection, int headDirection, string action = "std", string gesture = "std", bool headOnly = false, int frame = 1, int carryDrink = 0, bool cropImage = false)
        {
            if (figure.All(char.IsDigit))
            {
                figure = new FigureConverter().ConvertOldToNew(figure);
                Console.WriteLine("Converting old figure to new: " + figure);
            }

            Figure = figure;
            Size = size.ToLower();
            IsSmall = Size != "b" && Size != "l";
            BodyDirection = bodyDirection;
            HeadDirection = headDirection;
            FiguredataReader = figuredataReader;
            RenderEntireFigure = !headOnly;
            CropImage = cropImage;

            if (IsSmall)
            {
                CANVAS_WIDTH /= 2;
                CANVAS_HEIGHT /= 2;
            }

            Action = action;
            Gesture = gesture;

            if (action == "lay")
            {
                var temp = CANVAS_HEIGHT;
                CANVAS_HEIGHT = CANVAS_WIDTH;
                CANVAS_WIDTH = temp;
            }

            BodyCanvas = new Image<Rgba32>(CANVAS_WIDTH, CANVAS_HEIGHT, HexToColor("transparent"));
            FaceCanvas = new Image<Rgba32>(CANVAS_WIDTH, CANVAS_HEIGHT, HexToColor("transparent"));
            DrinkCanvas = new Image<Rgba32>(CANVAS_WIDTH, CANVAS_HEIGHT, HexToColor("transparent"));

            Frame = frame - 1;
            CarryDrink = carryDrink;

            if (action == "lay")
            {
                this.Gesture = "lay";
                this.Action = "lay";
                this.HeadDirection = this.BodyDirection;

                if (this.BodyDirection != 2 && this.BodyDirection != 4)
                    this.BodyDirection = 2;

                if (this.HeadDirection != 2 && this.HeadDirection != 4)
                    this.HeadDirection = 2;

                this.CarryDrink = 0;
            }
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
            using (var faceCanvas = this.FaceCanvas)
            using (var bodyCanvas = this.BodyCanvas)
            using (var drinkCanvas = this.DrinkCanvas)
            {
                foreach (var asset in buildQueue)
                {
                    if (!RenderEntireFigure && !IsHead(asset.Part.Type))
                        continue;

                    DrawAsset(buildQueue, BodyCanvas, FaceCanvas, DrinkCanvas, asset);
                }

                if (HeadDirection == 4 || HeadDirection == 6 || HeadDirection == 5)
                {
                    FaceCanvas.Mutate(ctx => ctx.Flip(FlipMode.Horizontal));
                }

                if (BodyDirection == 4 || BodyDirection == 6 || BodyDirection == 5)
                {
                    BodyCanvas.Mutate(ctx => ctx.Flip(FlipMode.Horizontal));
                    DrinkCanvas.Mutate(ctx => ctx.Flip(FlipMode.Horizontal));
                }

                BodyCanvas.Mutate(ctx => ctx.DrawImage(FaceCanvas, 1f));
                BodyCanvas.Mutate(ctx => ctx.DrawImage(DrinkCanvas, 1f));

                Image<Rgba32> finalCanvas = RenderEntireFigure ? this.BodyCanvas : this.FaceCanvas;
                return RenderImage(finalCanvas);
            }
        }

        private byte[] RenderImage(Image<Rgba32> croppedBitmap)
        {
            if (Size == "l")
            {
                using (var image = croppedBitmap.Clone())
                {
                    var resizeOptions = new ResizeOptions
                    {
                        Size = new SixLabors.ImageSharp.Size(image.Width * 2, image.Height * 2),
                        Sampler = KnownResamplers.NearestNeighbor
                    };
                    image.Mutate(x => x.Resize(resizeOptions));
                    return ToBytes(image);
                }
            }
            else
            {
                return ToBytes(croppedBitmap);
            }
        }

        private static byte[] ToBytes(Image<Rgba32> bitmap)
        {
            using (var ms = new MemoryStream())
            {
                bitmap.Save(ms, new SixLabors.ImageSharp.Formats.Png.PngEncoder());
                return ms.ToArray();
            }
        }

        private void DrawAsset(List<AvatarAsset> buildQueue, Image<Rgba32> bodyCanvas, Image<Rgba32> faceCanvas, Image<Rgba32> drinkCanvas, AvatarAsset asset)
        {
            var graphicsOptions = new GraphicsOptions
            {
                ColorBlendingMode = PixelColorBlendingMode.Normal
            };

            using (var image = SixLabors.ImageSharp.Image.Load<Rgba32>(asset.FileName))
            {
                if (buildQueue.Count(x => x.Set.HiddenLayers.Contains(asset.Part.Type)) > 0)
                    return;

                if (asset.Part.Type != "ey")
                {
                    if (asset.Part.Colorable)
                    {
                        string[] parts = asset.Parts;
                        if (parts.Length > 2 && parts[2].Length > 0 && parts[2].IsNumeric())
                        {
                            var paletteId = int.Parse(parts[2]);
                            if (FiguredataReader.FigureSetTypes.ContainsKey(parts[0]))
                            {
                                var figureTypeSet = FiguredataReader.FigureSetTypes[parts[0]];
                                var palette = FiguredataReader.FigurePalettes[figureTypeSet.PaletteId];
                                var colourData = palette.FirstOrDefault(x => x.ColourId == parts[2]);
                                if (colourData != null)
                                {
                                    TintImage(image, colourData.HexColor, 255);
                                }
                            }
                        }
                    }
                }
                else
                {
                    TintImage(image, "FFFFFF", 255);
                }

                try
                {
                    if (this.IsHead(asset.Part.Type))
                    {
                        var point = new SixLabors.ImageSharp.Point(faceCanvas.Width - asset.ImageX, faceCanvas.Height - asset.ImageY);
                        point = MutatePoint(point, faceCanvas);

                        faceCanvas.Mutate(ctx =>
                        {
                            ctx.DrawImage(image, point, graphicsOptions);
                        });
                    }
                    else
                    {
                        if (!asset.IsDrinkCanvas)
                        {
                            var point = new SixLabors.ImageSharp.Point(bodyCanvas.Width - asset.ImageX, bodyCanvas.Height - asset.ImageY);
                            point = MutatePoint(point, bodyCanvas);

                            bodyCanvas.Mutate(ctx =>
                            {
                                ctx.DrawImage(image, point, graphicsOptions);
                            });
                        }
                        else
                        {
                            var point = new SixLabors.ImageSharp.Point(bodyCanvas.Width - asset.ImageX, bodyCanvas.Height - asset.ImageY);
                            point = MutatePoint(point, bodyCanvas);

                            drinkCanvas.Mutate(ctx =>
                            {
                                ctx.DrawImage(image, point, graphicsOptions);
                            });
                        }
                    }
                }
                catch { }
            }
        }

        private SixLabors.ImageSharp.Point MutatePoint(SixLabors.ImageSharp.Point point, Image<Rgba32> canvas)
        {
            var x = point.X + 1;
            var y = point.Y + 2;
            return new SixLabors.ImageSharp.Point(x, y);
        }

        private List<AvatarAsset> BuildDrawQueue()
        {
            var tempQueue = new List<AvatarAsset>();
            var figureData = new Dictionary<string, string>();

            foreach (string data in Figure.Split('.'))
            {
                string[] parts = data.Split('-');
                figureData[parts[0]] = string.Join("-", parts);
            }

            foreach (string data in figureData.Values)
            {
                string[] parts = data.Split('-');
                if (parts.Length < 2) return null;

                var setList = FiguredataReader.FigureSets.Values.Where(x => x.Id == parts[1]).ToList();
                if (setList.Any())
                {
                    foreach (var set in setList)
                    {
                        var partList = set.FigureParts;
                        foreach (var part in partList)
                        {
                            var t = LoadFigureAsset(parts, part, set);
                            if (t == null) continue;
                            tempQueue.Add(t);
                        }
                    }
                }
            }

            var headRenderList = tempQueue.Where(x => IsHead(x.Part.Type)).ToList();
            int headRenderOrder = headRenderList.Count > 0 ? headRenderList.Max(x => x.RenderOrder) : 1;

            if (CarryDrink > 0)
            {
                var carryItemAsset = this.LoadCarryItemAsset(CarryDrink);
                if (carryItemAsset != null)
                {
                    if (BodyDirection == 1 || BodyDirection == 5 || BodyDirection == 6 || BodyDirection == 0)
                    {
                        carryItemAsset.IsDrinkCanvas = false;
                        carryItemAsset.RenderOrder = 0;
                    }
                    else
                    {
                        carryItemAsset.RenderOrder = headRenderOrder + 1;
                        carryItemAsset.IsDrinkCanvas = true;
                    }
                    tempQueue.Add(carryItemAsset);

                    if (Action == "drk")
                    {
                        foreach (var asset in tempQueue)
                        {
                            if (asset == carryItemAsset) continue;
                            if (asset.Name.Contains("_drk_"))
                            {
                                asset.RenderOrder = 100;
                                asset.IsDrinkCanvas = true;
                            }
                        }
                    }
                }
            }

            tempQueue = tempQueue.OrderBy(x => x.RenderOrder).ToList();
            return tempQueue;
        }

        private AvatarAsset LoadCarryItemAsset(int carryId)
        {
            int direction = BodyDirection;
            if (BodyDirection == 4) direction = 2;
            if (BodyDirection == 6) direction = 0;
            if (BodyDirection == 5) direction = 1;

            var part = new FigurePart("0", "ri", false, 0);
            var set = new FigureSet("ri", "", "", false, false, false);

            var asset = LocateAsset((this.IsSmall ? "sh" : "h") + "_" + Action + "_ri_" + carryId + "_" + direction + "_0", null, part, set);
            if (asset == null)
                asset = LocateAsset((this.IsSmall ? "sh" : "h") + "_crr_ri_" + carryId + "_" + direction + "_0", null, part, set);
            if (asset == null)
                asset = LocateAsset((this.IsSmall ? "sh" : "h") + "_std_ri_" + carryId + "_0_0", null, part, set);

            return asset;
        }

        private AvatarAsset LoadFigureAsset(string[] parts, FigurePart part, FigureSet set)
        {
            int direction;
            string gesture;

            if (IsHead(part.Type))
            {
                direction = HeadDirection;
                gesture = Gesture;
                if (HeadDirection == 4) direction = 2;
                if (HeadDirection == 6) direction = 0;
                if (HeadDirection == 5) direction = 1;
            }
            else
            {
                direction = BodyDirection;
                gesture = Action;
                if (BodyDirection == 4) direction = 2;
                if (BodyDirection == 6) direction = 0;
                if (BodyDirection == 5) direction = 1;
            }

            if (direction == 1 && part.Type == "ls") return null;
            if (Action == "lay") { if (BodyDirection == 4) direction = 2; }
            if (CarryDrink > 0 && Action != "lay" && Action != "drk")
            {
                var partsForAction = new string[] { "ls", "lh" };
                if (partsForAction.Contains(part.Type))
                    gesture = "std";
            }

            AvatarAsset asset = null;

            if (CarryDrink > 0 && (part.Type == "rs" || part.Type == "rh") && Action != "drk" && Action != "crr")
                asset = LocateAsset((this.IsSmall ? "sh" : "h") + "_" + "crr" + "_" + part.Type + "_" + part.Id + "_" + direction + "_0", parts, part, set);

            if (asset == null)
                asset = LocateAsset((this.IsSmall ? "sh" : "h") + "_" + gesture + "_" + part.Type + "_" + part.Id + "_" + direction + "_" + Frame, parts, part, set);

            if (asset == null)
                asset = LocateAsset((this.IsSmall ? "sh" : "h") + "_" + "std" + "_" + part.Type + "_" + part.Id + "_" + direction + "_" + Frame, parts, part, set);

            if (asset == null)
                asset = LocateAsset((this.IsSmall ? "sh" : "h") + "_" + "std" + "_" + part.Type + "_" + part.Id + "_" + direction + "_0", parts, part, set);

            if (IsSmall)
            {
                if (asset == null)
                    asset = LocateAsset((this.IsSmall ? "sh" : "h") + "_" + "std" + "_" + part.Type + "_" + 1 + "_" + direction + "_" + Frame, parts, part, set);
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

        private AvatarAsset LocateAsset(string assetName, string[] parts, FigurePart part, FigureSet set)
        {
            var offsets = FlashExtractor.Instance.Parts.ContainsKey(assetName) ? FlashExtractor.Instance.Parts[assetName] : null;
            if (offsets == null) return null;

            var file = FileUtil.SolveFile("figuredata/images/", assetName, endsWith: false, equals: true);
            if (file == null) return null;

            return new AvatarAsset(this.IsSmall, Action, assetName, file, int.Parse(offsets.Split(',')[0]), int.Parse(offsets.Split(',')[1]), part, set, CANVAS_HEIGHT, CANVAS_WIDTH, parts);
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

        public static Rgba32 HexToColor(string hexColourCode)
        {
            if (hexColourCode == "transparent")
                return SixLabors.ImageSharp.Color.Transparent.ToPixel<Rgba32>();

            hexColourCode = hexColourCode.Replace("#", "").ToLower();

            return SixLabors.ImageSharp.Color.FromRgb(
                byte.Parse(hexColourCode.Substring(0, 2), NumberStyles.HexNumber),
                byte.Parse(hexColourCode.Substring(2, 2), NumberStyles.HexNumber),
                byte.Parse(hexColourCode.Substring(4, 2), NumberStyles.HexNumber));
        }
    }
}
