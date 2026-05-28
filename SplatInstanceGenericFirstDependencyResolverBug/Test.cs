using Splat;

using SplatInstanceGenericFirstDependencyResolverBug.Support;

namespace SplatInstanceGenericFirstDependencyResolverBug;

[TestClass]
public sealed class Test
{
    internal static IEnumerable<IDependencyResolver> Resolvers()
    {
        yield return new ModernDependencyResolver();
        yield return new InstanceGenericFirstDependencyResolver();
    }

    [TestMethod]
    [DynamicData(nameof(Resolvers))]
    public void NestedGetService(IDependencyResolver resolver)
    {
        resolver.RegisterLazySingleton(() => new NestingService<TestService>(resolver));

        var nesting = resolver.GetService<NestingService<TestService>>();

        Assert.IsNotNull(nesting, "Service should resolve");
        Assert.IsNull(nesting.Service, "Nested service has not been registered, and should be null");

        var service = new TestService();
        resolver.RegisterConstant(service);

        Assert.IsTrue(resolver.HasRegistration<TestService>(), "Service should have a registration now");
        Assert.AreSame(service, resolver.GetService<TestService>(), "Service should resolve to the registered instance");
    }

    [TestMethod]
    [DynamicData(nameof(Resolvers))]
    public void CallbackGetService(IDependencyResolver resolver)
    {
        var service = new TestService();

        resolver.ServiceRegistrationCallback<TestService>((d) => resolver.GetService<TestService>());

        Assert.IsNull(resolver.GetService<TestService>(), "Service should not be available before registration");

        resolver.RegisterConstant(new DummyService());
        resolver.RegisterConstant(service);

        Assert.IsTrue(resolver.HasRegistration<TestService>(), "Service should have a registration now");
        Assert.AreSame(service, resolver.GetService<TestService>(), "Service should resolve to the registered instance");
    }


    [TestMethod]
    [DynamicData(nameof(Resolvers))]
    public void TypedCallback(IDependencyResolver resolver)
    {
        var invocations = 0;
        var registrations = 0;

        resolver.ServiceRegistrationCallback<TestService>((d) => {
            ++invocations;

            if (resolver.HasRegistration<TestService>()) {
                ++registrations;
            }
        });

        resolver.RegisterConstant(new DummyService());
        resolver.RegisterConstant(new TestService());

        Assert.AreEqual(registrations, invocations, "Callback should only be executed when the service is registered");
    }
}
