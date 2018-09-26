using System.Collections.Generic;

namespace Pdf.Storage.Hangfire
{
    public static class HangfireConstants
    {
        public const string HighPriorityQueue = "critical";
        public const string DefaultPriorityQueue = "default";
        public static IEnumerable<string> GetQueues() => new [] { HighPriorityQueue, DefaultPriorityQueue };
    }
}