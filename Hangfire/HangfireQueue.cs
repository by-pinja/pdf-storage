using System;
using System.Linq.Expressions;
using Hangfire;

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
    }
}