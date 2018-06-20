using System;
using System.Linq.Expressions;
using Hangfire.Annotations;
using Hangfire.Common;
using Hangfire.States;
using Pdf.Storage.Hangfire;

namespace Pdf.Storage.Hangfire
{
    public class HangfireMock : IHangfireQueue
    {
        private readonly IServiceProvider _serviceProvider;
        public bool ExecuteActions { get; set;} = true;

        public HangfireMock(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public virtual string Enqueue<T>(Expression<Action<T>> methodCall)
        {
            if(!ExecuteActions)
                return Guid.NewGuid().ToString();

            var service = (T)_serviceProvider.GetService(typeof(T));
            methodCall.Compile().Invoke(service);
            return Guid.NewGuid().ToString();
        }

        public string Schedule<T>(Expression<Action<T>> methodCall, TimeSpan delay)
        {
            if(!ExecuteActions)
                return Guid.NewGuid().ToString();;

            var service = (T)_serviceProvider.GetService(typeof(T));
            methodCall.Compile().Invoke(service);
            return Guid.NewGuid().ToString();
        }

        public bool RemoveJob(string id)
        {
            return false;
        }

        public string EnqueueWithHighPriority<T>(Expression<Action<T>> methodCall)
        {
            if(!ExecuteActions)
                return Guid.NewGuid().ToString();

            var service = (T)_serviceProvider.GetService(typeof(T));
            methodCall.Compile().Invoke(service);
            return Guid.NewGuid().ToString();
        }

        public void ScheduleRecurring<T>(string recurringJobId, Expression<Action<T>> methodCall, string cronExpression)
        {
        }
    }
}