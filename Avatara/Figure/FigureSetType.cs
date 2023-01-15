namespace Avatara.Figure
{
    public class FigureSetType
    {
        public string Set;
        public int PaletteId;
        public bool? IsMandatory;
        public bool? IsMaleMandatoryNonHC;
        public bool? IsMaleMandatoryHC;
        public bool? IsFemaleMandatoryNonHC;
        public bool? IsFemaleMandatoryHC;

        public FigureSetType(string set, int paletteId, bool? isMandatory, bool? isMaleMandatoryNonHC, bool? isMaleMandatoryHC, bool? isFemaleMandatoryNonHC, bool? isFemaleMandatoryHC)
        {
            this.Set = set;
            this.PaletteId = paletteId;
            this.IsMandatory = isMandatory;
            this.IsMaleMandatoryNonHC = isMaleMandatoryNonHC;
            this.IsMaleMandatoryHC = isMaleMandatoryHC;
            this.IsFemaleMandatoryNonHC = isFemaleMandatoryNonHC;
            this.IsFemaleMandatoryNonHC = isFemaleMandatoryHC;
        }
    }
}