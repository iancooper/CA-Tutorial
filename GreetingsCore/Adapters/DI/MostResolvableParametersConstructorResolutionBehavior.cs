using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Reflection;
using SimpleInjector;
using SimpleInjector.Advanced;

//See: https://simpleinjector.readthedocs.io/en/latest/extensibility.html#overriding-constructor-resolution-behavior
//Frankly single constructor only is just nonsense. But folks want to use SimpleInjector, so we just override its
//behaviour everywhere

namespace GreetingsCore.Adapters.DI
{
    public class MostResolvableParametersConstructorResolutionBehavior : IConstructorResolutionBehavior
    {
        private readonly Container container;

        public MostResolvableParametersConstructorResolutionBehavior(Container container)
        {
            this.container = container;
        }

        private bool IsCalledDuringRegistrationPhase => !container.IsLocked();

        [DebuggerStepThrough]
        public ConstructorInfo GetConstructor(Type implementationType)
        {
            var constructor = GetConstructors(implementationType).FirstOrDefault();
            if (constructor != null) return constructor;
            throw new ActivationException(BuildExceptionMessage(implementationType));
        }

        private IEnumerable<ConstructorInfo> GetConstructors(Type implementation)
        {
            return from ctor in implementation.GetConstructors()
                let parameters = ctor.GetParameters()
                where IsCalledDuringRegistrationPhase
                      || implementation.GetConstructors().Length == 1
                      || ctor.GetParameters().All(CanBeResolved)
                orderby parameters.Length descending
                select ctor;
        }

        private bool CanBeResolved(ParameterInfo parameter)
        {
            return GetInstanceProducerFor(new InjectionConsumerInfo(parameter)) != null;
        }

        private InstanceProducer GetInstanceProducerFor(InjectionConsumerInfo i)
        {
            return container.Options.DependencyInjectionBehavior.GetInstanceProducer(i, false);
        }

        private static string BuildExceptionMessage(Type type)
        {
            return !type.GetConstructors().Any()
                ? TypeShouldHaveAtLeastOnePublicConstructor(type)
                : TypeShouldHaveConstructorWithResolvableTypes(type);
        }

        private static string TypeShouldHaveAtLeastOnePublicConstructor(Type type)
        {
            return string.Format(CultureInfo.InvariantCulture,
                "For the container to be able to create {0}, it should contain at least " +
                "one public constructor.", type.ToFriendlyName());
        }

        private static string TypeShouldHaveConstructorWithResolvableTypes(Type type)
        {
            return string.Format(CultureInfo.InvariantCulture,
                "For the container to be able to create {0}, it should contain a public " +
                "constructor that only contains parameters that can be resolved.",
                type.ToFriendlyName());
        }
    }
}