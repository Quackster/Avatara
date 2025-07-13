using System.Drawing;
using System;
using Avatara;
using Avatara.Figure;

namespace Test
{
    internal class Program
    {
        static void Main(string[] args)
        {
            // ... only need to be called once

            FiguredataReader.Instance.Load();
            FlashExtractor.Instance.Load();

            // ... the imager arguments...

            string? figure = "1000118001270012900121001";
            string size = "b";
            int bodyDirection = 2;
            int headDirection = 2;
            string action = "std";
            string gesture = "sml";
            bool headOnly = false;
            int frame = 1;
            int carryDrink = -1;
            bool cropImage = false;

            // .. generating the PNG output

            var avatar = new Avatar(FiguredataReader.Instance, figure, "b", bodyDirection, headDirection, action: action, gesture: gesture, headOnly: headOnly, frame: frame, carryDrink: carryDrink, cropImage: cropImage);
            File.WriteAllBytes("figure1.png", avatar.Run());

            var avatar2 = new Avatar(FiguredataReader.Instance, figure, "l", bodyDirection, headDirection, action: action, gesture: gesture, headOnly: headOnly, frame: frame, carryDrink: carryDrink, cropImage: cropImage);
            File.WriteAllBytes("figure2.png", avatar2.Run());

            var avatar3 = new Avatar(FiguredataReader.Instance, figure, "s", bodyDirection, headDirection, action: action, gesture: gesture, headOnly: headOnly, frame: frame, carryDrink: carryDrink, cropImage: cropImage);
            File.WriteAllBytes("figure3.png", avatar3.Run());
        }
    }
}