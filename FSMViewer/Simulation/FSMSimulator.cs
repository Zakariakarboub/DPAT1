using System;
using System.Linq;
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
            _model = model;
            _renderer = renderer;
        }

        public void Run()
        {
            _model.Reset();
            Console.WriteLine("=== FSM SIMULATOR ===");
            Console.WriteLine("Available triggers:");

            foreach (var trigger in _model.Triggers.Values)
            {
                Console.WriteLine($"  {trigger.Id}: {trigger.Name}");
            }
            Console.WriteLine("Type 'quit' to exit, 'show' to display current state, 'reset' to restart");
            Console.WriteLine();

            while (true)
            {
                if (_model.CurrentState != null)
                {
                    Console.WriteLine($"Current state: {_model.CurrentState.Id} ({_model.CurrentState.Name})");
                }
                Console.Write("Enter trigger: ");

                var input = Console.ReadLine()?.Trim().ToLower();

                if (input == "quit")
                    break;

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

                if (string.IsNullOrEmpty(input))
                    continue;

                if (_model.CurrentState != null)
                {
                    var transition = _model.Transitions.Values
                        .FirstOrDefault(t => t.Source.Id == _model.CurrentState.Id &&
                                           t.Trigger.Id.ToLower() == input);

                    if (transition != null)
                    {
                        ExecuteTransition(transition);
                    }
                    else
                    {
                        Console.WriteLine($"No transition found for trigger '{input}' from state '{_model.CurrentState.Id}'");
                    }
                }

                Console.WriteLine();
            }
        }

        private void ExecuteTransition(Transition transition)
        {
            Console.WriteLine($"Executing transition: {transition.Source.Id} -> {transition.Target.Id}");

            foreach (var action in transition.Source.ExitActions)
            {
                Console.WriteLine($"  Exit action: {action.Name}");
            }

            foreach (var action in transition.Actions)
            {
                Console.WriteLine($"  Transition action: {action.Name}");
            }

            _model.CurrentState = transition.Target;

            foreach (var action in transition.Target.EntryActions)
            {
                Console.WriteLine($"  Entry action: {action.Name}");
            }

            if (_model.FinalStates.Contains(transition.Target))
            {
                Console.WriteLine("*** FINAL STATE REACHED ***");
            }
        }
    }
}