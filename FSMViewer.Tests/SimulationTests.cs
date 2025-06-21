using System;
using System.IO;
using Xunit;
using FSMViewer.Builder;
using FSMViewer.Factory;
using FSMViewer.Model;
using FSMViewer.Simulation;

namespace FSMViewer.Tests
{
    public class SimulationTests
    {
        [Fact]
        public void FSMSimulator_ResetCommand_ResetsToInitialState()
        {
            // Arrange: build FSM with a transition
            var builder = new FSMBuilder(new StateFactory())
                .AddState("init", "Initial", StateType.INITIAL)
                .AddState("s1", "State1", StateType.SIMPLE)
                .AddState("final", "Final", StateType.FINAL)
                .AddTrigger("go", "go")
                .AddTransition("t1", "init", "s1", "go", "");
            var model = builder.Build();

            var dummy = new DummyRenderer();
            var sim = new FSMSimulator(model, dummy);

            // Feed commands: show, go, reset, show, quit
            var commands = "show\ngo\nreset\nshow\nquit\n";
            Console.SetIn(new StringReader(commands));
            var outWriter = new StringWriter();
            Console.SetOut(outWriter);

            // Act
            sim.Run();
            var log = outWriter.ToString();

            // Assert: after 'go' the state changed, after 'reset' back to initial
            Assert.Contains("Executing transition: init -> s1", log);
            Assert.Contains("FSM reset to initial state", log);
            // Verify model.CurrentState is initial
            Assert.NotNull(model.CurrentState);
            Assert.Equal("init", model.CurrentState.Id);
        }

        [Fact]
        public void FSMSimulator_InvalidTrigger_ShowsErrorAndSuggestions()
        {
            // Arrange: FSM with one transition
            var builder = new FSMBuilder(new StateFactory())
                .AddState("start", "Start", StateType.INITIAL)
                .AddState("end", "End", StateType.FINAL)
                .AddTrigger("go", "go")
                .AddTransition("t1", "start", "end", "go", "");
            var model = builder.Build();

            var dummy = new DummyRenderer();
            var sim = new FSMSimulator(model, dummy);

            // Feed commands: unknown, quit
            var input = "unknown\nquit\n";
            Console.SetIn(new StringReader(input));
            var outWriter = new StringWriter();
            Console.SetOut(outWriter);

            // Act
            sim.Run();
            var log = outWriter.ToString();

            // Assert: error message about no transition found
            Assert.Contains("No transition found for trigger 'unknown' from state 'start'", log);
            // Suggest available triggers
            Assert.Contains("Available triggers", log);
            Assert.Contains("go (go)", log);
        }

        // Dummy renderer stub
        private class DummyRenderer : FSMViewer.Renderer.IRenderer
        {
            public string Render(FSMModel model) => string.Empty;
        }
    }
}
