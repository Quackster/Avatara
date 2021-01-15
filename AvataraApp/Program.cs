using Avatara;
using Avatara.Figure;
using System;
using System.IO;

namespace AvataraApp
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                FigureExtractor.Parse();
                FigureExtractor.Parse();
                FigureExtractor.Parse();

                var Parts = FigureExtractor.Parts;

                var figuredataReader = new FiguredataReader();
                figuredataReader.LoadFigurePalettes();
                figuredataReader.loadFigureSetTypes();
                figuredataReader.LoadFigureSets();

                string figure = "hd-600-7.ch-645-66.lg-720-82.sh-725-80.hr-500-45";
                //string figure = "hd-180-1.hr-100-.ch-260-62.lg-275-64.ea-1402-.ca-1806-73";
                Console.WriteLine("Parsing: " + figure);

                var avatar = new Avatar(figure, "b", 1, 1, figuredataReader, gesture: "", action: "drk", carryDrink: 3);
                File.WriteAllBytes("figure1-1-drk.png", avatar.Run());

                avatar = new Avatar(figure, "l", 2, 2, figuredataReader, gesture: "", action: "drk", carryDrink: 3);
                File.WriteAllBytes("figure2-2-drk.png", avatar.Run());

                avatar = new Avatar(figure, "b", 3, 3, figuredataReader, gesture: "", action: "drk", carryDrink: 3);
                File.WriteAllBytes("figure3-3-drk.png", avatar.Run());


                Console.WriteLine("Done");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }

            Console.Read();
        }
    }
}
