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

                FigureExtractor.Parse("2013_figuredata");
           // FigureExtractor.Parse("shockwave_figuredata");

            var Parts = FigureExtractor.Parts;

                var figuredataReader = new FiguredataReader();
                figuredataReader.LoadFigurePalettes("2013_figuredata");
                figuredataReader.loadFigureSetTypes("2013_figuredata");
                figuredataReader.LoadFigureSets("2013_figuredata");

            /*
            figuredataReader.LoadFigurePalettes("shockwave_figuredata");
            figuredataReader.loadFigureSetTypes("shockwave_figuredata");
            figuredataReader.LoadFigureSets("shockwave_figuredata");*/


            var avatar = new Avatar("cc-260-62.lg-270-64.hr-100-.ch-210-66.ha-3139-82.hd-209-1.sh-300-64", "b", 2, 2, figuredataReader, cropImage: true);
                File.WriteAllBytes("figure-1.png", avatar.Run());

            /*
                avatar = new Avatar("hd-206-7", "b", 2, 2, figuredataReader, headOnly: true);
                File.WriteAllBytes("figure-2.png", avatar.Run());*/
            avatar = new Avatar("fa-1205-67.hd-180-17.lg-280-92.hr-170-37.cc-3246-62.ch-210-92", "b", 1, 1, figuredataReader, cropImage: true);
            File.WriteAllBytes("figure-2.png", avatar.Run());


            Console.WriteLine("Done");


            Console.Read();
        }
    }
}
