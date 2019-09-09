using System;
using System.Runtime;

namespace Miris.LazyProxy.PerformanceSample
{
    class Program
    {
        static void Main(string[] args)
        {

            try
            {
                GCSettings.LatencyMode = GCLatencyMode.Batch;
                GC.TryStartNoGCRegion(150 * 1024 * 1024, false);

                Console.WriteLine("== Memory consumption ==");
                Console.WriteLine();

                for (var i = 1; i <= 90_000; i++)
                {
                    var x = LazyProxyGenerator.CreateLazyProxyFor<IService>(() => new Service());

                    x.Incrementa(); // instance is created here!

                    if (i % 2000 == 0)
                    {
                        Console.WriteLine($"{ i.ToString("0.0,0").PadLeft(10) } >> { (GC.GetTotalMemory(false) / 1024).ToString("0.0,0") } Kb");
                    }
                }
            }
            finally
            {
                Console.ReadLine();

                GC.EndNoGCRegion();
            }


        }

    }

    public interface IService
            : IDisposable
    {
        void Incrementa();
    }

    public class Service
        : IService
    {
        private int X;

        public Service()
        {
            X = 0;
        }

        public void Incrementa() => X += 1;


        public void Dispose()
        {
        }
    }
}
