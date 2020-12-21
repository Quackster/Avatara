﻿using Avatara;
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

                string figure = "hd-190-28.ch-215-110.lg-275-62.hr-828-52.ha-1006-";
                Console.WriteLine("Parsing: " + figure);

                var avatar = new Avatar(figure, false, 2, 2, figuredataReader, "lay", "agr");
                File.WriteAllBytes("figure1-1.png", avatar.Run());

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
