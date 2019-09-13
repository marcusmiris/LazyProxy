using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Miris.LazyProxy.Tests.Issues.GitHub._1
{
    /// <summary>
    ///     https://github.com/marcusmiris/LazyProxy/issues/1
    /// </summary>
    [TestClass]
    public class GitHubIssue1Tests
    {
        [TestMethod]
        public void GitHubIssue1Test()
        {
            var proxy = LazyProxyGenerator.CreateLazyProxyFor<IUsuarioRepository>(() => new UsuarioRepository());

            //var usr = proxy.GetById(Guid.NewGuid());

            //Assert.IsNotNull(usr);
        }
    }


    public class UsuarioMembership { }

    #region ' interfaces '

    public interface IUsuarioRepository //: IBaseRepository<UsuarioMembership>
    {
        TSubclass Get<TSubclass, T>()
            where TSubclass : UsuarioMembership, new()
            where T : class;
    }

    #endregion

    public class UsuarioRepository
        : IUsuarioRepository
    {
        public TSubclass Get<TSubclass, T>()
            where TSubclass : UsuarioMembership, new()
            where T : class
        {
            throw new NotImplementedException();
        }
    }

    public class UsuarioRepositoryProxy
        : IUsuarioRepository
    {

        public UsuarioRepositoryProxy(Func<IUsuarioRepository> factory)
        {
            Factory = factory;
        }

        public Func<IUsuarioRepository> Factory { get; }
        public IUsuarioRepository Service { get; private set; }

        public IUsuarioRepository GetService() => Service ?? (Service = Factory());


        public TSubclass Get<TSubclass, T>()
            where TSubclass : UsuarioMembership, new()
            where T : class
        {
            return GetService().Get<TSubclass, T>();
        }
    }

}
