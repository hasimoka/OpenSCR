using Prism.Events;

namespace VideoViews.Views
{
    public class Messenger : EventAggregator
    {
        public static Messenger Instance { get; } = new Messenger();
    }
}
