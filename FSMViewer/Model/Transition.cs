using System.Collections.Generic;

namespace FSMViewer.Model
{
    public class Transition
    {
        public string Id { get; set; }
        public State Source { get; set; }
        public State Target { get; set; }
        public Trigger Trigger { get; set; }
        public string Guard { get; set; }
        public List<FSMAction> Actions { get; set; } = new();

        public Transition(string id, State source, State target, Trigger trigger, string guard = "")
        {
            Id = id;
            Source = source;
            Target = target;
            Trigger = trigger;
            Guard = guard;
        }
    }
}