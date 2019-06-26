namespace ParallelCopy.Common.Enums
{
    using System.ComponentModel;

    public enum ClipboardItemType
    {
        [Description("Text")]
        Text,

        [Description("Image")]
        Image,

        [Description("Files/Folders")]
        FileFolder,

        [Description("Audio")]
        Audio,

        [Description("Data")]
        Data
    }
}