using System.Collections.Generic;

namespace FSMViewer.Model
{
    public class FSMModel
    {
        public Dictionary<string, State> States { get; set; } = new();
        public Dictionary<string, Transition> Transitions { get; set; } = new();
        public Dictionary<string, Trigger> Triggers { get; set; } = new();
        public Dictionary<string, FSMAction> Actions { get; set; } = new();
        public State? InitialState { get; set; }
        public List<State> FinalStates { get; set; } = new();
        public State? CurrentState { get; set; }

        public void Reset()
        {
            CurrentState = InitialState;
        }
    }
}