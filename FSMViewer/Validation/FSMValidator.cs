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

            if (model.InitialState == null)
                return errors;

            // BFS vanuit initial, maar zorg dat ook ouders en via ouders bereikbare toestanden meedoen
            var reachable = new HashSet<string>();
            var queue = new Queue<State>();

            void MarkReachable(State s)
            {
                if (s == null || reachable.Contains(s.Id))
                    return;
                reachable.Add(s.Id);
                queue.Enqueue(s);
                // Ook alle ouders markeren
                var p = s.Parent;
                while (p != null)
                {
                    if (reachable.Add(p.Id))
                        queue.Enqueue(p);
                    p = p.Parent;
                }
            }

            // start bij initial
            MarkReachable(model.InitialState);

            while (queue.Count > 0)
            {
                var current = queue.Dequeue();
                // alle uitgaande transitions
                var outs = model.Transitions.Values
                    .Where(t => t.Source.Id == current.Id);

                foreach (var t in outs)
                {
                    MarkReachable(t.Target);
                }
            }

            // onbereikbare staten
            var unreachable = model.States.Values
                .Where(s => !reachable.Contains(s.Id))
                .Select(s => s.Id)
                .ToList();

            foreach (var id in unreachable)
                errors.Add($"State '{id}' is not reachable from initial state");

            return errors;
        }
    }
}
