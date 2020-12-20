namespace Avatara.Figure
{
    public class FigurePart
    {
        public string Id;
        public string Type;
        public bool Colorable;
        public int Index;

        public FigurePart(string id, string type, bool colorable, int index)
        {
            this.Id = id;
            this.Type = type;
            this.Colorable = colorable;
            this.Index = index;
        }
    }
}