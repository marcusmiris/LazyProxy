using System;
using Castle.DynamicProxy;

namespace Miris.LazyProxy
{
    public static class LazyProxyBuilder
    {
        public static T CreateProxyForLazy<T>(
            Func<Lazy<T>> lazyFactory)
            where T : class
            => CreateProxyForLazy(lazyFactory, new ProxyGenerator());


        public static T CreateProxyForLazy<T>(
            Func<Lazy<T>> lazyFactory,
            IProxyGenerator proxyGenerator)
            where T : class
        {
            return proxyGenerator.CreateInterfaceProxyWithoutTarget<T>(
                new LazyInterceptor<T>(lazyFactory));
        }
    }
}
