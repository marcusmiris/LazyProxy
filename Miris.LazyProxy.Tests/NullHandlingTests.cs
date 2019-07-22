using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace Miris.LazyProxy.Tests
{
    [TestClass]
    public class NullHandlingTests
    {

        [TestMethod]
        [ExpectedException(typeof(NullReferenceException))]
        public void ThrowCorrectNullReferenceException()
        {
            var svc = LazyProxyBuilder.CreateProxyForLazy<IService>(() => null);

            svc.Do();
        }

        [TestMethod]
        [ExpectedException(typeof(NullReferenceException))]
        public void ThrowCorrectLazyNullReferenceException()
        {
            var svc = LazyProxyBuilder.CreateProxyForLazy<IService>(() => new Lazy<IService>(() => null));

            svc.Do();
        }

        public interface IService
            : IDisposable
        {
            void Do();
        }

    }
}
