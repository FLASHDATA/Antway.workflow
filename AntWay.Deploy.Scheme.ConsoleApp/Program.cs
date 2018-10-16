using System;
using System.Collections.Generic;
using Antway.Core.Persistence;
using AntWay.Core.Scheme;
using AntWay.Persistence.Provider.Model;
using Sample.Model.Expedientes;

namespace AntWay.Deploy.Scheme.ConsoleApp
{
    class Program
    {
        static void Main(string[] args)
        {
            List<KeyValuePair<string,List<string>>>
              schemeParametersKVP = MappingReflection
                                   .GetSchemeValuesParameters(ExpedientesParametersMappingNOTUSED.Single);

            var schemesPersistence = new SchemesPersistence
            {
                IDALSchemes = PersistenceObjectsFactory.GetIDALWFSchemaObject(),
                IDALSchemeParameters = PersistenceObjectsFactory.GetIDALWFSchemeParameters(),
            };

            schemesPersistence.UpdateSchemes(SchemeLocator.SchemeCode,
                                             schemeParametersKVP);
        }
    }
}
