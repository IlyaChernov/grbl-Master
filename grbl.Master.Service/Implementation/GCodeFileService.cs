namespace grbl.Master.Service.Implementation
{
    using grbl.Master.Model;
    using grbl.Master.Model.Enum;
    using grbl.Master.Service.Interface;
    using ICSharpCode.AvalonEdit.Document;
    using System.IO;

    public class GCodeFileService : IGCodeFileService
    {
        private readonly FileSystemWatcher _fileSystemWatcher = new FileSystemWatcher();
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
            if (System.IO.File.Exists(path))
            {
                File = new GCodeFile
                {
                    FilePath = path,
                    FileState = FileState.Unchanged,
                    FileData = new TextDocument(System.IO.File.ReadAllText(path))
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
