using FSMViewer.Renderer;

namespace FSMViewer.Model
{
    public class SimpleState : State
    {
        public SimpleState(string id, string name) : base(id, name, StateType.SIMPLE) { }

        public override void Accept(IStateVisitor visitor)
        {
            visitor.Visit(this);
        }

        public override State Clone()
        {
            return new SimpleState(Id, Name);
        }
    }
}