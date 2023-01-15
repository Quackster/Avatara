using Avatara.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Avatara.Figure
{
    public class FiguredataReader
    {
        #region Fields

        public static readonly FiguredataReader Instance = new FiguredataReader();

        #endregion

        public Dictionary<int, List<FigureColor>> FigurePalettes;
        public Dictionary<string, FigureSetType> FigureSetTypes;
        public Dictionary<string, FigureSet> FigureSets;

        public bool? MandatoryGenderFlag { get; }

        public FiguredataReader()
        {
            this.FigurePalettes = new Dictionary<int, List<FigureColor>>();
            this.FigureSetTypes = new Dictionary<string, FigureSetType>();
            this.FigureSets = new Dictionary<string, FigureSet>();
            this.MandatoryGenderFlag = null;

        }

        public void Load()
        {
            this.LoadFigureSets();
            this.LoadFigureSetTypes();
            this.LoadFigurePalettes();
        }

        public void LoadFigureSets()
        {
            var xmlFile = FileUtil.SolveXmlFile("figuredata");
            var list = xmlFile.SelectNodes("//sets/settype/set");

            for (int i = 0; i < list.Count; i++)
            {
                var set = list.Item(i);
                String setType = set.ParentNode.Attributes.GetNamedItem("type").InnerText;
                String id = set.Attributes.GetNamedItem("id").InnerText;
                String gender = set.Attributes.GetNamedItem("gender").InnerText;
                bool club = set.Attributes.GetNamedItem("club").InnerText == "1";
                bool colourable = set.Attributes.GetNamedItem("colorable").InnerText == "1";
                bool selectable = set.Attributes.GetNamedItem("selectable").InnerText == "1";

                var figureSet = new FigureSet(setType, id, gender, club, colourable, selectable);
                var partList = set.ChildNodes;

                for (int j = 0; j < partList.Count; j++)
                {
                    var part = partList.Item(j);//.getChildNodes();

                    if (part.Name == "hiddenlayers" || part.Attributes == null)
                    {
                        continue;
                    }

                    figureSet.FigureParts.Add(new FigurePart(
                            part.Attributes.GetNamedItem("id").InnerText,
                            part.Attributes.GetNamedItem("type").InnerText,
                            part.Attributes.GetNamedItem("colorable").InnerText == "1",
                            int.Parse(part.Attributes.GetNamedItem("index").InnerText)));
                }


                for (int j = 0; j < partList.Count; j++)
                {
                    var part = partList.Item(j);//.getChildNodes();

                    if (part.Name != "hiddenlayers")
                    {
                        continue;
                    }

                    var hiddenLayerList = part.ChildNodes;

                    for (int k = 0; k < hiddenLayerList.Count; k++)
                    {

                        var hiddenLayer = hiddenLayerList.Item(k);
                        figureSet.HiddenLayers.Add(hiddenLayer.Attributes.GetNamedItem("parttype").InnerText);
                    }
                }

                this.FigureSets.Add(id, figureSet);
            }
        }

        public void LoadFigurePalettes()
        {
            var xmlFile = FileUtil.SolveXmlFile("figuredata");
            var list = xmlFile.SelectNodes("//colors/palette");

            for (int i = 0; i < list.Count; i++)
            {
                var palette = list.Item(i);
                var colourList = palette.ChildNodes;

                var paletteId = int.Parse(palette.Attributes.GetNamedItem("id").InnerText);
                this.FigurePalettes.Add(paletteId, new List<FigureColor>());

                for (int k = 0; k < colourList.Count; k++)
                {
                    var colour = colourList.Item(k);

                    String colourId = colour.Attributes.GetNamedItem("id").InnerText;
                    String index = colour.Attributes.GetNamedItem("index").InnerText;
                    bool isClubRequired = colour.Attributes.GetNamedItem("club").InnerText == "1";
                    bool isSelectable = colour.Attributes.GetNamedItem("selectable").InnerText == "1";

                    this.FigurePalettes[paletteId].Add(new FigureColor(colourId, index, isClubRequired, isSelectable, colour.InnerText));
                }
            }
        }

        public void LoadFigureSetTypes()
        {
            var xmlFile = FileUtil.SolveXmlFile("figuredata");
            var list = xmlFile.SelectNodes("//settype");

            for (int i = 0; i < list.Count; i++)
            {
                var setType = list.Item(i);
                String set = setType.Attributes.GetNamedItem("type").InnerText;
                int paletteId = int.Parse(setType.Attributes.GetNamedItem("paletteid").InnerText);


                bool isMandatoryFlag = setType.Attributes.GetNamedItem("mandatory") != null;

                bool? isMandatory = null;
                bool? isMaleMandatoryNonHC = null;
                bool? isMaleMandatoryHC = null;
                bool? isFemaleMandatoryNonHC = null;
                bool? isFemaleMandatoryHC = null;

                if (isMandatoryFlag)
                {
                    isMandatory = setType.Attributes.GetNamedItem("mandatory") != null && setType.Attributes.GetNamedItem("mandatory")?.InnerText == "1";

                    if (isMandatory != null)
                    {
                        isMaleMandatoryHC = isMandatory;
                        isMaleMandatoryNonHC = isMandatory;
                        isFemaleMandatoryHC = isMandatory;
                        isFemaleMandatoryNonHC = isMandatory;
                    }
                }
                else
                {
                    isMaleMandatoryNonHC = setType.Attributes.GetNamedItem("mand_m_0") != null && setType.Attributes.GetNamedItem("mand_m_0")?.InnerText == "1";
                    isMaleMandatoryHC = setType.Attributes.GetNamedItem("mand_m_1") != null && setType.Attributes.GetNamedItem("mand_m_1")?.InnerText == "1";

                    isFemaleMandatoryNonHC = setType.Attributes.GetNamedItem("mand_f_0") != null && setType.Attributes.GetNamedItem("mand_f_0")?.InnerText == "1";
                    isFemaleMandatoryHC = setType.Attributes.GetNamedItem("mand_f_1") != null && setType.Attributes.GetNamedItem("mand_f_1")?.InnerText == "1";
                }

                this.FigureSetTypes.Add(set, new FigureSetType(set, paletteId, isMandatory, isMaleMandatoryNonHC, isMaleMandatoryHC, isFemaleMandatoryNonHC, isFemaleMandatoryHC));
            }
        }
    }
}
