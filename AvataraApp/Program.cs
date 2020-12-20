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

                string figure = "lg-696-95.ea-1406-.hr-890-31.fa-1204-.hd-625-2.ch-660-93.he-1601-.ca-1813-";
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
