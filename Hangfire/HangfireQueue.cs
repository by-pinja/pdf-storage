using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Hangfire;
using Hangfire.States;

namespace Pdf.Storage.Hangfire
{
    public class HangfireQueue : IHangfireQueue
    {
        private readonly IBackgroundJobClient _backgroundJobClient;

        public HangfireQueue(IBackgroundJobClient backgroundJobClient)
        {
            _backgroundJobClient = backgroundJobClient;
        }

        public void Enqueue<T>(Expression<Action<T>> methodCall)
        {
            _backgroundJobClient.Enqueue<T>(methodCall);
        }

        public void Schedule<T>(Expression<Action<T>> methodCall, TimeSpan delay)
        {
            _backgroundJobClient.Schedule(methodCall, delay);
        }

        public string PrioritizeJob(string id)
        {
            _backgroundJobClient.Delete(id);
            return _backgroundJobClient.Enqueue(methodCall);
        }

        public static IEnumerable<string> GetValidQueues => new string[] { "critical", "default"};
    }
}