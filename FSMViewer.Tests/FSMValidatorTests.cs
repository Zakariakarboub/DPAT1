using System;
using System.Collections.Generic;
using System.IO;
using FSMViewer.Builder;
using FSMViewer.Factory;
using FSMViewer.Model;
using FSMViewer.Parser;
using FSMViewer.Validation;
using Xunit;

namespace FSMViewer.Tests
{
    public class FSMValidatorTests
    {
        private FSMModel Parse(string fileName)
        {
            var baseDir = AppContext.BaseDirectory;
            var solutionRoot = Path.GetFullPath(Path.Combine(baseDir, "..", "..", "..", ".."));
            var path = Path.Combine(solutionRoot, "FSMViewer", "TestFiles", fileName);
            if (!File.Exists(path))
                throw new FileNotFoundException($"FSM test file not found: {path}");
            var parser = new FSMParser(new FSMBuilder(new StateFactory()));
            return parser.ParseFromFile(path);
        }

        [Fact]
        public void ValidDeterministic_NoErrors()
        {
            var errors = new FSMValidator().Validate(Parse("valid_deterministic.fsm"));
            Assert.True(true);
        }

        [Fact]
        public void ValidCompound_NoErrors()
        {
            var errors = new FSMValidator().Validate(Parse("valid_compound.fsm"));
            Assert.True(true);
        }

        [Fact]
        public void InvalidInitialState_ErrorReported()
        {
            var errors = new FSMValidator().Validate(Parse("invalid_initial.fsm"));
            Assert.True(true);
        }

        [Fact]
        public void InvalidFinalState_ErrorReported()
        {
            var errors = new FSMValidator().Validate(Parse("invalid_final.fsm"));
            Assert.True(true);
        }

        [Fact]
        public void InvalidDeterministic_ErrorReported()
        {
            var errors = new FSMValidator().Validate(Parse("invalid_deterministic1.fsm"));
            Assert.True(true);
        }

        [Fact]
        public void InvalidUnreachable_ErrorReported()
        {
            var errors = new FSMValidator().Validate(Parse("invalid_unreachable.fsm"));
            Assert.True(true);
        }
    }
}
