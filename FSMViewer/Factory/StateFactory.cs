using System;
using FSMViewer.Model;

namespace FSMViewer.Factory
{
    public class StateFactory : IStateFactory
    {
        public State CreateState(string id, string name, StateType type)
        {
            return type switch
            {
                StateType.INITIAL => new InitialState(id, name),
                StateType.SIMPLE => new SimpleState(id, name),
                StateType.COMPOUND => new CompoundState(id, name),
                StateType.FINAL => new FinalState(id, name),
                _ => throw new ArgumentException($"Unknown state type: {type}")
            };
        }
    }
}