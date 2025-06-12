using System.Linq;
using System.Text;
using FSMViewer.Model;

namespace FSMViewer.Renderer
{
    public class TextRenderer : IRenderer, IStateVisitor
    {
        private StringBuilder _output = new();
        private int _indentLevel;
        private FSMModel? _model;

        public string Render(FSMModel model)
        {
            _model = model;
            _output = new StringBuilder();
            _indentLevel = 0;

            _output.AppendLine("=== FSM MODEL ===");
            _output.AppendLine();

            RenderStates(model);
            RenderTransitions(model);
            RenderCurrentState(model);

            return _output.ToString();
        }

        private void RenderStates(FSMModel model)
        {
            _output.AppendLine("STATES:");
            foreach (var state in model.States.Values.Where(s => s.Parent == null))
            {
                state.Accept(this);
            }
            _output.AppendLine();
        }

        private void RenderTransitions(FSMModel model)
        {
            _output.AppendLine("TRANSITIONS:");
            foreach (var transition in model.Transitions.Values)
            {
                var guard = string.IsNullOrEmpty(transition.Guard) ? "" : $" [{transition.Guard}]";
                _output.AppendLine($"  {transition.Source.Id} --{transition.Trigger.Id}{guard}--> {transition.Target.Id}");
            }
            _output.AppendLine();
        }

        private void RenderCurrentState(FSMModel model)
        {
            if (model.CurrentState != null)
            {
                _output.AppendLine($"CURRENT STATE: {model.CurrentState.Id} ({model.CurrentState.Name})");
            }
        }

        public void Visit(SimpleState state)
        {
            WriteIndented($"[SIMPLE] {state.Id}: {state.Name}");
            RenderActions(state);
        }

        public void Visit(CompoundState state)
        {
            WriteIndented($"[COMPOUND] {state.Id}: {state.Name}");
            RenderActions(state);

            _indentLevel++;
            foreach (var child in state.Children)
            {
                child.Accept(this);
            }
            _indentLevel--;
        }

        public void Visit(InitialState state)
        {
            WriteIndented($"[INITIAL] {state.Id}: {state.Name}");
            RenderActions(state);
        }

        public void Visit(FinalState state)
        {
            WriteIndented($"[FINAL] {state.Id}: {state.Name}");
            RenderActions(state);
        }

        private void RenderActions(State state)
        {
            if (state.EntryActions.Any())
            {
                _indentLevel++;
                WriteIndented($"Entry: {string.Join(", ", state.EntryActions.Select(a => a.Name))}");
                _indentLevel--;
            }
            if (state.ExitActions.Any())
            {
                _indentLevel++;
                WriteIndented($"Exit: {string.Join(", ", state.ExitActions.Select(a => a.Name))}");
                _indentLevel--;
            }
        }

        private void WriteIndented(string text)
        {
            var indent = new string(' ', _indentLevel * 2);
            _output.AppendLine($"{indent}{text}");
        }
    }
}