namespace grbl.Master.Model
{
    using grbl.Master.Model.Enum;

    using ICSharpCode.AvalonEdit.Document;

    public class GCodeFile : NotifyPropertyChanged
    {
        public string FilePath { get; set; }

        public FileState FileState { get; set; }

        public TextDocument FileData { get; set; } = new TextDocument();
    }
}
