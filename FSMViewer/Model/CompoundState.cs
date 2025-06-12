using FSMViewer.Renderer;

namespace FSMViewer.Model
{
    public class CompoundState : State
    {
        public CompoundState(string id, string name) : base(id, name, StateType.COMPOUND) { }

        public override void Accept(IStateVisitor visitor)
        {
            visitor.Visit(this);
        }

        public override State Clone()
        {
            var clone = new CompoundState(Id, Name);
            foreach (var child in Children)
            {
                var childClone = child.Clone();
                childClone.Parent = clone;
                clone.Children.Add(childClone);
            }
            return clone;
        }
    }
}