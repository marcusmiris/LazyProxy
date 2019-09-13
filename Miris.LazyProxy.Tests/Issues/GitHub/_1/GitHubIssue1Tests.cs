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

            var usr = proxy.GetById(Guid.NewGuid());

            Assert.IsNotNull(usr);
        }
    }


    public class UsuarioMembership { }

    #region ' interfaces '

    public interface IUsuarioRepository : IBaseRepository<UsuarioMembership>
    {
        TSubclass Get<TSubclass, T>()
            where TSubclass : UsuarioMembership, new()
            where T : class;

    }

    public interface IBaseRepository<TEntidade>
    : ISeedworkRepository<TEntidade>
    where TEntidade : class
    {
        TEntidade GetById(Guid? id);
        TSubclass GetById<TSubclass>(Guid? id) where TSubclass : TEntidade;
    }

    public interface ISeedworkRepository<TEntidade> where TEntidade : class
    {
        ISession Session { get; }
        IQueryable<TEntidade> Query { get; }
        void Alterar(TEntidade item);
        void Excluir(TEntidade item);
        void Incluir(TEntidade item);
    }

    public interface ISession { }

    #endregion

    public class UsuarioRepository
        : IUsuarioRepository
    {
        public ISession Session => null;

        public IQueryable<UsuarioMembership> Query => null;

        public void Alterar(UsuarioMembership item) { }

        public void Excluir(UsuarioMembership item) { }

        public TSubclass Get<TSubclass, T>()
            where TSubclass : UsuarioMembership, new()
            where T : class
            => null;

        public UsuarioMembership GetById(Guid? id) => new UsuarioMembership();

        public TSubclass GetById<TSubclass>(Guid? id) where TSubclass : UsuarioMembership
        {
            return null;
        }

        public void Incluir(UsuarioMembership item) { }
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

        public ISession Session => throw new NotImplementedException();

        public IQueryable<UsuarioMembership> Query => throw new NotImplementedException();

        public IUsuarioRepository GetService() => Service ?? (Service = Factory());


        public TSubclass Get<TSubclass, T>()
            where TSubclass : UsuarioMembership, new()
            where T : class
        {
            return GetService().Get<TSubclass, T>();
        }

        public UsuarioMembership GetById(Guid? id)
        {
            throw new NotImplementedException();
        }

        public TSubclass GetById<TSubclass>(Guid? id) where TSubclass : UsuarioMembership
        {
            throw new NotImplementedException();
        }

        public void Alterar(UsuarioMembership item)
        {
            throw new NotImplementedException();
        }

        public void Excluir(UsuarioMembership item)
        {
            throw new NotImplementedException();
        }

        public void Incluir(UsuarioMembership item)
        {
            throw new NotImplementedException();
        }
    }

}
