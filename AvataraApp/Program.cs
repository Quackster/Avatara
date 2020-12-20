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

                var figuredataReader = new FiguredataReader();
                figuredataReader.LoadFigurePalettes();
                figuredataReader.loadFigureSetTypes();
                figuredataReader.LoadFigureSets();

                var avatar = new Avatar("hd-180-1.hr-100-.ch-260-62.lg-270-64.sh-300-64.ha-1008-.ea-1402-.ca-1806-73", false, 2, 2, figuredataReader);
                avatar.Run();

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }

            Console.Read();
        }
    }
}
