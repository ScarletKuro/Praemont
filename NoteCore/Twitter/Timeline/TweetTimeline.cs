using NoteCore.Model;

namespace NoteCore.Twitter.Timeline
{
    public class TweetTimeline
    {
        private readonly Timelines _tweeterTimelines;

        public TweetTimeline()
        {
            //InitializeComponent();
            _tweeterTimelines = new Timelines();
            Controller = new TimelineController(_tweeterTimelines);
            Controller.StartTimelines();
        }

        public TimelineController Controller { get; }
        public Timelines GeTimelines { get { return _tweeterTimelines; } }
    }
}