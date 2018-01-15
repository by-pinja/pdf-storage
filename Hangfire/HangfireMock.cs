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

        public virtual void Enqueue<T>(Expression<Action<T>> methodCall)
        {
            if(!ExecuteActions)
                return;

            var service = (T)_serviceProvider.GetService(typeof(T));
            methodCall.Compile().Invoke(service);
        }

        public void Schedule<T>(Expression<Action<T>> methodCall, TimeSpan delay)
        {
            if(!ExecuteActions)
                return;

            var service = (T)_serviceProvider.GetService(typeof(T));
            methodCall.Compile().Invoke(service);
        }
    }
}