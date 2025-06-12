using FSMViewer.Renderer;

namespace FSMViewer.Model
{
    public class InitialState : State
    {
        public InitialState(string id, string name) : base(id, name, StateType.INITIAL) { }

        public override void Accept(IStateVisitor visitor)
        {
            visitor.Visit(this);
        }

        public override State Clone()
        {
            return new InitialState(Id, Name);
        }
    }
}