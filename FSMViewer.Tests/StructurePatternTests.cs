using System;
using System.Collections.Generic;
using Xunit;
using FSMViewer.Factory;
using FSMViewer.Model;
using FSMViewer.Renderer;

namespace FSMViewer.Tests
{
    public class StructurePatternTests
    {
        [Fact]
        public void CompoundState_Clone_DeepCopiesChildren()
        {
            // Arrange
            var factory = new StateFactory();
            var compound = factory.CreateState("c", "Compound", StateType.COMPOUND) as CompoundState;
            Assert.NotNull(compound);

            var child1 = factory.CreateState("s1", "State1", StateType.SIMPLE);
            var child2 = factory.CreateState("s2", "State2", StateType.SIMPLE);
            // set relationships
            child1.Parent = compound;
            child2.Parent = compound;
            compound.Children.Add(child1);
            compound.Children.Add(child2);

            // Act
            var clone = compound.Clone();
            var cloneComp = Assert.IsType<CompoundState>(clone);

            // Assert
            // IDs preserved but different instances
            Assert.Equal(compound.Id, cloneComp.Id);
            Assert.NotSame(compound, cloneComp);

            // Children deep-copied
            Assert.Equal(2, cloneComp.Children.Count);
            for (int i = 0; i < compound.Children.Count; i++)
            {
                var originalChild = compound.Children[i];
                var clonedChild = cloneComp.Children[i];
                Assert.Equal(originalChild.Id, clonedChild.Id);
                Assert.NotSame(originalChild, clonedChild);
                // Parent of clone-child should be clone compound
                Assert.Equal(cloneComp, clonedChild.Parent);
            }
        }

        [Fact]
        public void CompoundState_Accept_VisitorTraversesAllNodes()
        {
            // Arrange
            var factory = new StateFactory();
            var compound = factory.CreateState("c", "Compound", StateType.COMPOUND) as CompoundState;
            Assert.NotNull(compound);

            var child1 = factory.CreateState("s1", "State1", StateType.SIMPLE);
            var child2 = factory.CreateState("s2", "State2", StateType.SIMPLE);
            child1.Parent = compound;
            child2.Parent = compound;
            compound.Children.Add(child1);
            compound.Children.Add(child2);

            var visited = new List<State>();
            var visitor = new TestVisitor(visited);

            // Act
            compound.Accept(visitor);

            // Assert: compound then each child in order
            Assert.Equal(3, visited.Count);
            Assert.Equal(compound, visited[0]);
            Assert.Equal(child1, visited[1]);
            Assert.Equal(child2, visited[2]);
        }

        // Simple visitor collecting visits
        private class TestVisitor : IStateVisitor
        {
            private readonly List<State> _visited;
            public TestVisitor(List<State> visited) => _visited = visited;

            public void Visit(SimpleState state)
            {
                _visited.Add(state);
            }

            public void Visit(CompoundState state)
            {
                _visited.Add(state);
                foreach (var child in state.Children)
                {
                    child.Accept(this);
                }
            }

            public void Visit(InitialState state)
            {
                _visited.Add(state);
            }

            public void Visit(FinalState state)
            {
                _visited.Add(state);
            }
        }
    }
}