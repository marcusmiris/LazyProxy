using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace Miris.LazyProxy.Tests
{
    [TestClass]
    public class PerformanceTests
    {


        [TestMethod]
        public void PerformanceTest()
        {
            for (var i = 1; i <= 1_000_000; i++)
            {
                LazyProxyGenerator.CreateLazyProxyFor<IService>(() => new Service());
            }
            Assert.IsTrue(true);
        }





        public interface IService
           : IDisposable
        {
        }

        public class Service
            : IService
        {
            private readonly int X;

            public Service()
            {
                X = int.MaxValue;
            }

            public void Dispose()
            {
            }
        }

    }
}