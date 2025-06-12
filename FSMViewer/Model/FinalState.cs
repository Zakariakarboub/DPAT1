using FSMViewer.Renderer;

namespace FSMViewer.Model
{
    public class FinalState : State
    {
        public FinalState(string id, string name) : base(id, name, StateType.FINAL) { }

        public override void Accept(IStateVisitor visitor)
        {
            visitor.Visit(this);
        }

        public override State Clone()
        {
            return new FinalState(Id, Name);
        }
    }
}