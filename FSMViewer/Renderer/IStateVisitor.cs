using FSMViewer.Model;

namespace FSMViewer.Renderer
{
    public interface IStateVisitor
    {
        void Visit(SimpleState state);
        void Visit(CompoundState state);
        void Visit(InitialState state);
        void Visit(FinalState state);
    }
}