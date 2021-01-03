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
        public int CarryDrink;

        public Image<Rgba32> BodyCanvas;
        public Image<Rgba32> FaceCanvas;
        public Image<Rgba32> DrinkCanvas;

        public int CANVAS_HEIGHT = 110;
        public int CANVAS_WIDTH = 64;
        public bool RenderEntireFigure;

        public Avatar(string figure, bool isSmall, int bodyDirection, int headDirection, FiguredataReader figuredataReader, string action = "std", string gesture = "sml", bool headOnly = false, int frame = 1, int carryDrink = 0)
        { 
            Figure = figure;
            IsSmall = isSmall;
            BodyDirection = bodyDirection;
            HeadDirection = headDirection;
            FiguredataReader = figuredataReader;
            RenderEntireFigure = !headOnly;

            if (isSmall)
            {
                CANVAS_WIDTH = CANVAS_WIDTH / 2;
                CANVAS_HEIGHT = CANVAS_HEIGHT / 2;
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
                    // Don't render anything BUT the head if we chose head only
                    if (!RenderEntireFigure && !IsHead(asset.Part.Type))
                        continue;

                    DrawAsset(buildQueue, BodyCanvas, FaceCanvas, DrinkCanvas, asset);
                }


                // Flip the head if necessary
                if (HeadDirection == 4 || HeadDirection == 6 || HeadDirection == 5)
                {
                    FaceCanvas.Mutate(ctx =>
                    {
                        ctx.Flip(FlipMode.Horizontal);
                    });
                }

                // Flip the body if necessary
                if (BodyDirection == 4 || BodyDirection == 6 || BodyDirection == 5)
                {
                    BodyCanvas.Mutate(ctx =>
                    {
                        ctx.Flip(FlipMode.Horizontal);
                    });

                    DrinkCanvas.Mutate(ctx =>
                    {
                        ctx.Flip(FlipMode.Horizontal);
                    });
                }

                // Draw head on body
                BodyCanvas.Mutate(ctx =>
                {
                    ctx.DrawImage(FaceCanvas, 1f);
                });

                // Draw drink animation on body
                BodyCanvas.Mutate(ctx =>
                {
                    ctx.DrawImage(DrinkCanvas, 1f);
                });

                Image<Rgba32> finalCanvas = null;

                if (RenderEntireFigure)
                {
                    finalCanvas = this.BodyCanvas;
                }
                else
                {
                    finalCanvas = this.FaceCanvas;
                }

                using (Bitmap tempBitmap = finalCanvas.ToBitmap())
                {
                    if (!RenderEntireFigure)
                    {
                        using (var bitmap = ImageUtil.TrimBitmap(tempBitmap))
                        {
                            return RenderImage(bitmap);
                        }
                    }
                    else
                    {
                        // Crop the image
                        return RenderImage(tempBitmap);
                    }
                }
            }
        }

        private void DrawAsset(List<AvatarAsset> buildQueue, Image<Rgba32> bodyCanvas, Image<Rgba32> faceCanvas, Image<Rgba32> drinkCanvas, AvatarAsset asset)
        {
            var graphicsOptions = new GraphicsOptions();
            graphicsOptions.ColorBlendingMode = PixelColorBlendingMode.Normal;

            using (var image = SixLabors.ImageSharp.Image.Load<Rgba32>(asset.FileName))
            {
                if (buildQueue.Count(x => x.Set.HiddenLayers.Contains(asset.Part.Type)) > 0)
                    return;

                if (asset.Part.Type != "ey")
                {
                    if (asset.Part.Colorable)
                    {
                        string[] parts = asset.Parts;

                        if (parts.Length > 2)
                        {
                            var paletteId = int.Parse(parts[2]);

                            if (!FiguredataReader.FigureSetTypes.ContainsKey(parts[0]))
                                return;

                            var figureTypeSet = FiguredataReader.FigureSetTypes[parts[0]];
                            var palette = FiguredataReader.FigurePalettes[figureTypeSet.PaletteId];
                            var colourData = palette.FirstOrDefault(x => x.ColourId == parts[2]);

                            if (colourData == null)
                            {
                                return;
                            }

                            TintImage(image, colourData.HexColor, 255);

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
                        faceCanvas.Mutate(ctx =>
                        {
                            ctx.DrawImage(image, new SixLabors.ImageSharp.Point(faceCanvas.Width - asset.ImageX, faceCanvas.Height - asset.ImageY), graphicsOptions);
                        });
                    }
                    else
                    {
                        if (!asset.IsDrinkCanvas)
                        {
                            bodyCanvas.Mutate(ctx =>
                            {
                                ctx.DrawImage(image, new SixLabors.ImageSharp.Point(bodyCanvas.Width - asset.ImageX, bodyCanvas.Height - asset.ImageY), graphicsOptions);
                            });
                        }
                        else
                        {
                            drinkCanvas.Mutate(ctx =>
                            {
                                ctx.DrawImage(image, new SixLabors.ImageSharp.Point(bodyCanvas.Width - asset.ImageX, bodyCanvas.Height - asset.ImageY), graphicsOptions);
                            });
                        }
                    }
                }
                catch { }
            }
        }

        private byte[] RenderImage(Bitmap croppedBitmap)
        {
            return croppedBitmap.ToByteArray();
        }

        private List<AvatarAsset> BuildDrawQueue()
        {
            List<AvatarAsset> tempQueue = new List<AvatarAsset>();
            Dictionary<string, string> figureData = new Dictionary<string, string>();


            foreach (string data in Figure.Split("."))
            {
                string[] parts = data.Split("-");
                figureData.Add(parts[0], string.Join("-", parts));

            }

            var assetLast = new List<AvatarAsset>();

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

                        tempQueue.Add(t);
                    }
                }
            }

            // Find maxiumum head render order (used for drinks)
            int headRenderOrder = tempQueue.Where(x => IsHead(x.Part.Type)).Max(x => x.RenderOrder);

            // Render drink next
            if (CarryDrink > 0)
            {
                var carryItemAsset = this.LoadCarryItemAsset(CarryDrink);

                if (carryItemAsset != null)
                {
                    // Remove drink drink canvas, hide behind face on these rotations
                    if (BodyDirection == 1 || BodyDirection == 5 || BodyDirection == 6 || BodyDirection == 0)
                    {
                        carryItemAsset.IsDrinkCanvas = false; // Render on body instead of drink canvas
                        carryItemAsset.RenderOrder = 0;
                    }
                    else
                    {
                        // Render drink one above the max render order of the head
                        carryItemAsset.RenderOrder = headRenderOrder + 1;
                        carryItemAsset.IsDrinkCanvas = true;
                    }

                    tempQueue.Add(carryItemAsset);

                    // Move hands in front of drink
                    if (Action == "drk")
                    {
                        foreach (var asset in tempQueue)
                        {
                            if (asset == carryItemAsset)
                                continue;

                            // Render arms and hands above the rest, including the item being carried
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
            var key = "ri" + (IsSmall ? "_sh" : "_h");
            var document = FigureExtractor.Parts.ContainsKey(key) ? FigureExtractor.Parts[key] : null;

            if (document == null)
                return null;

            int direction = BodyDirection;

            if (BodyDirection == 4)
                direction = 2;

            if (BodyDirection == 6)
                direction = 0;

            if (BodyDirection == 5)
                direction = 1;

            var part = new FigurePart("0", "ri", false, 0);
            var set = new FigureSet("ri", "", "", false, false, false);

            var asset = LocateAsset((this.IsSmall ? "sh" : "h") + "_" + Action + "_ri_" + carryId + "_" + direction + "_0", document, null, part, set);

            if (asset == null)
                asset = LocateAsset((this.IsSmall ? "sh" : "h") + "_crr_ri_" + carryId + "_" + direction + "_0", document, null, part, set);

            if (asset == null)
                asset = LocateAsset((this.IsSmall ? "sh" : "h") + "_std_ri_" + carryId + "_0_0", document, null, part, set);

            return asset;
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

            // Hide left arm on side view
            if (direction == 1 && part.Type == "ls")
                return null;

            if (Action == "lay")
            {
                if (BodyDirection == 4)
                    direction = 2;
            }

            if (CarryDrink > 0 && Action != "lay" && Action != "drk")
            {
                var partsForAction = new string[] { "ls", "lh" };

                if (partsForAction.Contains(part.Type))
                    gesture = "std";
            }

            AvatarAsset asset = null;

            if (CarryDrink > 0 && (part.Type == "rs" || part.Type == "rh") && Action != "drk" && Action != "crr")
                asset = LocateAsset((this.IsSmall ? "sh" : "h") + "_" + "crr" + "_" + part.Type + "_" + part.Id + "_" + direction + "_0", document, parts, part, set);

            if (asset == null)
                asset = LocateAsset((this.IsSmall ? "sh" : "h") + "_" + gesture + "_" + part.Type + "_" + part.Id + "_" + direction + "_" + Frame, document, parts, part, set);

            if (asset == null)
                asset = LocateAsset((this.IsSmall ? "sh" : "h") + "_" + "std" + "_" + part.Type + "_" + part.Id + "_" + direction + "_" + Frame, document, parts, part, set);
            
            if (asset == null)
                asset = LocateAsset((this.IsSmall ? "sh" : "h") + "_" + "std" + "_" + part.Type + "_" + part.Id + "_" + direction + "_0", document, parts, part, set);

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

                    return new AvatarAsset(this.IsSmall, Action, name, FileUtil.SolveFile("figuredata/" + document.FileName + "/", name), int.Parse(offsets[0]), int.Parse(offsets[1]), part, set, CANVAS_HEIGHT, CANVAS_WIDTH, parts);
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
