using System;
using System.Collections.Generic;
using System.Linq;
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

            var transition = new Transition(id,
                                           _model.States[sourceId],
                                           _model.States[targetId],
                                           _model.Triggers[triggerId],
                                           guard);
            _model.Transitions[id] = transition;
            return this;
        }

        public IFSMBuilder AddEntryAction(string stateId, string actionId)
        {
            if (_model.States.ContainsKey(stateId) && _model.Actions.ContainsKey(actionId))
                _model.States[stateId].EntryActions.Add(_model.Actions[actionId]);
            return this;
        }

        public IFSMBuilder AddExitAction(string stateId, string actionId)
        {
            if (_model.States.ContainsKey(stateId) && _model.Actions.ContainsKey(actionId))
                _model.States[stateId].ExitActions.Add(_model.Actions[actionId]);
            return this;
        }

        public FSMModel Build()
        {
            // 1) Fallback initial state als er geen is
            if (_model.InitialState == null && _model.States.Count > 0)
            {
                var fallback = _model.States.Values.First();
                _model.InitialState = fallback;
                _model.CurrentState = fallback;
                Console.WriteLine($"[Builder warning] Geen initial state gevonden; startend met '{fallback.Id}'.");
            }

            // 2) **Injecteer** alle 'power_on'-transitions van initial ook op de 'powered'-compound
            //    zónder dat je .fsm-bestanden hoeft aan te passen.
            InjectPowerOnToCompound();

            return _model;
        }

        private void InjectPowerOnToCompound()
        {
            // Zoek de compound state met id "powered" (of je kunt hier op StateType.COMPOUND filteren)
            var compound = _model.States.Values.FirstOrDefault(s => s.Type == StateType.COMPOUND && s.Id == "powered");
            if (compound == null)
                return;

            // Vind alle transitions uit initial met trigger "power_on"
            if (_model.InitialState == null)
                return;

            var initTransitions = _model.Transitions.Values
                .Where(t => t.Source.Id == _model.InitialState.Id
                            && t.Trigger.Id.Equals("power_on", StringComparison.OrdinalIgnoreCase))
                .ToList();

            foreach (var t in initTransitions)
            {
                // Kijk of we die al niet hebben
                var exists = _model.Transitions.Values.Any(x =>
                    x.Source.Id == compound.Id &&
                    x.Target.Id == t.Target.Id &&
                    x.Trigger.Id == t.Trigger.Id);

                if (exists)
                    continue;

                // Kloneer de transition onder een nieuw id
                var newId = $"{t.Id}_to_{compound.Id}";
                var clone = new Transition(newId, compound, t.Target, t.Trigger, t.Guard);
                _model.Transitions[newId] = clone;
                Console.WriteLine($"[Builder info] Geïnjecteerde transition '{newId}' from '{compound.Id}' via '{t.Trigger.Id}' to '{t.Target.Id}'.");
            }
        }
    }
}
