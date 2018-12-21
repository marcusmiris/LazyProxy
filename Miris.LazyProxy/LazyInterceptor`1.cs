using Castle.DynamicProxy;
using System;

namespace Miris.LazyProxy
{
    public class LazyInterceptor<T>
        : IInterceptor
    {
        private readonly Lazy<Lazy<T>> _lazy;

        public LazyInterceptor(Func<Lazy<T>> lazyFactory)
        {
            _lazy = new Lazy<Lazy<T>>(lazyFactory);
        }

        public void Intercept(IInvocation invocation)
        {
            var target = _lazy.Value.Value;

            invocation.ReturnValue = invocation.GetConcreteMethod().Invoke(
                target,
                invocation.Arguments);
        }
    }
}
