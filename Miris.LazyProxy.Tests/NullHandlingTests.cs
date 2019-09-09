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
            var svc = LazyProxyGenerator.CreateLazyProxyFor<IService>(() => null);

            svc.Do();
        }

        public interface IService
            : IDisposable
        {
            void Do();
        }

    }
}
