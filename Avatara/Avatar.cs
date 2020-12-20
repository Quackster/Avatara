using Avatara.Figure;
using System;
using System.Collections.Generic;
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

        public Avatar(string figure, bool isSmall, int bodyDirection, int headDirection, FiguredataReader figuredataReader)
        {
            Figure = figure;
            IsSmall = isSmall;
            BodyDirection = bodyDirection;
            HeadDirection = headDirection;
            FiguredataReader = figuredataReader;
        }

        public void Run()
        {
            bool isValid = validateFigure(this.Figure);
            Console.WriteLine("Figure valid: " + isValid);
        }

        public bool validateFigure(String figure)
        {
            //System.out.println("Validating: " + figure);
            String[] figureData = figure.Split(".");

            if (figureData.Length == 0)
            {
                return false;
            }

            List<string> sets = new List<string>();

            foreach (string data in figureData)
            {
                String[] parts = data.Split("-");

                if (parts.Length < 2 || parts.Length > 3)
                {
                    return false;
                }

                sets.Add(parts[0]);
            }

            foreach (var figureSetType in FiguredataReader.FigureSetTypes.Values)
            {
                if (figureSetType.IsMandatory && !sets.Contains(figureSetType.Set))
                {
                    return false;
                }
            }

            foreach (string data in figureData)
            {
                string[] parts = data.Split("-");

                if (parts.Length < 2 || parts.Length > 3)
                {
                    return false;
                }

                String set = parts[0];
                String setId = parts[1];

                var figureSet = FiguredataReader.FigureSets.Values.FirstOrDefault(s =>
                        s.SetType == set &&
                        s.Id == setId);

                if (figureSet == null)
                {
                    return false;
                }


                if (!figureSet.Selectable)
                {
                    return false;
                }

                var figureSetType = FiguredataReader.FigureSetTypes[set];

                if (parts.Length > 2 && parts[2].Length > 0)
                {
                    var paletteId = parts[2];

                    if (FiguredataReader.FigurePalettes[figureSetType.PaletteId].Count(palette => palette.ColourId == paletteId) == 0)
                    {
                        return false;
                    }
                }
            }

            return true;
        }
    }
}
