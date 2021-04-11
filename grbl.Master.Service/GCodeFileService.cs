namespace grbl.Master.Service
{
    using grbl.Master.Common.Interfaces.Service;
    using grbl.Master.Model;
    using grbl.Master.Model.Enum;
    using ICSharpCode.AvalonEdit.Document;
    using System.IO;
    using System.Reactive;
    using System.Reactive.Subjects;

    public class GCodeFileService : IGCodeFileService
    {
        private readonly Subject<Unit> _stopSubject = new Subject<Unit>();
        private readonly FileSystemWatcher _fileSystemWatcher = new FileSystemWatcher();
        //private Regex xRegex = new Regex(@"([Xx])((-?\d+)([,.]\d+)?|([,.]\d+))");
        //private Regex yRegex = new Regex(@"([Yy])((-?\d+)([,.]\d+)?|([,.]\d+))");
        //private Regex zRegex = new Regex(@"([Zz])((-?\d+)([,.]\d+)?|([,.]\d+))");

        public GCodeFile File { get; internal set; } = new GCodeFile();

        public GCodeFileService()
        {
            _fileSystemWatcher.Deleted += FileSystemWatcherDeleted;
            _fileSystemWatcher.Created += FileSystemWatcherCreated;
            _fileSystemWatcher.Renamed += FileSystemWatcherRenamed;
            _fileSystemWatcher.Changed += FileSystemWatcherChanged;
        }

        public void Load(string path)
        {
            this._stopSubject.OnNext(Unit.Default);

            if (System.IO.File.Exists(path))
            {
                File = new GCodeFile
                {
                    FilePath = path,
                    FileState = FileState.Unchanged,
                    FileData = new TextDocument(System.IO.File.ReadAllText(path)),
                };

                _fileSystemWatcher.Path = Path.GetDirectoryName(path);
                _fileSystemWatcher.EnableRaisingEvents = true;
            }
        }

        private void FileSystemWatcherRenamed(object sender, RenamedEventArgs e)
        {
            if (e.OldName == Path.GetFileName(File.FilePath))
            {
                File.FileState = FileState.Deleted;
            }
        }

        private void FileSystemWatcherCreated(object sender, FileSystemEventArgs e)
        {
            if (e.Name == Path.GetFileName(File.FilePath))
            {
                File.FileState = FileState.Updated;
            }
        }

        private void FileSystemWatcherDeleted(object sender, FileSystemEventArgs e)
        {
            if (e.Name == Path.GetFileName(File.FilePath))
            {
                File.FileState = FileState.Deleted;
            }
        }

        private void FileSystemWatcherChanged(object sender, FileSystemEventArgs e)
        {
            if (e.Name == Path.GetFileName(File.FilePath))
            {
                File.FileState = FileState.Updated;
            }
        }
    }
}
