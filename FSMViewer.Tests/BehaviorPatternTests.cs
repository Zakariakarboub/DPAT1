using System;
using System.IO;
using Xunit;
using FSMViewer.Builder;
using FSMViewer.Factory;
using FSMViewer.Model;
using FSMViewer.Renderer;
using FSMViewer.Simulation;

namespace FSMViewer.Tests
{
    public class BehaviorPatternTests
    {
        [Fact]
        public void TextRenderer_Render_ProducesCorrectLines()
        {
            // Arrange: simple FSM Initial->Simple->Final
            var builder = new FSMBuilder(new StateFactory())
                .AddState("s1", "State1", StateType.INITIAL)
                .AddState("s2", "State2", StateType.SIMPLE)
                .AddState("s3", "State3", StateType.FINAL)
                .AddTrigger("t1", "trigger1")
                .AddTransition("t1", "s1", "s2", "t1", "");
            var model = builder.Build();

            var renderer = new TextRenderer();

            // Act
            var output = renderer.Render(model);
            var lines = output.Split(new[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries);

            // Assert: should contain type tags
            Assert.Contains(lines, l => l.StartsWith("[INITIAL] s1: State1"));
            Assert.Contains(lines, l => l.StartsWith("[SIMPLE] s2: State2"));
            Assert.Contains(lines, l => l.StartsWith("[FINAL] s3: State3"));
        }

        [Fact]
        public void FSMSimulator_WithStrategy_CallsRendererAndUpdatesState()
        {
            // Arrange: build FSM and dummy renderer
            var builder = new FSMBuilder(new StateFactory())
                .AddState("s1", "State1", StateType.INITIAL)
                .AddState("s2", "State2", StateType.SIMPLE)
                .AddState("s3", "State3", StateType.FINAL)
                .AddTrigger("t1", "trigger1")
                .AddTransition("t1", "s1", "s2", "t1", "");
            var model = builder.Build();

            var dummy = new DummyRenderer();
            var sim = new FSMSimulator(model, dummy);

            // Feed commands: show then t1 then quit
            var input = new StringReader("show\nt1\nquit\n");
            Console.SetIn(input);
            var outputWriter = new StringWriter();
            Console.SetOut(outputWriter);

            // Act
            sim.Run();

            // Assert: DummyRenderer.Render called on 'show'
            Assert.True(dummy.RenderCalled, "Render should be called when 'show' input is given");
            // Simulator logs transition execution
            var log = outputWriter.ToString();
            Assert.Contains("Executing transition: s1 -> s2", log);
            // Model's current state updated
            Assert.Equal("s2", model.CurrentState.Id);
        }

        // Dummy renderer for strategy pattern
        private class DummyRenderer : IRenderer
        {
            public bool RenderCalled { get; private set; }

            public string Render(FSMModel model)
            {
                RenderCalled = true;
                return string.Empty;
            }
        }
    }
}
