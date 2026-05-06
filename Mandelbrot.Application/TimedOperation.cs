using System.Diagnostics;
using Serilog;

namespace Mandelbrot.Application;

public sealed class TimedOperation : IDisposable
{
    private readonly ILogger _logger;
    private readonly string _operationName;
    private readonly Stopwatch _stopwatch;
    private bool _disposed;

    private TimedOperation(string operationName, ILogger logger)
    {
        _operationName = operationName;
        _logger = logger;
        _stopwatch = Stopwatch.StartNew();

        _logger.Information("Starting {OperationName}.", _operationName);
    }

    public TimeSpan Elapsed => _stopwatch.Elapsed;

    public static TimedOperation Start(string operationName, ILogger? logger = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(operationName);

        return new TimedOperation(operationName, logger ?? new LoggerConfiguration().CreateLogger());
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _stopwatch.Stop();
        _logger.Information(
            "Finished {OperationName} in {ElapsedMilliseconds:F2} ms.",
            _operationName,
            _stopwatch.Elapsed.TotalMilliseconds);

        _disposed = true;
    }
}
