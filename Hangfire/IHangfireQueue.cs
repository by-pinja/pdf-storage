using System;
using System.Linq.Expressions;

namespace Pdf.Storage.Hangfire
{
    public interface IHangfireQueue
    {
        void Enqueue<T>(Expression<Action<T>> methodCall);
    }
}