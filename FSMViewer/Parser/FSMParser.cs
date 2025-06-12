using System;
using System.IO;
using System.Text.RegularExpressions;
using FSMViewer.Model;
using FSMViewer.Builder;

namespace FSMViewer.Parser
{
    public class FSMParser
    {
        private readonly IFSMBuilder _builder;

        public FSMParser(IFSMBuilder builder)
        {
            _builder = builder;
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
            var lines = content.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);

            foreach (var line in lines)
            {
                var trimmedLine = line.Trim();
                if (string.IsNullOrEmpty(trimmedLine)) continue;

                if (trimmedLine.StartsWith("STATE"))
                    ParseState(trimmedLine);
                else if (trimmedLine.StartsWith("TRIGGER"))
                    ParseTrigger(trimmedLine);
                else if (trimmedLine.StartsWith("ACTION"))
                    ParseAction(trimmedLine);
                else if (trimmedLine.StartsWith("TRANSITION"))
                    ParseTransition(trimmedLine);
            }

            return _builder.Build();
        }

        private void ParseState(string line)
        {
            var match = Regex.Match(line, @"STATE\s+(\w+)\s+(\w+|_)\s+""([^""]*)""\s*:\s*(\w+)");
            if (match.Success)
            {
                var id = match.Groups[1].Value;
                var parent = match.Groups[2].Value == "_" ? null : match.Groups[2].Value;
                var name = match.Groups[3].Value;
                var typeStr = match.Groups[4].Value;

                if (Enum.TryParse<StateType>(typeStr, out var type))
                {
                    _builder.AddState(id, name, type, parent);
                }
            }
        }

        private void ParseTrigger(string line)
        {
            var match = Regex.Match(line, @"TRIGGER\s+(\w+)\s+""([^""]*)""");
            if (match.Success)
            {
                var id = match.Groups[1].Value;
                var name = match.Groups[2].Value;
                _builder.AddTrigger(id, name);
            }
        }

        private void ParseAction(string line)
        {
            var match = Regex.Match(line, @"ACTION\s+(\w+)\s+""([^""]*)""\s*:\s*(\w+)");
            if (match.Success)
            {
                var id = match.Groups[1].Value;
                var name = match.Groups[2].Value;
                var typeStr = match.Groups[3].Value;

                if (Enum.TryParse<ActionType>(typeStr, out var type))
                {
                    _builder.AddAction(id, name, type);
                }
            }
        }

        private void ParseTransition(string line)
        {
            var match = Regex.Match(line, @"TRANSITION\s+(\w+)\s+(\w+)\s*->\s*(\w+)\s+(\w+)\s+""([^""]*)""");
            if (match.Success)
            {
                var id = match.Groups[1].Value;
                var source = match.Groups[2].Value;
                var target = match.Groups[3].Value;
                var trigger = match.Groups[4].Value;
                var guard = match.Groups[5].Value;

                _builder.AddTransition(id, source, target, trigger, guard);
            }
        }
    }
}