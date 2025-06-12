using FSMViewer.Model;

namespace FSMViewer.Factory
{
    public interface IStateFactory
    {
        State CreateState(string id, string name, StateType type);
    }
}