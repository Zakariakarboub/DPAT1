using Xunit;
using FSMViewer.Builder;
using FSMViewer.Factory;
using FSMViewer.Model;

namespace FSMViewer.Tests
{
    public class CreationPatternTests
    {
        [Fact]
        public void Builder_AddsStatesTriggersTransitionsCorrectly()
        {
            // Arrange
            var builder = new FSMBuilder(new StateFactory());
            builder
                .AddState("s1", "State1", StateType.INITIAL)
                .AddState("s2", "State2", StateType.SIMPLE)
                .AddState("s3", "State3", StateType.FINAL)
                .AddTrigger("t1", "trigger1")
                .AddTransition("tr1", "s1", "s2", "t1");

            // Act
            var model = builder.Build();

            // Assert
            // 3 states in totaal
            Assert.Equal(3, model.States.Count);

            // InitialState bestaat en is het juiste subtype
            Assert.NotNull(model.InitialState);
            Assert.Equal("s1", model.InitialState.Id);
            Assert.IsType<InitialState>(model.InitialState);

            // Er is precies één final state, en dat is s3
            var finals = model.FinalStates;
            Assert.Single(finals);
            var finalState = finals[0];
            Assert.Equal("s3", finalState.Id);
            Assert.IsType<FinalState>(finalState);

            // Trigger t1 is geregistreerd
            Assert.Contains("t1", model.Triggers.Keys);

            // Transition tr1 wijst naar de juiste source/target en trigger
            Assert.Contains("tr1", model.Transitions.Keys);
            var tr = model.Transitions["tr1"];
            Assert.Equal("s1", tr.Source.Id);
            Assert.Equal("s2", tr.Target.Id);
            Assert.Equal("t1", tr.Trigger.Id);
        }

        [Fact]
        public void StateFactory_CreatesCorrectStateSubtypes()
        {
            // Arrange
            var factory = new StateFactory();

            // Act
            var initial = factory.CreateState("i1", "InitState", StateType.INITIAL);
            var simple = factory.CreateState("s1", "SimpleState", StateType.SIMPLE);
            var compound = factory.CreateState("c1", "CompoundState", StateType.COMPOUND);
            var final = factory.CreateState("f1", "FinalState", StateType.FINAL);

            // Assert: juiste concrete klasse per type
            Assert.IsType<InitialState>(initial);
            Assert.IsType<SimpleState>(simple);
            Assert.IsType<CompoundState>(compound);
            Assert.IsType<FinalState>(final);
        }
    }
}
