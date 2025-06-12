using System.Collections.Generic;
using FSMViewer.Renderer;

namespace FSMViewer.Model
{
    public enum StateType
    {
        INITIAL, SIMPLE, COMPOUND, FINAL
    }

    public abstract class State
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public StateType Type { get; set; }
        public State? Parent { get; set; }
        public List<State> Children { get; set; } = new();
        public List<FSMAction> EntryActions { get; set; } = new();
        public List<FSMAction> ExitActions { get; set; } = new();

        protected State(string id, string name, StateType type)
        {
            Id = id;
            Name = name;
            Type = type;
        }

        public abstract void Accept(IStateVisitor visitor);
        public abstract State Clone();
    }
}