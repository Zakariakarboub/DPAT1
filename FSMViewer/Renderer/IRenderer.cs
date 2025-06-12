using FSMViewer.Model;

namespace FSMViewer.Renderer
{
    public interface IRenderer
    {
        string Render(FSMModel model);
    }
}