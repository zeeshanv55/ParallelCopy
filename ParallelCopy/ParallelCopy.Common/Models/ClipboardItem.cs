namespace ParallelCopy.Common.Models
{
    using System;
    using System.Collections.Specialized;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Windows;
    using System.Windows.Media.Imaging;
    using Enums;

    public class ClipboardItem
    {
        public bool Exists { get; set; }

        public ClipboardItemType Type { get; set; }

        public string Description1 { get; set; }

        public string Description2 { get; set; }

        public object Data { get; set; }

        public ClipboardItem()
        {
            this.Read();
        }

        public void Read()
        {
            if (Any())
            {
                this.Exists = true;

                if (Clipboard.ContainsText())
                {
                    var text = Clipboard.GetText();
                    Type = ClipboardItemType.Text;
                    Description1 = text.Replace("\r\n", "...");
                    Description2 = string.Empty;
                    Data = text;
                }
                else if (Clipboard.ContainsImage())
                {
                    var image = Clipboard.GetImage();
                    Type = ClipboardItemType.Image;
                    Description1 = $"{image.Width}x{image.Height}";
                    Description2 = string.Empty;
                    Data = image;
                }
                else if (Clipboard.ContainsFileDropList())
                {
                    var stringCollection = Clipboard.GetFileDropList();
                    var stringArray = new string[stringCollection.Count];
                    stringCollection.CopyTo(stringArray, 0);

                    var commonPrefix = string.Join(
                        "\\",
                        stringArray.Select(s => s.Split('\\').AsEnumerable())
                        .Transpose()
                        .TakeWhile(s => s.All(d => d == s.First()))
                        .Select(s => s.First()));

                    Type = ClipboardItemType.FileFolder;
                    Description1 = $"{stringCollection.Count} Items at {commonPrefix}";
                    Description2 = string.Join(", ", stringArray.Select(s => s.Replace(commonPrefix + "\\", "")));
                    Data = stringCollection;
                }
                else if (Clipboard.ContainsAudio())
                {
                    Type = ClipboardItemType.Audio;
                    Description1 = string.Empty;
                    Description2 = string.Empty;
                    Data = Clipboard.GetAudioStream();
                }
                else
                {
                    Type = ClipboardItemType.Data;
                    Description1 = string.Empty;
                    Description2 = string.Empty;
                    Data = Clipboard.GetDataObject();
                }
            }
            else
            {
                this.Exists = false;
            }
        }

        public void Write()
        {
            switch (this.Type)
            {
                case ClipboardItemType.Text:
                    Clipboard.SetText((string)this.Data);
                    break;

                case ClipboardItemType.FileFolder:
                    Clipboard.SetFileDropList((StringCollection)this.Data);
                    break;

                case ClipboardItemType.Image:
                    Clipboard.SetImage((BitmapSource)this.Data);
                    break;

                case ClipboardItemType.Audio:
                    Clipboard.SetAudio((Stream)this.Data);
                    break;
            }
        }

        private static bool Any()
        {
            var dataFormats = typeof(DataFormats).GetFields(BindingFlags.Public | BindingFlags.Static).Select(f => f.Name);
            return dataFormats.Any(x => Clipboard.ContainsData(x));
        }

        public override bool Equals(object obj)
        {
            if (!(obj is ClipboardItem))
            {
                return false;
            }

            var objectToCompare = (ClipboardItem)obj;

            switch (Type)
            {
                case ClipboardItemType.Text:
                    return objectToCompare.Type == ClipboardItemType.Text && Convert.ToString(this.Data) == Convert.ToString(objectToCompare.Data);

                case ClipboardItemType.FileFolder:
                    if (objectToCompare.Type != ClipboardItemType.FileFolder)
                    {
                        return false;
                    }

                    var currentArray = new string[((StringCollection)this.Data).Count];
                    var arrayToCompare = new string[((StringCollection)objectToCompare.Data).Count];
                    ((StringCollection)this.Data).CopyTo(currentArray, 0);
                    ((StringCollection)objectToCompare.Data).CopyTo(arrayToCompare, 0);

                    return currentArray.SequenceEqual(arrayToCompare);
            }

            return false;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }
}