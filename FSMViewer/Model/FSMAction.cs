namespace FSMViewer.Model
{
    public enum ActionType
    {
        ENTRY_ACTION, EXIT_ACTION, TRANSITION_ACTION
    }

    public class FSMAction
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public ActionType Type { get; set; }

        public FSMAction(string id, string name, ActionType type)
        {
            Id = id;
            Name = name;
            Type = type;
        }
    }
}