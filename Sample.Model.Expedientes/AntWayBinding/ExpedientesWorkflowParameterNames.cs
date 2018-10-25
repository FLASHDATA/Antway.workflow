using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AntWay.Core.Mapping;


namespace Sample.Model.Expedientes.AntWayBinding
{

    public static class ExpedientesWorkflowParameterNames
    {
        public const string ESTAT_SIGNATURA = "ESTAT_SIGNATURA";
        public const string ESTAT_SIGNATURA_ENVIAT_A_SIGNAR = "ENVIAT_A_SIGNAR";
        public const string ESTAT_SIGNATURA_SIGNAT = "SIGNAT";
        public const string ESTAT_SIGNATURA_CADUCAT = "CADUCAT";
    }


    /// <summary>
    /// MAPEADO ENTERO DE PARÁMETROS PARA BD
    /// SEGURAMENTE SE QUITARÁ (NO SE ESTÁ USANDO EL MAPEO A BD IMPLEMENTADO)
    /// </summary>
    //public class ExpedientesParametersMappingNOTUSED
    //{
    //    public static ExpedientesParametersMappingNOTUSED Single
    //                                    => new ExpedientesParametersMappingNOTUSED();

    //    public const string ESTAT_SIGNATURA_ENVIAT_A_SIGNAR = "ENVIAT_A_SIGNAR";
    //    public const string ESTAT_SIGNATURA_SIGNAT = "SIGNAT";
    //    public const string ESTAT_SIGNATURA_CADUCAT = "CADUCAT";

    //    [ParameterValues(Values = new string[]
    //            {
    //                ESTAT_SIGNATURA_ENVIAT_A_SIGNAR,
    //                ESTAT_SIGNATURA_SIGNAT,
    //                ESTAT_SIGNATURA_CADUCAT
    //            })]
    //    public string ESTAT_SIGNATURA => "ESTAT_SIGNATURA";
    //}
}
