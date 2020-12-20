using System;

namespace Avatara.Figure
{
    public class FigurePart
    {
        public string Id;
        public string Type;
        public bool Colorable;
        public int Index;
        public int OrderId;

        public FigurePart(string id, string type, bool colorable, int index)
        {
            this.Id = id;
            this.Type = type;
            this.Colorable = colorable;
            this.Index = index;
            this.OrderId = GetOrder();

        }

        private int GetOrder()
        {
            switch (Type)
            {
                case "sh":
                    return 5;
                    break;
                case "lg":
                    return 6;
                    break;
                case "ch":
                    return 7;
                    break;
                case "wa":
                    return 8;
                    break;
                case "ca":
                    return 9;
                    break;
                case "fa":
                    return 27;
                    break;
                case "ea":
                    return 28;
                    break;
                case "ha":
                    return 29;
                    break;
                case "he":
                    return 20;
                    break;
                case "cc":
                    return 21;
                    break;
                case "cp":
                    return 6;
                    break;
                case "hd":
                    return 22;
                    break;
                case "bd":
                    return 1;
                    break;
                case "fc":
                    return 23;
                    break;
                case "hr":
                    return 24;
                    break;
                case "lh":
                    return 5;
                    break;
                case "ls":
                    return 6;
                    break;
                case "rh":
                    return 10;
                    break;
                case "rs":
                    return 11;
                    break;
                case "ey":
                    return 24;
                    break;
                case "li":
                    return 0;
                    break;
                case "hrb":
                    return 26;
                    break;
                case "ri":
                    return 26;
                    break;
                case "lc":
                    return 23;
                    break;
                case "rc":
                    return 24;
                    break;
                case "fx":
                    return 100;
                    break;
            }

            return -1;
        }
    }
}