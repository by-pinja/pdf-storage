using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Hangfire;
using Hangfire.Common;
using Hangfire.States;

namespace Pdf.Storage.Hangfire
{
    public class HangfireQueue : IHangfireQueue
    {
        private readonly IBackgroundJobClient _backgroundJobClient;
        private readonly IRecurringJobManager _recurringJobManager;

        public HangfireQueue(IBackgroundJobClient backgroundJobClient, IRecurringJobManager recurringJobManager)
        {
            _backgroundJobClient = backgroundJobClient;
            _recurringJobManager = recurringJobManager;
        }

        public string Enqueue<T>(Expression<Action<T>> methodCall)
        {
            return _backgroundJobClient.Enqueue<T>(methodCall);
        }

        public string EnqueueWithHighPriority<T>(Expression<Action<T>> methodCall, string originalJobId = null)
        {
            var state = new EnqueuedState(HangfireConstants.HighPriorityQueue);
            var newJobId = _backgroundJobClient.Create(methodCall, state);

            if(originalJobId != null)
                _backgroundJobClient.Delete(originalJobId);

            return newJobId;
        }
        public string Schedule<T>(Expression<Action<T>> methodCall, TimeSpan delay)
        {
            return _backgroundJobClient.Schedule(methodCall, delay);
        }

        public void ScheduleRecurring<T>(string recurringJobId, Expression<Action<T>> methodCall, string cronExpression)
        {
            _recurringJobManager.AddOrUpdate(recurringJobId, Job.FromExpression<T>(methodCall), cronExpression);
        }
    }
}