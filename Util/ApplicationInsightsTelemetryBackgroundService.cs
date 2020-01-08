using System;
using System.Threading;
using System.Threading.Tasks;
using Hangfire;
using Hangfire.Storage;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Pdf.Storage.Util
{
    public sealed class ApplicationInsightsTelemetryBackgroundService : BackgroundService
    {
        private const string MetricPrefix = "pdf-storage-hangfire";
        private readonly TimeSpan _samplingInterval = TimeSpan.FromSeconds(60);
        private readonly TelemetryClient _telemetryClient;
        private readonly IMonitoringApi _hangfireApi;
        private readonly ILogger<ApplicationInsightsTelemetryBackgroundService> _logger;

        public ApplicationInsightsTelemetryBackgroundService(TelemetryClient telemetryClient, JobStorage hangfireJobStorage, ILogger<ApplicationInsightsTelemetryBackgroundService> logger)
        {
            _telemetryClient = telemetryClient;
            _hangfireApi = hangfireJobStorage.GetMonitoringApi();
            _logger = logger;
        }

        private async Task Worker(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                try
                {
                    await Task.Delay(_samplingInterval);

                    var hangfireStats = _hangfireApi.GetStatistics();

                    _telemetryClient.TrackMetric(new MetricTelemetry(MetricPrefix + "-enqueued", hangfireStats.Enqueued));
                    _telemetryClient.TrackMetric(new MetricTelemetry(MetricPrefix + "-scheduled", hangfireStats.Scheduled));
                    _telemetryClient.TrackMetric(new MetricTelemetry(MetricPrefix + "-failed", hangfireStats.Failed));
                    _telemetryClient.TrackMetric(new MetricTelemetry(MetricPrefix + "-processing", hangfireStats.Processing));
                    _telemetryClient.TrackMetric(new MetricTelemetry(MetricPrefix + "-servers", hangfireStats.Servers));
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Writing additional telemetry failed");
                }
            }
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await Worker(stoppingToken);
        }
    }
}