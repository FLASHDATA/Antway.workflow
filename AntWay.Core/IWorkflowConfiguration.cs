using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AntWay.Interfaces
{
    //TODO: Por definir interfaces que deben implementar todos las aplicaciones
    //que utilizen AntWay
    public interface IWorkflowConfiguration
    {
        string SchemeCode { get; }
    }
}
