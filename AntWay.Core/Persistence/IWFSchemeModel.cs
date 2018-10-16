using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AntWay.Persistence.Provider.Model;

namespace Antway.Core.Persistence
{
    public interface IWFSchemeModel
    {
        List<WorkflowSchemeParameterValuesView> GetParametersList(string schemCode);
    }
}
