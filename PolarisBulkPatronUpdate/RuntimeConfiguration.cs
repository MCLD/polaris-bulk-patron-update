namespace PolarisBulkPatronUpdate
{
    internal class RuntimeConfiguration
    {
        public string CsvPath { get; set; } = string.Empty;
        public int DelayBetweenWrites { get; set; } = 0;
        public bool Go { get; set; }
    }
}