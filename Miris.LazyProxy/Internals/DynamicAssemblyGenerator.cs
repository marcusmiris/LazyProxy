using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Security.Cryptography;
using System.Text;

namespace Miris.LazyProxy.Internals
{
    /// <summary>
    ///     Thread-safe.
    /// </summary>
    internal static class DynamicAssemblyGenerator
    {

        #region ' DynamicModuleBuilder '

        private static readonly object moduleLockObject = new object();

        private static ModuleBuilder dynamicModuleBuilder;

        public static ModuleBuilder GetDynamicModuleBuilder()
        {
            lock (moduleLockObject)
            {
                if (dynamicModuleBuilder == null)
                {
                    dynamicModuleBuilder = GetDynamicAssemblyBuilder()
                        .DefineDynamicModule(GetDynamicAssemblyName().Name);
                }
            }

            return dynamicModuleBuilder;
        }

        #endregion

        #region ' DynamicAssemblyName '

        private static readonly object assemblyNameLockObject = new object();

        private static AssemblyName dynamicAssemblyName;
        private static AssemblyName GetDynamicAssemblyName()
        {
            lock (assemblyNameLockObject)
            {
                if (dynamicAssemblyName == null) dynamicAssemblyName = new AssemblyName("Miris.LazyProxy.DynamicProxies");
            }
            return dynamicAssemblyName;
        }

        #endregion

        #region ' DynamicAssemblyBuilder '

        private static readonly object assemblyBuilderLockObject = new object();

        private static AssemblyBuilder dynamicAssemblyBuilder;
        private static AssemblyBuilder GetDynamicAssemblyBuilder()
        {
            lock (assemblyBuilderLockObject)
            {
                if (dynamicAssemblyBuilder == null)
                {
                    dynamicAssemblyBuilder = AssemblyBuilder.DefineDynamicAssembly(
                        GetDynamicAssemblyName(),
                        AssemblyBuilderAccess.Run);
                }
            }
            return dynamicAssemblyBuilder;
        }

        #endregion

        private static readonly object registryAcessLockObject = new object();

        private static ConcurrentDictionary<Type, LazyProxyTypeRegistration> RegisteredTypes
            = new ConcurrentDictionary<Type, LazyProxyTypeRegistration>();

        internal static LazyProxyTypeRegistration GetProxyTypeFor(Type serviceType)
        {
            LazyProxyTypeRegistration registry;

            lock (registryAcessLockObject)  // ver a possiblidade de locar de alguma forma baseado no tipo T.
            {
                if (!RegisteredTypes.TryGetValue(serviceType, out registry))
                {
                    return RegisterProxyFor(serviceType);
                }
            }

            return registry;
        }


        private static LazyProxyTypeRegistration RegisterProxyFor(Type serviceType)
        {
            var proxyType = CreateProxyTypeFor(serviceType);

            var registry = new LazyProxyTypeRegistration(proxyType, proxyType.GetConstructors()[0]);

            RegisteredTypes.TryAdd(
                serviceType,
                registry);

            return registry;
        }

        private static TypeInfo CreateProxyTypeFor(Type serviceType)
        {
            var serviceDelegateType = typeof(Func<>).MakeGenericType(serviceType);
            //

            var module = GetDynamicModuleBuilder();

            var typeBuilder = module.DefineType(
                $"{ GetDynamicAssemblyName().Name }.{ serviceType.Name }Proxy_{ ComputeHashFor(serviceType) }",
                TypeAttributes.Public | TypeAttributes.Class);
            {
                // members
                var _serviceFactoryFB = typeBuilder.DefineField("_serviceFactory", serviceDelegateType, FieldAttributes.Private);
                var _serviceFB = typeBuilder.DefineField("_service", serviceType, FieldAttributes.Private);

                #region ' ctor '
                var ctorBuilder = typeBuilder.DefineConstructor(MethodAttributes.Public, CallingConventions.HasThis, new[] { serviceDelegateType });
                {
                    ctorBuilder.DefineParameter(1, ParameterAttributes.In, "serviceFactory");

                    var ctorIL = ctorBuilder.GetILGenerator();

                    // chama ctor da base class
                    ctorIL.Emit(OpCodes.Ldarg_0);   // push "this" onto stack.
                    ctorIL.Emit(OpCodes.Call, typeof(object).GetConstructor(BindingFlags.Public | BindingFlags.FlattenHierarchy | BindingFlags.Instance, null, new Type[0], null));     // call base constructor
                    ctorIL.Emit(OpCodes.Nop);
                    ctorIL.Emit(OpCodes.Nop);

                    // seta atributo privado;
                    ctorIL.Emit(OpCodes.Ldarg_0);   // push "this" onto stack.
                    ctorIL.Emit(OpCodes.Ldarg_1);   // push "this" onto stack.s
                    ctorIL.Emit(OpCodes.Stfld, _serviceFactoryFB);
                    ctorIL.Emit(OpCodes.Ret);
                }
                #endregion

                // https://sharplab.io/#v2:C4LglgNgNAJiDUAfAAgJgIwFgBQOB2AhgLYCmAzgA4EDGJABAGYD2TdA3jnV3Z98gMx00dACIBPQkTDUACgCcmADzG9u3EHQCSAZRJyAbtJKr2JtRTlh9BYPWQBWADw69h2gD46AfTKujAMRpgJjkxAG4zbgsrG3oXAyNvXwTaCNxsNTUBUQliaXklMQAKB2ddFJJPZLcSQOpg0IBKSK4ObABITp8/WjqGsToAXjpqgKCQ8Ja6AF8cKejrWy1ymroAcRJgFaMixtMMzMOwBjoi7oqh4bwAVwgIPfPV4cex+ondtMPD5AB2JJ6SJ8vrN0l8hIJkAAWURMXZTNpgtQbLYA3YAOhEsMaQMOIKm2ShojAlCYvjhB0OCMRXGR21o6JExIopJIHymILUeIp4LoYDwtjkDBocTpxm5Xw0mkZJLIBAARhAxWoqd9oZi2dyuVkIag6KKppL9dyVZkFrE6HISAQYEw8BABnzgHQABppfGCUXksEmsHOoa8/logCyBEUADUCBBroD2XNxXwIWqsewZnGwQS1UyWV6vj7uBz8yY49MgA

                #region ' private IService GetService() => _service ?? (_service = _serviceFactory()) '

                var getService = typeBuilder.DefineMethod(
                    "GetService",
                    MethodAttributes.Private | MethodAttributes.HideBySig,
                    serviceType,
                    new Type[0]);
                {
                    var ilg = getService.GetILGenerator();

                    var serviceIsCreated = ilg.DefineLabel();

                    ilg.Emit(OpCodes.Nop);

                    #region ' if (_service == null) '

                    ilg.Emit(OpCodes.Ldarg_0);                      // load "this"
                    ilg.Emit(OpCodes.Ldfld, _serviceFB);            // load "_service" on the evaluation stack.
                    ilg.Emit(OpCodes.Brtrue_S, serviceIsCreated);   // se possui valor, pula para o label "ServiceIsCreated".

                    #endregion
                    #region ' {   _service = serviceFactory() } '

                    ilg.Emit(OpCodes.Ldarg_0);
                    ilg.Emit(OpCodes.Ldarg_0);
                    ilg.Emit(OpCodes.Ldfld, _serviceFactoryFB);                                 // load pointer to "_serviceFactory" on the evaluation stack
                    ilg.EmitCall(OpCodes.Callvirt, serviceDelegateType.GetMethod("Invoke"), null);  // invokes for the reference in the evaluation stack;
                    ilg.Emit(OpCodes.Stfld, _serviceFB);                                        // joga resultado no field "_serviceFB";

                    #endregion

                    #region ' return _service; '

                    ilg.Emit(OpCodes.Nop);
                    ilg.MarkLabel(serviceIsCreated);        // label

                    ilg.Emit(OpCodes.Ldarg_0);              // carrega "this"
                    ilg.Emit(OpCodes.Ldfld, _serviceFB);    // carrega field "_service"
                    ilg.Emit(OpCodes.Ret);                  // saí da rotina.

                    #endregion

                }

                #endregion

                // implementa métodos
                var interfacesToImplement = new[] { serviceType }.Concat(serviceType.GetInterfaces()).ToArray();
                foreach (var @interface in interfacesToImplement)
                {
                    typeBuilder.AddInterfaceImplementation(@interface);

                    foreach (var target in @interface.GetMethods())
                    {
                        var methodBuilder = typeBuilder.DefineMethod(
                            $"{@interface.Name}.{target.Name}", //target.Name,
                            MethodAttributes.Public | MethodAttributes.Final | MethodAttributes.HideBySig | MethodAttributes.NewSlot | MethodAttributes.Virtual,
                            target.ReturnType,
                            target.GetParameters().Select(p => p.ParameterType).ToArray());
                        {
                            #region ' define argumentos genéricos... '
                            {
                                var targetGenericArguments = target.GetGenericArguments();
                                if (targetGenericArguments.Length > 0)
                                {
                                    var genericParameterNames = targetGenericArguments.Select(_ => _.Name).ToArray();
                                    var genericParameterBuilders = methodBuilder.DefineGenericParameters(genericParameterNames);

                                    for (var i = 0; i < targetGenericArguments.Length; i++)
                                    {
                                        var targetBuilder = targetGenericArguments[i];
                                        //var interfaceConstraints = targetBuilder.GetGenericParameterConstraints();

                                        //if (interfaceConstraints.Length > 0)
                                        //{
                                        //    genericParameterBuilders[i].SetBaseTypeConstraint(interfaceConstraints[0]);
                                        //    genericParameterBuilders[i].SetInterfaceConstraints(interfaceConstraints);
                                        //}
                                        genericParameterBuilders[i].SetGenericParameterAttributes(targetBuilder.GetTypeInfo().GenericParameterAttributes);
                                    }
                                }
                            }
                            #endregion


                            var ilg = methodBuilder.GetILGenerator();

                            #region ' gera instrução " [return] getService().target() " '
                            ilg.Emit(OpCodes.Nop);

                            // IDisposable.Dispose() isn't proxied...
                            if (target.DeclaringType != typeof(IDisposable) && !target.Name.Equals("Dispose"))
                            {
                                ilg.Emit(OpCodes.Ldarg_0);
                                ilg.EmitCall(OpCodes.Call, getService, null);

                                // coloca cada um dos argumentos no evaluation stack.
                                for (var i = 1; i <= target.GetParameters().Length; i++)
                                {
                                    ilg.Emit(OpCodes.Ldarg, i);
                                }

                                ilg.EmitCall(OpCodes.Callvirt, target, null);

                            }

                            ilg.Emit(OpCodes.Ret);
                            #endregion
                        }

                        typeBuilder.DefineMethodOverride(methodBuilder, target);
                    }
                }

                // implements disposable.
                // The disposable method is not proxied.


            };

            return typeBuilder.CreateTypeInfo();
        }

        /// <summary>
        ///     Computes a hash aiming to differentiate homonymous types.
        /// </summary>
        private static string ComputeHashFor(Type serviceType)
        {
            char[] notValidChars = new[] { '=', '/', '+' };
            string computed;
            string hashKey = serviceType.AssemblyQualifiedName;

            // hash
            using (var hashAlgorithm = SHA256.Create())
            {
                var enconding = Encoding.UTF8;
                computed = Convert.ToBase64String(hashAlgorithm.ComputeHash(enconding.GetBytes(hashKey)));
            }

            // removes invalid chars.
            var stringBuilder = new StringBuilder();
            foreach (var @char in computed)
            {
                if (notValidChars.Contains(@char)) continue;
                stringBuilder.Append(@char);
            }

            return stringBuilder.ToString();
        }

    }
}
