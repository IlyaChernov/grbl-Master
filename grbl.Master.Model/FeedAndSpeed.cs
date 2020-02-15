namespace grbl.Master.Model
{
    public class FeedAndSpeed : NotifyPropertyChanged
    {
        public long FeedRate { get; set; }

        public long ToolNumber { get; set; }

        public long SpindleSpeed { get; set; }
    }
}
