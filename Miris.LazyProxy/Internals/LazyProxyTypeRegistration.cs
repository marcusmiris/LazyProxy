using System.Reflection;

namespace Miris.LazyProxy.Internals
{
    internal class LazyProxyTypeRegistration
    {
        public LazyProxyTypeRegistration(
            TypeInfo typeInfo,
            ConstructorInfo ctorInfo)
        {
            TypeInfo = typeInfo;
            CtorInfo = ctorInfo;
        }

        public TypeInfo TypeInfo { get; }
        public ConstructorInfo CtorInfo { get; }
    }
}
