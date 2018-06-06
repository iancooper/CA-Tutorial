using System;
using Paramore.Brighter;
using SimpleInjector;

namespace GreetingsCore.Adapters.BrighterFactories
{
    public class ServicesHandlerFactoryAsync : IAmAHandlerFactoryAsync
    {
        private readonly Container _serviceProvider;

        public ServicesHandlerFactoryAsync(Container serviceProvider)
        {
            _serviceProvider = serviceProvider;

        }
        public IHandleRequestsAsync Create(Type handlerType)
        {
            return _serviceProvider.GetInstance(handlerType) as IHandleRequestsAsync;
        }

        public void Release(IHandleRequestsAsync handler)
        {
            var disposable = handler as IDisposable;
            disposable?.Dispose();
        }
    }


    public class ServicesHandlerFactory : IAmAHandlerFactory
    {
        private readonly Container _serviceProvider;

        public ServicesHandlerFactory(Container serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public IHandleRequests Create(Type handlerType)
        {
            return _serviceProvider.GetInstance(handlerType) as IHandleRequests;
        }

        public void Release(IHandleRequests handler)
        {
            var disposable = handler as IDisposable;
            disposable?.Dispose();
        }
    }
}