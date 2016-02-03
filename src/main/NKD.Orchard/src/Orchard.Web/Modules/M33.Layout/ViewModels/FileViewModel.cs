using System.Collections.Generic;

namespace M33.Layout.ViewModels
{
    public class FileViewModel
    {
        public string Content { get; set; }
        public string FileName { get; set; }
        public IEnumerable<FileModel> Files { get; set; }
        public FileType FileType { get; set; } 
    }

    public class FileModel
    {
        public string Name { get; set; }
        public string Path { get; set; }
        public bool Current { get; set; }
    }

    public class AddFile
    {
        public string TypeOfFile { get; set; }
        public string FileName { get; set; }
    }

    public class Settings
    {
        public string Description { get; set; }
        public string Placement { get; set; }
        public string ThemeImage { get; set; }
        public string ThemeZoneImage { get; set; }
        
    }

    public enum FileType{
        CSS,
        RAZOR,
        JS
    }
}