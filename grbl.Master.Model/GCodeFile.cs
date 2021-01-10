namespace grbl.Master.Model
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Windows.Media.Media3D;

    using grbl.Master.Model.Enum;

    using ICSharpCode.AvalonEdit.Document;

    public class GCodeFile : NotifyPropertyChanged
    {
        public string FilePath { get; set; }

        public FileState FileState { get; set; }

        public TextDocument FileData { get; set; } = new TextDocument();

        public List<string> FileDataLines { get; set; } = new List<string>();

        public ObservableCollection<Visual3D> Path3DRepresentation { get; set; } = new ObservableCollection<Visual3D>();
    }
}
