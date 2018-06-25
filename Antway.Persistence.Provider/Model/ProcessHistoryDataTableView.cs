using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AntWay.Persistence.Model
{
    public class ProcessHistoryDataTableView
    {
        public string Aplicacion { get; set; }
        public string Localizador { get; set; }
        public string EstadoActual { get; set; }
        public string Tags { get; set; }
        public string UltimaActualizacion { get; set; }
    }

    public class ProcessHistoryFilter
    {
        public int PaginatioFromRecord { get; set; }
        public int PaginatioToRecord { get; set; }

        public List<DataTableFilterFields> FilterFields { get; set; }

        public DataTableFilterFields FilterFrom
        {
            get
            {
                DataTableFilterFields dateFrom = FilteredFields
                                                .FirstOrDefault(f => f.Field == "From");

                DateTime dateTimeFrom = dateFrom?.Value != null
                                            ? Convert.ToDateTime(dateFrom.Value)
                                                .AddHours(TimeZoneInfo.Local.GetUtcOffset(DateTime.UtcNow).Hours)
                                            : DateTime.Now;
                
                var result = new DataTableFilterFields
                               {
                                   Field = "From",
                                   Value = dateTimeFrom.Date
                               };

                return result;
            }
        }

        public DataTableFilterFields FilterTo
        {
            get
            {
                DataTableFilterFields dateTo = FilteredFields
                                                .FirstOrDefault(f => f.Field == "To");

                DateTime dateTime = dateTo != null
                                        ? Convert.ToDateTime(dateTo.Value)
                                            .AddHours(TimeZoneInfo.Local.GetUtcOffset(DateTime.UtcNow).Hours)
                                        : DateTime.Now;

                var result = new DataTableFilterFields
                {
                    Field = "To",
                    Value = dateTime.Date.AddHours(23).AddMinutes(59)
                };

                return result;
            }
        }

        public List<DataTableFilterFields> FilteredStringFields
        {
            get
            {
                var result = FilteredFields
                                .Where(f => (f.Type ==null || f.Type?.ToLower() == "string"))
                                .ToList();
                return result;
            }
        }

        protected List<DataTableFilterFields> FilteredFields
        {
            get
            {
                var result = FilterFields
                                .Where(f => f.Value != null
                                            && !String.IsNullOrEmpty(f.Value.ToString()))
                                .ToList();
                return result;
            }
        }
    }

    public class DataTableFilterFields
    {
        public string Field { get; set; }
        public object Value { get; set; }
        public string Type { get; set; }
    }
}
