namespace grbl.Master.Service
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Reactive;
    using System.Reactive.Linq;
    using System.Reactive.Subjects;
    using System.Threading;
    using System.Windows.Media.Media3D;

    using grbl.Master.Common.Interfaces.Service;
    using grbl.Master.Model;
    using grbl.Master.Model.Enum;
    using grbl.Master.Model.GCode;

    using HelixToolkit.Wpf;

    using ICSharpCode.AvalonEdit.Document;

    public class GCodeFileService : IGCodeFileService
    {
        private readonly IGCodeParserService _gCodeParserService;

        private readonly Subject<Unit> _stopSubject = new Subject<Unit>();
        private readonly FileSystemWatcher _fileSystemWatcher = new FileSystemWatcher();
        //private Regex xRegex = new Regex(@"([Xx])((-?\d+)([,.]\d+)?|([,.]\d+))");
        //private Regex yRegex = new Regex(@"([Yy])((-?\d+)([,.]\d+)?|([,.]\d+))");
        //private Regex zRegex = new Regex(@"([Zz])((-?\d+)([,.]\d+)?|([,.]\d+))");

        public GCodeFile File { get; internal set; } = new GCodeFile();

        public GCodeFileService(IGCodeParserService gCodeParserService)
        {
            this._gCodeParserService = gCodeParserService;
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
                    FileDataLines = System.IO.File.ReadAllLines(path).ToList()
                };

                File.Path3DRepresentation.Add(new DefaultLights());

                Observable.Start(() =>
                    {
                        var lastFrame = new GCodeFrame
                                            {
                                                MovementType = MovementType.Rapid,
                                                Points = new List<Point3D> { new Point3D(0, 0, 0) }
                                            };

                        var pointsArray = new List<Point3D>();

                        foreach (var documentLine in File.FileDataLines)
                        {
                            if (_gCodeParserService.GCodeLineToFrame(documentLine, lastFrame, out var newFrame))
                            {
                                lastFrame = newFrame;

                                pointsArray.AddRange(newFrame.Points);
                            }
                            
                            //var newPoint = GCodeLineTo3DPath(documentLine, lastPoint);
                            //if (!newPoint.Equals(lastPoint))
                            //{
                            //    pointsArray.Add(lastPoint);
                            //    pointsArray.Add(newPoint);
                            //    lastPoint = newPoint;
                            //}
                        }

                        return pointsArray;

                    }).TakeUntil(_stopSubject).ObserveOn(SynchronizationContext.Current).Subscribe(x =>
                    {
                        var line = new LinesVisual3D { Points = new Point3DCollection(x) };
                        this.File.Path3DRepresentation.Add(line);
                    });

                _fileSystemWatcher.Path = Path.GetDirectoryName(path);
                _fileSystemWatcher.EnableRaisingEvents = true;
            }
        }

        //private Point3D GCodeLineTo3DPath(string gcode, Point3D currentPoint)
        //{
        //    currentPoint.X = GetDoubleByRegex(gcode, xRegex, new[] { 'x', 'X' }, currentPoint.X);
        //    currentPoint.Y = GetDoubleByRegex(gcode, yRegex, new[] { 'y', 'Y' }, currentPoint.Y);
        //    currentPoint.Z = GetDoubleByRegex(gcode, zRegex, new[] { 'z', 'Z' }, currentPoint.Z);


        //    return currentPoint;
        //}

        //private double GetDoubleByRegex(string gcode, Regex reg, char[] excessiveChars, double fallbackValue)
        //{
        //    foreach (Match match in reg.Matches(gcode))
        //    {
        //        var numericValue = match.Value.TrimStart(excessiveChars);
        //        numericValue = numericValue
        //            .Replace('.', CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator[0]).Replace(
        //                ',',
        //                CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator[0]);
        //        if (double.TryParse(numericValue, out var result))
        //        {
        //            return result;
        //        }
        //    }

        //    return fallbackValue;
        //}

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
