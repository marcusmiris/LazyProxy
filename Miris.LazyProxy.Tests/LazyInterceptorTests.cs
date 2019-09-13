using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace Miris.LazyProxy.Tests
{
    [TestClass]
    public class LazyInterceptorTests
    {
        [TestMethod]
        public void DisposeNonLoadedLazy()
        {
            var svc = new Service();
            Assert.AreEqual(false, svc.Disposed);

            var construiuLazy = false;

            IService svcProxy = LazyProxyGenerator.CreateLazyProxyFor<IService>(() =>
            {
                construiuLazy = true;
                return svc;
            });

            Assert.IsFalse(construiuLazy);

            // dispose não deve ser invocado!
            svcProxy.Dispose();
            Assert.IsFalse(construiuLazy);
            Assert.IsFalse(svc.Disposed);

            // chama um método para criar o lazy
            svcProxy.Do();
            Assert.IsTrue(construiuLazy);
            Assert.IsFalse(svc.Disposed);

            // chama dispose
            svcProxy.Dispose();
            Assert.IsTrue(svc.Disposed);
        }

        public interface IService
            : IDisposable
        {
            void Do();
        }

        public class Service
            : IService
        {
            public bool Disposed = false;

            public void Dispose()
            {
                Disposed = true;
            }

            public void Do()
            {

            }
            
        }
    }

    
}
