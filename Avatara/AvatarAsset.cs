namespace Avatara
{
    internal class AvatarAsset
    {
        public string Name;
        public int X;
        public int Y;
        public int ImageX;
        public int ImageY;
        public string FileName;

        public AvatarAsset(string name, string fileName, int X, int Y, int canvasW, int canvasH)
        {
            this.Name = name;
            this.X = X;
            this.Y = Y;
            this.FileName = fileName;


            ImageX = X + (canvasW / 2);// 32;
            ImageY = Y + (canvasH / 2);// 25;
        }
    }
}