using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;

namespace Miris.LazyProxy.Tests
{
    [TestClass]
    public class v2Testes
    {

        [TestMethod]
        public void GeneratedDynamicProxyTest()
        {
            var proxy = LazyProxyGenerator.CreateLazyProxyFor<IService>(() => new Service(0));

            try
            {
                proxy.Increments(); // void method;

                var valor = proxy.GetValue();
                Assert.AreEqual(1, valor);

                // Readonly Property
                Assert.AreEqual("ReadonlyProperty", proxy.ReadonlyProperty);

                // Normal property
                proxy.Key = "Olá";
                Assert.AreEqual("Olá", proxy.Key);

                var result = proxy.IncrementsWith(2, 3, 6, 12);
                Assert.AreEqual(24, result);

                // out parameter
                proxy.GetVAlue(out valor);
                Assert.AreEqual(valor, 24);

                // varios out's e com valor de retorno;
                var boolResult = proxy.GetOneTwoThree(out var one, out var two, out var three);
                Assert.IsTrue(boolResult);
                Assert.IsNotNull(one); Assert.IsNotNull(two); Assert.IsNotNull(three);
                Assert.AreEqual(1, one.GetValue());
                Assert.AreEqual(2, two.GetValue());
                Assert.AreEqual(3, three.GetValue());

                // reference parameter
                IService metade = null; new Service(0);
                proxy.Half(ref metade);
                Assert.AreEqual(12, metade.GetValue());

                // varios ref's e com valor de retorno.
                var zero = new Service(0);
                IService four = zero, five = zero, siz = zero;
                boolResult = proxy.GetQuatroCincoSeis(ref four, ref five, ref siz);
                Assert.IsTrue(boolResult);
                Assert.IsFalse(ReferenceEquals(four, zero)); Assert.AreEqual(4, four.GetValue());
                Assert.IsFalse(ReferenceEquals(five, zero)); Assert.AreEqual(5, five.GetValue());
                Assert.IsFalse(ReferenceEquals(siz, zero)); Assert.AreEqual(6, siz.GetValue());

                // params
                result = proxy.IncrementParams(1, 2, 3);
                Assert.AreEqual(30, result);

                proxy.WriteonlyProperty = 10;
                Assert.AreEqual(10, proxy.GetValue());

                var objResult = proxy.GetIncrementedObj();
                Assert.AreNotEqual(objResult, proxy);
                Assert.AreEqual(11, objResult.GetValue());
            }
            catch (Exception ex)
            {
                var y = ex;
                throw;
            }


        }

        [TestMethod]
        public void GenericInterfaceImplementation()
        {
            var proxy = LazyProxyGenerator.CreateLazyProxyFor<IGenericInterface<int>>(() => new GenericInterface<int>(() => 10));

            var atual = proxy.Cache;
            Assert.AreEqual(default, atual);

            var depois = proxy.GetObject();
            Assert.AreEqual(10, depois);
        }

        [TestMethod]
        public void GeneratedDynamicProxyInheritanceFromGenericTest()
        {
            var proxy = LazyProxyGenerator.CreateLazyProxyFor<IGenericInterface<int>>(() => new GenericInterface<int>(() => 10));

            var atual = proxy.Cache;
            Assert.AreEqual(default, atual);

            var depois = proxy.GetObject();
            Assert.AreEqual(10, depois);
        }
    }


    #region ' IService '

    public interface IService
    {
        void Increments();  // Action;
        void SetValue(int newValue);    // Action c/ argumento

        int GetValue();     // Func<?>

        int IncrementsWith(int a, int b, int c, int d); // func com argumento

        int IncrementParams(params int[] valores);     // params

        void GetVAlue(out int valor);   //out

        bool GetOneTwoThree(out IService um, out IService dois, out IService tres);

        void Half(ref IService resultado);

        bool GetQuatroCincoSeis(ref IService quatro, ref IService cinco, ref IService seis);

        string Key { get; set; }

        string ReadonlyProperty { get; }

        int WriteonlyProperty { set; }

        IService GetIncrementedObj();
    }

    public class Service
        : IService
    {
        private int Valor;

        public Service(int seed)
        {
            Valor = seed;
        }

        public string ReadonlyProperty => nameof(ReadonlyProperty);

        public string Key { get; set; }

        public string PrivateSetProperty => throw new NotImplementedException();

        public int WriteonlyProperty { set => Valor = value; }

        public IService GetIncrementedObj() => new Service(Valor + 1);

        public bool GetQuatroCincoSeis(ref IService quatro, ref IService cinco, ref IService seis)
        {
            quatro = new Service(4);
            cinco = new Service(5);
            seis = new Service(6);
            return true;
        }

        public bool GetOneTwoThree(out IService um, out IService dois, out IService tres)
        {
            um = new Service(1);
            dois = new Service(2);
            tres = new Service(3);
            return true;
        }

        public int GetValue() => Valor;

        public void GetVAlue(out int valor)
        {
            valor = Valor;
        }

        public void Increments() => Valor += 1;

        public int IncrementParams(params int[] valores)
        {
            return Valor += valores.Sum();
        }

        public int IncrementsWith(int a, int b, int c, int d)
        {
            return Valor += a + b + c + d;
        }

        public void Half(ref IService resultado)
        {
            resultado = new Service(Valor / 2);
        }

        public void SetValue(int newValue) => Valor = newValue;
    }

    #endregion

    #region ' IGenericInterface '

    public interface IGenericInterface<T>
    {
        T Cache { get; }

        T GetObject();
    }

    public class GenericInterface<T>
        : IGenericInterface<T>
    {
        private readonly Func<T> factory;

        public GenericInterface(Func<T> factory)
        {
            this.factory = factory;
        }

        #region ' IGenericInterface<T> '

        public T Cache { get; private set; }

        public T GetObject()
        {
            if (Cache.Equals(default(T)))
            {
                Cache = factory();
            }
            return this.Cache;
        }

        #endregion
    }

    #endregion

    public interface IIntService
        : IGenericInterface<int>
    {

    }






}
