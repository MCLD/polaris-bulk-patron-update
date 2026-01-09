using System.Diagnostics;
using System.Globalization;
using Clc.Polaris.Api;
using Clc.Polaris.Api.Models;
using CsvHelper;
using CsvHelper.Configuration;
using Microsoft.Extensions.Logging;

namespace PolarisBulkPatronUpdate
{
    internal class UpdateFields
    {
        private readonly ILogger<UpdateFields> _logger;
        private readonly IPapiClient _papiClient;
        private readonly RuntimeConfiguration _runtimeConfiguration;

        public UpdateFields(ILogger<UpdateFields> logger,
            IPapiClient papiClient,
            RuntimeConfiguration runtimeConfiguration)
        {
            ArgumentNullException.ThrowIfNull(logger);
            ArgumentNullException.ThrowIfNull(papiClient);
            ArgumentNullException.ThrowIfNull(runtimeConfiguration);

            _logger = logger;
            _papiClient = papiClient;
            _runtimeConfiguration = runtimeConfiguration;
        }

        public async Task GoAsync(CancellationToken cancellationToken)
        {
            var sw = Stopwatch.StartNew();

            if (!_runtimeConfiguration.Go)
            {
                _logger.LogWarning("This is a test run, changes are not being written to Polaris.");
            }

            _logger.LogInformation("Opening CSV file: {CsvFile}", _runtimeConfiguration.CsvPath);

            using var reader = new StreamReader(_runtimeConfiguration.CsvPath);
            using var csvIn = new CsvReader(reader,
                new CsvConfiguration(CultureInfo.InvariantCulture)
                {
                    HeaderValidated = null,
                    MissingFieldFound = null
                });

            int recordCount = 0;
            int updateCount = 0;

            await foreach (var record in csvIn
                .GetRecordsAsync<PatronRegistrationParams>(cancellationToken))
            {
                recordCount++;
                if (cancellationToken.IsCancellationRequested)
                {
                    return;
                }

                if (!string.IsNullOrEmpty(record.Barcode))
                {
                    _logger.LogDebug("Found CSV record {RecordNumber} for barcode {Barcode}",
                        recordCount,
                        record.Barcode);
                    var recordUpdateData = GetChanges(record);

                    if (string.IsNullOrEmpty(record?.Barcode?.Trim()))
                    {
                        _logger.LogError("Unable to find barcode for record {RecordNumber}",
                            recordCount);
                        continue;
                    }

                    if (_runtimeConfiguration.DelayBetweenWrites > 0
                        && recordCount > 1)
                    {
                        Thread.Sleep(_runtimeConfiguration.DelayBetweenWrites);
                    }

                    using (_logger.BeginScope(new Dictionary<string, object>
                    {
                        ["CsvRecord"] = recordCount,
                        ["Barcode"] = record.Barcode
                    }))
                    {
                        if (_runtimeConfiguration.Go
                            && recordUpdateData.PerformUpdate)
                        {
                            var result = _papiClient.PatronUpdate(record.Barcode,
                                recordUpdateData.PatronUpdateParams);
                            if (result?.Response.IsSuccessStatusCode == true)
                            {
                                updateCount++;
                                _logger.LogDebug(
                                    "Updated record {RecordNumber} for barcode {Barcode}",
                                    recordCount,
                                    record.Barcode);
                            }
                            else
                            {
                                if (result == null)
                                {
                                    _logger.LogError(
                                        "Empty result during update of record {RecordNumber}",
                                        recordCount);
                                }
                                else
                                {
                                    _logger.LogError
                                        ("Error during update of record {RecordNumber}: {ErrorMessage}",
                                        recordCount,
                                        result.Data?.ErrorMessage);
                                }
                            }
                        }
                    }
                }
            }

            _logger.LogInformation(
                "Run complete, {RecordCount} records results in {UpdateCount} updates in {ElapsedMs} ms",
                recordCount,
                updateCount,
                sw.ElapsedMilliseconds);
        }

        private static RecordUpdateData GetChanges(PatronRegistrationParams registrationParams)
        {
            var result = new RecordUpdateData();
            if (registrationParams.AddrCheckDate.HasValue
                && registrationParams.AddrCheckDate > DateTime.MinValue)
            {
                result.PatronUpdateParams.AddrCheckDate = registrationParams.AddrCheckDate;
                result.PerformUpdate = true;
            }
            if (!string.IsNullOrEmpty(registrationParams.AltEmailAddress?.Trim()))
            {
                result.PatronUpdateParams.AltEmailAddress = registrationParams.AltEmailAddress.Trim();
                result.PerformUpdate = true;
            }
            if (!string.IsNullOrEmpty(registrationParams.EmailAddress?.Trim()))
            {
                result.PatronUpdateParams.EmailAddress = registrationParams.EmailAddress.Trim();
                result.PerformUpdate = true;
            }
            if (registrationParams.EnableSMS.HasValue)
            {
                result.PatronUpdateParams.EnableSMS = registrationParams.EnableSMS;
                result.PerformUpdate = true;
            }
            if (registrationParams.ExpirationDate.HasValue
                && registrationParams.ExpirationDate > DateTime.MinValue)
            {
                result.PatronUpdateParams.ExpirationDate = registrationParams.ExpirationDate;
                result.PerformUpdate = true;
            }
            if (!string.IsNullOrEmpty(registrationParams.User1?.Trim()))
            {
                result.PatronUpdateParams.User1 = registrationParams.User1.Trim();
                result.PerformUpdate = true;
            }
            if (!string.IsNullOrEmpty(registrationParams.User2?.Trim()))
            {
                result.PatronUpdateParams.User2 = registrationParams.User2.Trim();
                result.PerformUpdate = true;
            }
            if (!string.IsNullOrEmpty(registrationParams.User3?.Trim()))
            {
                result.PatronUpdateParams.User3 = registrationParams.User3.Trim();
                result.PerformUpdate = true;
            }
            if (!string.IsNullOrEmpty(registrationParams.User4?.Trim()))
            {
                result.PatronUpdateParams.User4 = registrationParams.User4.Trim();
                result.PerformUpdate = true;
            }
            if (!string.IsNullOrEmpty(registrationParams.User5?.Trim()))
            {
                result.PatronUpdateParams.User5 = registrationParams.User5.Trim();
                result.PerformUpdate = true;
            }

            return result;
        }
    }
}
