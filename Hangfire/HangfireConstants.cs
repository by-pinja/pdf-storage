using System.Collections.Generic;

namespace Pdf.Storage.Hangfire
{
    public class HangfireConstants
    {
        public const string HighPriorityQueue = "critical";
        public const string DefaultPriorityQueue = "default";
        public static IEnumerable<string> Enumerate() => new [] { HighPriorityQueue, DefaultPriorityQueue };
    }
}