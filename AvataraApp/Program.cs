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

                var avatar = new Avatar("ch-250-62", "b", 2, 2, figuredataReader, cropImage: true);
                File.WriteAllBytes("figure-1.png", avatar.Run());

                avatar = new Avatar("hd-206-7", "b", 2, 2, figuredataReader, headOnly: true);
                File.WriteAllBytes("figure-2.png", avatar.Run());


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
