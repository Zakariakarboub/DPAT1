using System.Collections.Generic;
using FSMViewer.Model;

namespace FSMViewer.Validation
{
    public interface IValidator
    {
        List<string> Validate(FSMModel model);
    }
}