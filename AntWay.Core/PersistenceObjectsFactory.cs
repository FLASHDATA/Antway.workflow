using AntWay.Persistence.Provider;
//Reference Data Provider
using AntWay.Oracle.Provider;

namespace Antway.Core
{
    public static class PersistenceObjectsFactory
    {
        public static IDALProcessPersistence GetIDALWFLocatorObject()
        {
            return new ProcessEFDAL();
        }

        public static IDALWFSchema GetIDALWFSchemaObject()
        {
            return new WFSchemaEFDAL();
        }
    }
}
