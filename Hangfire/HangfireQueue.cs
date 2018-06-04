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

        public string Enqueue<T>(Expression<Action<T>> methodCall)
        {
            return _backgroundJobClient.Enqueue<T>(methodCall);
        }

        public string EnqueueWithHighPriority<T>(Expression<Action<T>> methodCall)
        {
            var state = new EnqueuedState(HangfireConstants.HighPriorityQueue);
            return _backgroundJobClient.Create(methodCall, state);
        }
        public string Schedule<T>(Expression<Action<T>> methodCall, TimeSpan delay)
        {
            return _backgroundJobClient.Schedule(methodCall, delay);
        }

        public bool RemoveJob(string id)
        {
            return _backgroundJobClient.Delete(id);
        }
    }
}