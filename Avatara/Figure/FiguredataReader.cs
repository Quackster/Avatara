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
        public Dictionary<int, List<FigureColor>> FigurePalettes;
        public Dictionary<string, List<FigureSetType>> FigureSetTypes;
        public Dictionary<string, List<FigureSet>> FigureSets;

        public FiguredataReader()
        {
            this.FigurePalettes = new Dictionary<int, List<FigureColor>>();
            this.FigureSetTypes = new Dictionary<string, List<FigureSetType>> ();
            this.FigureSets = new Dictionary<string, List<FigureSet>>();

        }

        public void LoadFigureSets(string path)
        {
            var xmlFile = FileUtil.SolveXmlFile(path, "figuredata");
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

                    for (int k = 0; k < hiddenLayerList.Count; k++) {

                        var hiddenLayer = hiddenLayerList.Item(k);
                        figureSet.HiddenLayers.Add(hiddenLayer.Attributes.GetNamedItem("parttype").InnerText);
                    }
                }

                if (!this.FigureSets.ContainsKey(id))
                    this.FigureSets.Add(id, new List<FigureSet>());

                this.FigureSets[id].Add(figureSet);
            }
        }

        public void LoadFigurePalettes(string path)
        {
            var xmlFile = FileUtil.SolveXmlFile(path, "figuredata");
            var list = xmlFile.SelectNodes("//colors/palette");

            for (int i = 0; i < list.Count; i++)
            {
                var palette = list.Item(i);
                var colourList = palette.ChildNodes;

                var paletteId = int.Parse(palette.Attributes.GetNamedItem("id").InnerText);

                if (!this.FigurePalettes.ContainsKey(paletteId))
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

        public void loadFigureSetTypes(string path)
        {
            var xmlFile = FileUtil.SolveXmlFile(path, "figuredata");
            var list = xmlFile.SelectNodes("//settype");

            for (int i = 0; i < list.Count; i++)
            {
                var setType = list.Item(i);
                String set = setType.Attributes.GetNamedItem("type").InnerText;
                int paletteId = int.Parse(setType.Attributes.GetNamedItem("paletteid").InnerText);
                bool isMandatory = false;// setType.Attributes.GetNamedItem("mandatory").InnerText == "1";

                if (!this.FigureSetTypes.ContainsKey(set))
                    this.FigureSetTypes.Add(set, new List<FigureSetType>());

                this.FigureSetTypes[set].Add(new FigureSetType(set, paletteId, isMandatory));
            }
        }
    }
}
