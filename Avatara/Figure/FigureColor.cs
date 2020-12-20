namespace Avatara.Figure
{
    public class FigureColor
    {
        public string ColourId;
        public string Index;
        public bool IsClubRequired;
        public bool IsSelectable;

        public FigureColor(string colourId, string index, bool isClubRequired, bool isSelectable)
        {
            this.ColourId = colourId;
            this.Index = index;
            this.IsClubRequired = isClubRequired;
            this.IsSelectable = isSelectable;
        }
    }
}