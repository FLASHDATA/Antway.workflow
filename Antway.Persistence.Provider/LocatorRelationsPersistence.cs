using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace AntWay.Persistence.Provider.Model
{
    public class LocatorRelationsPersistence
    {
        public IDAL IDAL { get; set; }

        public LocatorRelationsView AddRelation(LocatorRelationsView locatorRelation)
        {
            var result = IDAL.Insert<LocatorRelationsView>(locatorRelation);
            return result;
        }
    }
}
