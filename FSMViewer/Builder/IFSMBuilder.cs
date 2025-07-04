using FSMViewer.Model;

namespace FSMViewer.Builder
{
    public interface IFSMBuilder
    {
        IFSMBuilder AddState(string id, string name, StateType type, string? parentId = null);
        IFSMBuilder AddTrigger(string id, string name);
        IFSMBuilder AddAction(string id, string name, ActionType type);
        IFSMBuilder AddEntryAction(string stateId, string actionId);
        IFSMBuilder AddExitAction(string stateId, string actionId);
        IFSMBuilder AddDoAction(string stateId, string actionId);                // Ondersteunt DO_ACTION
        IFSMBuilder AddTransition(string id, string sourceId, string targetId, string triggerId, string guard = "");
        IFSMBuilder AddTransitionAction(string transitionId, string actionId);    // Ondersteunt TRANSITION_ACTION
        FSMModel Build();
    }
}
