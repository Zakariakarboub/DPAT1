using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using FSMViewer.Model;
using FSMViewer.Builder;

namespace FSMViewer.Parser
{
    public class FSMParser
    {
        private readonly IFSMBuilder _builder;
        private string? _currentTransitionId;

        public FSMParser(IFSMBuilder builder)
        {
            _builder = builder ?? throw new ArgumentNullException(nameof(builder));
        }

        public FSMModel ParseFromFile(string filePath)
        {
            if (!File.Exists(filePath))
                throw new FileNotFoundException($"File not found: {filePath}");
            var content = File.ReadAllText(filePath);
            return ParseFromString(content);
        }

        public FSMModel ParseFromString(string content)
        {
            // 1) verwijder comments
            var uncommented = Regex.Replace(content, @"#.*", string.Empty);
            // 2) split op ';'
            var segments = uncommented
                .Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(s => s.Trim())
                .Where(s => s.Length > 0)
                .ToList();

            foreach (var line in segments)
            {
                if (line.StartsWith("STATE", StringComparison.OrdinalIgnoreCase)) ParseState(line);
                else if (line.StartsWith("TRIGGER", StringComparison.OrdinalIgnoreCase)) ParseTrigger(line);
                else if (line.StartsWith("ACTION", StringComparison.OrdinalIgnoreCase)) ParseAction(line);
                else if (line.StartsWith("TRANSITION", StringComparison.OrdinalIgnoreCase)) ParseTransition(line);
                else Console.WriteLine($"[Parser warning] onbekende regel genegeerd: '{line}'");
            }

            return _builder.Build();
        }

        // STATE <id> <parent|_> "<name>" : <type>
        static readonly Regex StateRegex = new Regex(
            @"^\s*STATE\s+(\w+)\s+(\w+|_)\s+""([^""]*)""\s*:\s*(\w+)\s*$",
            RegexOptions.IgnoreCase | RegexOptions.Compiled
        );

        void ParseState(string line)
        {
            var m = StateRegex.Match(line);
            if (!m.Success)
            {
                Console.WriteLine($"[Parser warning] invalid STATE: {line}");
                return;
            }

            var id = m.Groups[1].Value;
            var parent = m.Groups[2].Value == "_" ? null : m.Groups[2].Value;
            var name = m.Groups[3].Value;
            var typeStr = m.Groups[4].Value;

            if (!Enum.TryParse<StateType>(typeStr, ignoreCase: true, out var type))
            {
                Console.WriteLine($"[Parser warning] onbekend StateType '{typeStr}' in: {line}");
                return;
            }

            _builder.AddState(id, name, type, parent);
        }

        // TRIGGER <id> "<desc>"
        static readonly Regex TriggerRegex = new Regex(
            @"^\s*TRIGGER\s+(\w+)\s+""([^""]*)""\s*$",
            RegexOptions.IgnoreCase | RegexOptions.Compiled
        );

        void ParseTrigger(string line)
        {
            var m = TriggerRegex.Match(line);
            if (!m.Success)
            {
                Console.WriteLine($"[Parser warning] invalid TRIGGER: {line}");
                return;
            }

            var id = m.Groups[1].Value;
            var desc = m.Groups[2].Value;
            _builder.AddTrigger(id, desc);
        }

        // ACTION <id> "<desc>" : <type>
        static readonly Regex ActionRegex = new Regex(
            @"^\s*ACTION\s+(\w+)\s+""([^""]*)""\s*:\s*(\w+)\s*$",
            RegexOptions.IgnoreCase | RegexOptions.Compiled
        );

        void ParseAction(string line)
        {
            var m = ActionRegex.Match(line);
            if (!m.Success)
            {
                Console.WriteLine($"[Parser warning] invalid ACTION: {line}");
                return;
            }

            var actionId = m.Groups[1].Value;
            var actionName = m.Groups[2].Value;
            var typeStr = m.Groups[3].Value;

            if (!Enum.TryParse<ActionType>(typeStr, true, out var actionType))
            {
                Console.WriteLine($"[Parser warning] onbekend ActionType '{typeStr}' in: {line}");
                return;
            }

            // 1) registreer actie
            _builder.AddAction(actionId, actionName, actionType);

            // 2) koppel aan juiste target
            switch (actionType)
            {
                case ActionType.ENTRY_ACTION:
                    // moet meteen de stateId uit de regel parsen:
                    var entryStateId = line.Split()[1];
                    _builder.AddEntryAction(entryStateId, actionId);
                    break;

                case ActionType.EXIT_ACTION:
                    var exitStateId = line.Split()[1];
                    _builder.AddExitAction(exitStateId, actionId);
                    break;

                case ActionType.DO_ACTION:
                    var doStateId = line.Split()[1];
                    _builder.AddDoAction(doStateId, actionId);
                    break;

                case ActionType.TRANSITION_ACTION:
                    // Koppel op actionId == transitionId, ongeacht _currentTransitionId
                    _builder.AddTransitionAction(actionId, actionId);
                    break;
            }
        }

        // TRANSITION <id> <source> -> <target> [trigger] ["guard"]
        static readonly Regex TransitionRegex = new Regex(
            @"^\s*TRANSITION\s+(\w+)\s+(\w+)\s*->\s*(\w+)(?:\s+(\w+))?(?:\s+""([^""]*)"")?\s*$",
            RegexOptions.IgnoreCase | RegexOptions.Compiled
        );

        void ParseTransition(string line)
        {
            var m = TransitionRegex.Match(line);
            if (!m.Success)
            {
                Console.WriteLine($"[Parser warning] invalid TRANSITION: {line}");
                return;
            }

            _currentTransitionId = m.Groups[1].Value;

            var id = _currentTransitionId;
            var source = m.Groups[2].Value;
            var target = m.Groups[3].Value;

            // trigger
            string triggerId = m.Groups[4].Success && m.Groups[4].Value.Length > 0
                ? m.Groups[4].Value
                : $"__{id}__";

            if (!m.Groups[4].Success)
                _builder.AddTrigger(triggerId, string.Empty);

            var guard = m.Groups.Count >= 6 && m.Groups[5].Success
                ? m.Groups[5].Value
                : string.Empty;

            try
            {
                _builder.AddTransition(id, source, target, triggerId, guard);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Parser warning] kon transition '{id}' niet toevoegen: {ex.Message}");
            }
        }
    }
}
