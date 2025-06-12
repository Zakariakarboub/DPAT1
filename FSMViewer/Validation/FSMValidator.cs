using System.Collections.Generic;
using System.Linq;
using FSMViewer.Model;

namespace FSMViewer.Validation
{
    public class FSMValidator : IValidator
    {
        public List<string> Validate(FSMModel model)
        {
            var errors = new List<string>();

            errors.AddRange(ValidateInitialState(model));
            errors.AddRange(ValidateFinalStates(model));
            errors.AddRange(ValidateNonDeterminism(model));
            errors.AddRange(ValidateReachability(model));

            return errors;
        }

        private List<string> ValidateInitialState(FSMModel model)
        {
            var errors = new List<string>();

            if (model.InitialState == null)
            {
                errors.Add("No initial state defined");
                return errors;
            }

            var incomingTransitions = model.Transitions.Values
                .Where(t => t.Target.Id == model.InitialState.Id)
                .ToList();

            if (incomingTransitions.Any())
            {
                errors.Add($"Initial state '{model.InitialState.Id}' has incoming transitions");
            }

            return errors;
        }

        private List<string> ValidateFinalStates(FSMModel model)
        {
            var errors = new List<string>();

            foreach (var finalState in model.FinalStates)
            {
                var outgoingTransitions = model.Transitions.Values
                    .Where(t => t.Source.Id == finalState.Id)
                    .ToList();

                if (outgoingTransitions.Any())
                {
                    errors.Add($"Final state '{finalState.Id}' has outgoing transitions");
                }
            }

            return errors;
        }

        private List<string> ValidateNonDeterminism(FSMModel model)
        {
            var errors = new List<string>();

            var transitionGroups = model.Transitions.Values
                .GroupBy(t => new { SourceId = t.Source.Id, TriggerId = t.Trigger.Id });

            foreach (var group in transitionGroups)
            {
                if (group.Count() > 1)
                {
                    var conflictingTransitions = group.Select(t => t.Id).ToList();
                    errors.Add($"Non-deterministic transitions from state '{group.Key.SourceId}' with trigger '{group.Key.TriggerId}': {string.Join(", ", conflictingTransitions)}");
                }
            }

            return errors;
        }

        private List<string> ValidateReachability(FSMModel model)
        {
            var errors = new List<string>();

            if (model.InitialState == null) return errors;

            var reachableStates = new HashSet<string>();
            var queue = new Queue<State>();
            queue.Enqueue(model.InitialState);
            reachableStates.Add(model.InitialState.Id);

            while (queue.Count > 0)
            {
                var currentState = queue.Dequeue();
                var outgoingTransitions = model.Transitions.Values
                    .Where(t => t.Source.Id == currentState.Id);

                foreach (var transition in outgoingTransitions)
                {
                    if (!reachableStates.Contains(transition.Target.Id))
                    {
                        reachableStates.Add(transition.Target.Id);
                        queue.Enqueue(transition.Target);
                    }
                }
            }

            var unreachableStates = model.States.Keys.Except(reachableStates).ToList();
            foreach (var stateId in unreachableStates)
            {
                errors.Add($"State '{stateId}' is not reachable from initial state");
            }

            return errors;
        }
    }
}