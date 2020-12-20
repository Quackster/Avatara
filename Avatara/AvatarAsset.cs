namespace Avatara
{
    internal class AvatarAsset
    {
        public string Name;
        public int X;
        public int Y;
        public string FileName;

        public AvatarAsset(string name, string fileName, int X, int Y)
        {
            this.Name = name;
            this.X = X;
            this.Y = Y;
            this.FileName = fileName;
        }
    }
}