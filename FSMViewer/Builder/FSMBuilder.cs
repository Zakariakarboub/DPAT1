using System;
using FSMViewer.Model;
using FSMViewer.Factory;

namespace FSMViewer.Builder
{
    public class FSMBuilder : IFSMBuilder
    {
        private readonly FSMModel _model = new();
        private readonly IStateFactory _stateFactory;

        public FSMBuilder(IStateFactory stateFactory)
        {
            _stateFactory = stateFactory;
        }

        public IFSMBuilder AddState(string id, string name, StateType type, string? parentId = null)
        {
            var state = _stateFactory.CreateState(id, name, type);
            _model.States[id] = state;

            if (type == StateType.INITIAL)
            {
                _model.InitialState = state;
                _model.CurrentState = state;
            }
            else if (type == StateType.FINAL)
            {
                _model.FinalStates.Add(state);
            }

            if (!string.IsNullOrEmpty(parentId) && _model.States.ContainsKey(parentId))
            {
                var parent = _model.States[parentId];
                state.Parent = parent;
                parent.Children.Add(state);
            }

            return this;
        }

        public IFSMBuilder AddTrigger(string id, string name)
        {
            _model.Triggers[id] = new Trigger(id, name);
            return this;
        }

        public IFSMBuilder AddAction(string id, string name, ActionType type)
        {
            _model.Actions[id] = new FSMAction(id, name, type);
            return this;
        }

        public IFSMBuilder AddTransition(string id, string sourceId, string targetId, string triggerId, string guard = "")
        {
            if (!_model.States.ContainsKey(sourceId) ||
                !_model.States.ContainsKey(targetId) ||
                !_model.Triggers.ContainsKey(triggerId))
            {
                throw new ArgumentException("Invalid transition: source, target, or trigger not found");
            }

            var transition = new Transition(id, _model.States[sourceId], _model.States[targetId], _model.Triggers[triggerId], guard);
            _model.Transitions[id] = transition;
            return this;
        }

        public IFSMBuilder AddEntryAction(string stateId, string actionId)
        {
            if (_model.States.ContainsKey(stateId) && _model.Actions.ContainsKey(actionId))
            {
                _model.States[stateId].EntryActions.Add(_model.Actions[actionId]);
            }
            return this;
        }

        public IFSMBuilder AddExitAction(string stateId, string actionId)
        {
            if (_model.States.ContainsKey(stateId) && _model.Actions.ContainsKey(actionId))
            {
                _model.States[stateId].ExitActions.Add(_model.Actions[actionId]);
            }
            return this;
        }

        public FSMModel Build()
        {
            return _model;
        }
    }
}
