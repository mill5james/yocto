﻿using System;
using System.Diagnostics;

namespace yocto.tests
{
    public class LoggingLifetimeFactory : ILifetimeFactory
    {
        private readonly string _lifetime;
        private readonly ILifetimeFactory _innerLifetimeFactory;

        public LoggingLifetimeFactory(string lifetime, ILifetimeFactory innerLifetimeFactory, Func<object> factory)
        {
            Preconditions.CheckIsNotNullEmptyOrWhitespace(nameof(lifetime), lifetime);
            Preconditions.CheckIsNotNull(nameof(innerLifetimeFactory), innerLifetimeFactory);

            _lifetime = lifetime;
            _innerLifetimeFactory = innerLifetimeFactory;
        }

        public IInstanceFactory GetInstanceFactory(IContainer container, Type interfaceType, Type implementationType, Func<object> factory, object[] values)
        {
            var instanceFactory = _innerLifetimeFactory.GetInstanceFactory(container, interfaceType, implementationType, factory, values);

            Debug.WriteLine($"{implementationType.Name} using {_lifetime} with {instanceFactory.GetType().Name}");

            return instanceFactory;
        }
    }
}
