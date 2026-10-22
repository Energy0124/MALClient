using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace MALClient.XShared.Ioc
{
    /// <summary>
    ///     Drop-in replacement for MvvmLight's SimpleIoc (which is no longer maintained) covering the subset this app
    ///     uses: lazy singletons, interface to class mappings, factories and constructor injection.
    /// </summary>
    public class SimpleIoc
    {
        public static SimpleIoc Default { get; } = new SimpleIoc();

        private readonly object _syncLock = new object();
        private readonly Dictionary<Type, Type> _interfaceToClassMap = new Dictionary<Type, Type>();
        private readonly Dictionary<Type, Func<object>> _factories = new Dictionary<Type, Func<object>>();
        private readonly Dictionary<Type, object> _instances = new Dictionary<Type, object>();

        public void Register<TClass>() where TClass : class
        {
            var classType = typeof(TClass);
            if (classType.GetTypeInfo().IsInterface)
                throw new ArgumentException("An interface cannot be registered alone.");

            lock (_syncLock)
            {
                if (_factories.ContainsKey(classType))
                    return;

                _interfaceToClassMap[classType] = classType;
                _factories[classType] = () => MakeInstance(classType);
            }
        }

        public void Register<TInterface, TClass>() where TInterface : class where TClass : class, TInterface
        {
            var interfaceType = typeof(TInterface);
            var classType = typeof(TClass);

            lock (_syncLock)
            {
                if (_interfaceToClassMap.TryGetValue(interfaceType, out var registeredType))
                {
                    if (registeredType != classType)
                        throw new InvalidOperationException(
                            $"There is already a class registered for {interfaceType.FullName}.");
                    return;
                }

                _interfaceToClassMap[interfaceType] = classType;
                _factories[interfaceType] = () => MakeInstance(classType);
            }
        }

        public void Register<TClass>(Func<TClass> factory) where TClass : class
        {
            if (factory == null)
                throw new ArgumentNullException(nameof(factory));

            var classType = typeof(TClass);
            lock (_syncLock)
            {
                if (_factories.ContainsKey(classType))
                    throw new InvalidOperationException(
                        $"There is already a factory registered for {classType.FullName}");

                _interfaceToClassMap[classType] = null;
                _factories[classType] = factory;
            }
        }

        public bool IsRegistered<T>()
        {
            lock (_syncLock)
            {
                return _interfaceToClassMap.ContainsKey(typeof(T));
            }
        }

        public void Unregister<TClass>() where TClass : class
        {
            var serviceType = typeof(TClass);
            lock (_syncLock)
            {
                _interfaceToClassMap.Remove(serviceType);
                _factories.Remove(serviceType);
                _instances.Remove(serviceType);
            }
        }

        public TService GetInstance<TService>()
        {
            return (TService) GetInstance(typeof(TService));
        }

        public object GetInstance(Type serviceType)
        {
            Func<object> factory;
            lock (_syncLock)
            {
                if (_instances.TryGetValue(serviceType, out var instance))
                    return instance;

                if (!_factories.TryGetValue(serviceType, out factory))
                    throw new InvalidOperationException($"Type not found in cache: {serviceType.FullName}.");
            }

            //create outside of the lock, constructors and factories resolve their own dependencies
            var created = factory();
            lock (_syncLock)
            {
                if (_instances.TryGetValue(serviceType, out var instance))
                    return instance;

                _instances[serviceType] = created;
                return created;
            }
        }

        private object MakeInstance(Type classType)
        {
            var constructor = GetConstructor(classType);
            var parameters = constructor.GetParameters()
                .Select(parameter => GetInstance(parameter.ParameterType))
                .ToArray();
            return constructor.Invoke(parameters);
        }

        private static ConstructorInfo GetConstructor(Type classType)
        {
            var constructors = classType.GetTypeInfo().DeclaredConstructors
                .Where(constructor => constructor.IsPublic && !constructor.IsStatic)
                .ToList();

            if (constructors.Count == 0)
                throw new InvalidOperationException($"No public constructor found for {classType.FullName}.");

            return constructors.Count == 1
                ? constructors[0]
                : constructors.FirstOrDefault(constructor => constructor.GetCustomAttributes()
                      .Any(attribute => attribute.GetType().Name == "PreferredConstructorAttribute")) ??
                  constructors.OrderByDescending(constructor => constructor.GetParameters().Length).First();
        }
    }
}
