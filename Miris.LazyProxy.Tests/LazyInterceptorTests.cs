using System;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Miris.LazyProxy.Tests
{
    [TestClass]
    public class LazyInterceptorTests
    {
        [TestMethod]
        public void DisposeNonLoadedLazy()
        {
            var svc = LazyProxyBuilder.CreateProxyForLazy<IService>(() => throw new AbandonedMutexException());
            try
            {
                svc.Dispose();
            }
            catch (AbandonedMutexException)
            {
                Assert.Fail("Não deveria ter gerado exception.");
            }
        }

        public interface IService
            : IDisposable
        {
        }
    }

    
}
