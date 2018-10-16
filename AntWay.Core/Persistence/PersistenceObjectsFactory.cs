using AntWay.Persistence.Provider;
using AntWay.Oracle.Provider;
using AntWay.Persistence.Provider.Model;

namespace Antway.Core.Persistence
{
    public static class PersistenceObjectsFactory
    {
        public static IDALProcessPersistence GetIDALProcessObject()
        {
            return new ProcessEFDAL();
        }

        public static IDALWFSchemes GetIDALWFSchemaObject()
        {
            return new WFSchemesEFDAL();
        }

        public static IDALWFSchemeParameters GetIDALWFSchemeParameters()
        {
            return new WFSchemesParametersEFDAL();
        }

        public static IDALLocators GetIDALLocatorsObject()
        {
            return new LocatorsEFDAL();
        }
    }
}
