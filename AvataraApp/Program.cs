using Avatara;
using Avatara.Figure;
using System;

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

                string figure = "ha-1012-110.he-1609-62.hr-678-58.sh-906-64.ca-1807-62.wa-2005-62.lg-280-109.hd-180-1.ch-805-109";
                Console.WriteLine("Parsing: " + figure);

                var avatar = new Avatar(figure, false, 2, 2, figuredataReader, "std");
                avatar.Run();

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
