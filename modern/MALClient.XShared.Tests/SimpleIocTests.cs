using System;
using MALClient.XShared.Ioc;
using Xunit;

namespace MALClient.XShared.Tests
{
    public class SimpleIocTests
    {
        private interface IService
        {
        }

        private class Service : IService
        {
        }

        private class OtherService : IService
        {
        }

        private class Consumer
        {
            public Consumer(IService service)
            {
                Service = service;
            }

            public IService Service { get; }
        }

        private class MultipleConstructors
        {
            public MultipleConstructors()
            {
            }

            public MultipleConstructors(IService service)
            {
                Service = service;
            }

            public IService Service { get; }
        }

        [Fact]
        public void InstancesAreLazySingletons()
        {
            var ioc = new SimpleIoc();
            var created = 0;
            ioc.Register<IService>(() =>
            {
                created++;
                return new Service();
            });

            Assert.Equal(0, created);
            Assert.Same(ioc.GetInstance<IService>(), ioc.GetInstance<IService>());
            Assert.Equal(1, created);
        }

        [Fact]
        public void InterfaceMappingWithConstructorInjection()
        {
            var ioc = new SimpleIoc();
            ioc.Register<IService, Service>();
            ioc.Register<Consumer>();

            var consumer = ioc.GetInstance<Consumer>();

            Assert.IsType<Service>(consumer.Service);
            Assert.Same(ioc.GetInstance<IService>(), consumer.Service);
        }

        [Fact]
        public void GreediestConstructorIsUsedWhenThereAreSeveral()
        {
            var ioc = new SimpleIoc();
            ioc.Register<IService, Service>();
            ioc.Register<MultipleConstructors>();

            Assert.NotNull(ioc.GetInstance<MultipleConstructors>().Service);
        }

        [Fact]
        public void RegisteringSameMappingTwiceIsIgnored()
        {
            var ioc = new SimpleIoc();
            ioc.Register<IService, Service>();
            ioc.Register<IService, Service>();
            ioc.Register<Service>();
            ioc.Register<Service>();

            Assert.IsType<Service>(ioc.GetInstance<IService>());
        }

        [Fact]
        public void RegisteringDifferentMappingThrows()
        {
            var ioc = new SimpleIoc();
            ioc.Register<IService, Service>();

            Assert.Throws<InvalidOperationException>(() => ioc.Register<IService, OtherService>());
        }

        [Fact]
        public void RegisteringFactoryTwiceThrows()
        {
            var ioc = new SimpleIoc();
            ioc.Register<IService>(() => new Service());

            Assert.Throws<InvalidOperationException>(() => ioc.Register<IService>(() => new Service()));
        }

        [Fact]
        public void UnregisteredTypeThrows()
        {
            Assert.Throws<InvalidOperationException>(() => new SimpleIoc().GetInstance<IService>());
        }

        [Fact]
        public void UnregisterAllowsRegisteringAgain()
        {
            var ioc = new SimpleIoc();
            var first = new Service();
            var second = new Service();
            ioc.Register<Service>(() => first);
            Assert.Same(first, ioc.GetInstance<Service>());
            Assert.True(ioc.IsRegistered<Service>());

            ioc.Unregister<Service>();
            Assert.False(ioc.IsRegistered<Service>());

            ioc.Register<Service>(() => second);
            Assert.Same(second, ioc.GetInstance<Service>());
        }

        [Fact]
        public void InterfacesCannotBeRegisteredAlone()
        {
            Assert.Throws<ArgumentException>(() => new SimpleIoc().Register<IService>());
        }
    }
}
