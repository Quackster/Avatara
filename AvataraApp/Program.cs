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

                string figure = "hd-180-1.hr-100-.ch-260-62.lg-270-64.sh-300-64.ha-1008-.ea-1402-.ca-1806-73";//"ea -1406-62.lg-285-96.fa-1205-62.hd-190-28.ch-255-96.ha-1006-110.hr-831-42.sh-300-64.wa-2012-62";
                Console.WriteLine("Parsing: " + figure);

                var avatar = new Avatar(figure, false, 2, 2, figuredataReader);
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
