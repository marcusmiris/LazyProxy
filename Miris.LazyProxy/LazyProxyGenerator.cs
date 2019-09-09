using Miris.LazyProxy.Internals;
using System;

namespace Miris.LazyProxy
{
    public static class LazyProxyGenerator
    {

        public static T CreateLazyProxyFor<T>(Func<T> serviceFactory)
        {
            var registry = DynamicAssemblyGenerator.GetProxyTypeFor(typeof(T));

            return (T)registry.CtorInfo.Invoke(new[] { serviceFactory });
        }


        #region ' GetLazyProxyTypeFor(...) '

        /// <summary>
        ///     Get a dynamic proxy type generated to be used as lazy proxy used to 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static Type GetLazyProxyTypeFor<T>()
        {
            return GetLazyProxyTypeFor(typeof(T));
        }

        /// <summary>
        ///     Get a dynamic proxy type generated to be used as lazy proxy used to 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static Type GetLazyProxyTypeFor(Type serviceType)
        {
            return DynamicAssemblyGenerator
                .GetProxyTypeFor(serviceType)
                .TypeInfo
                ;
        }

        #endregion

    }
}
