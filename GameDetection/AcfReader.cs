namespace GameDetection
{
    public class AcfReader
    {
        public string FileLocation { get; private set; }

        public AcfReader(string FileLocation)
        {
            if (File.Exists(FileLocation))
                this.FileLocation = FileLocation;
            else
                throw new FileNotFoundException("Error", FileLocation);
        }

        public bool CheckIntegrity()
        {
            string Content = File.ReadAllText(FileLocation);
            int quote = Content.Count(x => x == '"');
            int braceleft = Content.Count(x => x == '{');
            int braceright = Content.Count(x => x == '}');

            return ((braceleft == braceright) && (quote % 2 == 0));
        }

        public ACF_Struct ACFFileToStruct()
        {
            return ACFFileToStruct(File.ReadAllText(FileLocation));
        }

        private ACF_Struct ACFFileToStruct(string RegionToReadIn)
        {
            ACF_Struct ACF = new ACF_Struct();
            int LengthOfRegion = RegionToReadIn.Length;
            int CurrentPos = 0;
            while (LengthOfRegion > CurrentPos)
            {
                int FirstItemStart = RegionToReadIn.IndexOf('"', CurrentPos);
                if (FirstItemStart == -1)
                    break;
                int FirstItemEnd = RegionToReadIn.IndexOf('"', FirstItemStart + 1);
                CurrentPos = FirstItemEnd + 1;
                string FirstItem = RegionToReadIn.Substring(FirstItemStart + 1, FirstItemEnd - FirstItemStart - 1);

                int SecondItemStartQuote = RegionToReadIn.IndexOf('"', CurrentPos);
                int SecondItemStartBraceleft = RegionToReadIn.IndexOf('{', CurrentPos);
                if (SecondItemStartBraceleft == -1 || SecondItemStartQuote < SecondItemStartBraceleft)
                {
                    int SecondItemEndQuote = RegionToReadIn.IndexOf('"', SecondItemStartQuote + 1);
                    string SecondItem = RegionToReadIn.Substring(SecondItemStartQuote + 1, SecondItemEndQuote - SecondItemStartQuote - 1);
                    CurrentPos = SecondItemEndQuote + 1;
                    ACF.SubItems.Add(FirstItem, SecondItem);
                }
                else
                {
                    int SecondItemEndBraceright = RegionToReadIn.NextEndOf('{', '}', SecondItemStartBraceleft + 1);
                    ACF_Struct ACFS = ACFFileToStruct(RegionToReadIn.Substring(SecondItemStartBraceleft + 1, SecondItemEndBraceright - SecondItemStartBraceleft - 1));
                    CurrentPos = SecondItemEndBraceright + 1;
                    ACF.SubACF.Add(FirstItem, ACFS);
                }
            }

            return ACF;
        }
    }

    internal static class Extension
    {
        public static int NextEndOf(this string str, char Open, char Close, int startIndex)
        {
            if (Open == Close)
                throw new Exception("\"Open\" and \"Close\" char are equivalent!");

            int OpenItem = 0;
            int CloseItem = 0;
            for (int i = startIndex; i < str.Length; i++)
            {
                if (str[i] == Open)
                {
                    OpenItem++;
                }
                if (str[i] == Close)
                {
                    CloseItem++;
                    if (CloseItem > OpenItem)
                        return i;
                }
            }
            throw new Exception("Not enough closing characters!");
        }
    }
}