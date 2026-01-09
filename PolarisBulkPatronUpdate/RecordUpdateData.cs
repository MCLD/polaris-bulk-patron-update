using Clc.Polaris.Api.Models;

namespace PolarisBulkPatronUpdate
{
    internal class RecordUpdateData
    {
        public PatronUpdateParams PatronUpdateParams { get; set; } = new PatronUpdateParams();
        public bool PerformUpdate { get; set; }
    }
}