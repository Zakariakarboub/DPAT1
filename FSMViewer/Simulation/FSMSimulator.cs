using System;
using System.Linq;
using System.Collections.Generic;
using FSMViewer.Model;
using FSMViewer.Renderer;

namespace FSMViewer.Simulation
{
    public class FSMSimulator
    {
        private readonly FSMModel _model;
        private readonly IRenderer _renderer;

        public FSMSimulator(FSMModel model, IRenderer renderer)
        {
            _model = model ?? throw new ArgumentNullException(nameof(model));
            _renderer = renderer ?? throw new ArgumentNullException(nameof(renderer));
        }

        public void Run()
        {
            _model.Reset();
            if (_model.CurrentState is null)
                throw new InvalidOperationException("CurrentState mag na Reset niet null zijn.");

            Console.WriteLine("=== FSM SIMULATOR ===");
            Console.WriteLine("Type 'quit' to exit, 'show' to render, 'reset' to restart");
            Console.WriteLine();

            while (true)
            {
                DisplayCurrentState();
                Console.Write("Enter trigger: ");
                var raw = Console.ReadLine()?.Trim();
                if (string.IsNullOrEmpty(raw))
                {
                    Console.WriteLine();
                    continue;
                }
                var input = raw.ToLowerInvariant();

                if (input == "quit") break;
                if (input == "show")
                {
                    Console.WriteLine(_renderer.Render(_model));
                    continue;
                }
                if (input == "reset")
                {
                    _model.Reset();
                    Console.WriteLine("FSM reset to initial state");
                    continue;
                }

                ProcessTrigger(raw);
                Console.WriteLine();
            }
        }

        private void ProcessTrigger(string rawInput)
        {
            var input = rawInput.ToLowerInvariant();
            // Zoek alle mogelijke overgangen (incl. parent lookup)
            var candidates = FindTransitionsHierarchical(input).ToList();

            if (!candidates.Any())
            {
                Console.WriteLine($"No transition found for trigger '{rawInput}' from state '{_model.CurrentState!.Id}'");
                SuggestAvailableTriggers();
                return;
            }

            Transition chosen;
            if (candidates.Count == 1)
            {
                chosen = candidates[0];
            }
            else
            {
                // Laat de gebruiker kiezen
                Console.WriteLine($"Meerdere overgangen gevonden voor trigger '{rawInput}':");
                for (int i = 0; i < candidates.Count; i++)
                {
                    var t = candidates[i];
                    var guard = string.IsNullOrEmpty(t.Guard) ? "<geen guard>" : t.Guard;
                    Console.WriteLine($"  [{i + 1}] {t.Source.Id} -> {t.Target.Id} [guard: {guard}]");
                }
                Console.Write("Kies nummer: ");
                var sel = Console.ReadLine();
                if (!int.TryParse(sel, out var idx) || idx < 1 || idx > candidates.Count)
                {
                    Console.WriteLine("Ongeldige keuze, overgang afgebroken.");
                    return;
                }
                chosen = candidates[idx - 1];
            }

            ExecuteTransition(chosen);
        }

        private IEnumerable<Transition> FindTransitionsHierarchical(string input)
        {
            State? s = _model.CurrentState;
            while (s != null)
            {
                var matches = _model.Transitions.Values
                    .Where(t =>
                        t.Source.Id == s.Id &&
                        (t.Trigger.Id.Equals(input, StringComparison.OrdinalIgnoreCase) ||
                         t.Trigger.Name.Equals(input, StringComparison.OrdinalIgnoreCase)));
                if (matches.Any())
                    return matches;
                s = s.Parent;
            }
            return Enumerable.Empty<Transition>();
        }

        private void DisplayCurrentState()
        {
            var cur = _model.CurrentState
                      ?? throw new InvalidOperationException("CurrentState mag niet null zijn.");
            Console.WriteLine($"Current state: {cur.Id} ({cur.Name})");
        }

        private void SuggestAvailableTriggers()
        {
            var cur = _model.CurrentState
                      ?? throw new InvalidOperationException("CurrentState mag niet null zijn.");

            var list = _model.Transitions.Values
                .Where(t => IsAncestorOrSelf(cur, t.Source))
                .Select(t => $"{t.Trigger.Id} ({t.Trigger.Name})")
                .Distinct()
                .ToList();

            if (list.Any())
            {
                Console.WriteLine("Available triggers (incl. inherited):");
                list.ForEach(t => Console.WriteLine($"  - {t}"));
            }
            else
            {
                Console.WriteLine("There are no outgoing transitions from this state or its parents.");
            }
        }

        private bool IsAncestorOrSelf(State state, State ancestor)
        {
            var c = state;
            while (c != null)
            {
                if (c.Id == ancestor.Id) return true;
                c = c.Parent;
            }
            return false;
        }

        private void ExecuteTransition(Transition t)
        {
            Console.WriteLine($"Executing transition: {t.Source.Id} -> {t.Target.Id}");
            foreach (var a in t.Source.ExitActions)
                Console.WriteLine($"  Exit action: {a.Name}");
            foreach (var a in t.Actions)
                Console.WriteLine($"  Transition action: {a.Name}");
            _model.CurrentState = t.Target;
            foreach (var a in t.Target.EntryActions)
                Console.WriteLine($"  Entry action: {a.Name}");
            if (_model.FinalStates.Contains(t.Target))
                Console.WriteLine("*** FINAL STATE REACHED ***");
        }
    }
}
