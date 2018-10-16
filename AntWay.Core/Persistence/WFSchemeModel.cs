using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Antway.Core;
using AntWay.Persistence.Provider.Model;

namespace Antway.Core.Persistence
{
    public class WFSchemeModel : IWFSchemeModel
    {
        public List<WorkflowSchemeParameterValuesView> GetParametersList(string schemeCode)
        {
            var schemesPersistence = new SchemesPersistence
            {
                IDALSchemeParameters = PersistenceObjectsFactory.GetIDALWFSchemeParameters()
            };

            var result = schemesPersistence.GetParametersList(schemeCode);


            return result;
        }
    }
}
