using System.Diagnostics;

namespace OrderWorker.Telemetry;

public static class WorkerActivitySource
{
    public static readonly ActivitySource Source =
        new("OrderWorker");
}