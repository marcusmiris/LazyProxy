using Castle.DynamicProxy;
using System;

namespace Miris.LazyProxy
{
    public class LazyInterceptor<T>
        : IInterceptor
        where T : class
    {
        private readonly Lazy<Lazy<T>> _lazy;

        public LazyInterceptor(Func<Lazy<T>> lazyFactory)
        {
            _lazy = new Lazy<Lazy<T>>(lazyFactory);
        }

        public void Intercept(IInvocation invocation)
        {
            if (!_lazy.IsValueCreated
                && invocation.Method.DeclaringType == typeof(IDisposable) &&
                invocation.Method.Name.Equals("Dispose"))
                return;

            var target = _lazy.Value.Value ?? throw new NullReferenceException();

            invocation.ReturnValue = invocation.GetConcreteMethod().Invoke(
                target,
                invocation.Arguments);
        }
    }
}
