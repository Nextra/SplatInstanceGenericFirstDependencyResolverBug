using Splat;

namespace SplatInstanceGenericFirstDependencyResolverBug.Support;

interface INestingService<T> : INestingService;

interface INestingService;

class NestingService<T> : INestingService<T>
{
    public T? Service { get; }

    public NestingService(IDependencyResolver resolver)
    {
        Service = resolver.GetService<T>();
    }
}