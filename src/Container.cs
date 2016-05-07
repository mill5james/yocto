﻿using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;

using static mill5.yocto.Preconditions;
// ReSharper disable InconsistentNaming

namespace mill5.yocto
{
    public class Container : IContainer
    {
        private readonly object _syncLock = new object();
        private readonly List<Container> _children = new List<Container>();

        private readonly ConcurrentDictionary<Type, IInstanceFactory> _factories =
            new ConcurrentDictionary<Type, IInstanceFactory>();

        private readonly Container _parent;

        private bool _disposed;

        static Container()
        {
            
        }

        private class Registration<T, V> : IRegistration
        {
            private readonly Container _container;

            public Registration(Container container)
            {
                CheckIsNotNull(nameof(container), container);
                _container = container;
            }

            public IRegistration Register(string lifetime)
            {
                CheckIsNotNullEmptyOrWhitespace(nameof(lifetime), lifetime);

                var interfaceType = typeof (T);
                var implementationType = typeof (V);

                _container.CreateInstanceFactory(interfaceType, implementationType, lifetime);

                return this;
            }
        }

        private Container()
        {
        }

        protected Container(Container parent)
        {
            CheckIsNotNull(nameof(parent), parent);
            _parent = parent;
        }

        ~Container()
        {
            Dispose(false);
        }

        protected void Dispose(bool dispose)
        {
            if (_disposed)
                return;

            Cleanup.SafeMethod(() =>
            {    
                _parent?.RemoveChild(this);
            
                List<Container> childrenToDispose;

                lock (_syncLock)
                {
                    childrenToDispose = _children.ToList();
                    _children.Clear();
                }

                foreach (var c in childrenToDispose)
                {
                    ((IDisposable)c).Dispose();
                }

                List<IInstanceFactory> factoriesToDispose = _factories.Values.ToList();

                foreach (var f in factoriesToDispose)
                {
                    (f as IDisposable)?.Dispose();
                }

                _factories.Clear();
            });

            _disposed = true;

            if (dispose)
                GC.SuppressFinalize(this);
        }

        public static Container Root { get; } = new Container();

        public IChildContainer GetChildContainer()
        {
            var child = new ChildContainer(this);

            lock (_syncLock)
            {
                _children.Add(child);
            }

            return child;
        }

        private void RemoveChild(Container child)
        {
            lock (_syncLock)
            {
                _children.Remove(child);
            }
        }

        public IRegistration RegisterSingleton<T, V>() where V : T
        {
            return new Registration<T, V>(this).AsSingleton();
        }

        public IRegistration Register<T,V>() where V : T
        {
            return new Registration<T, V>(this).AsMultiInstance();
        }

        public T Resolve<T>() where T : class
        {
            T instance;
            
            if (!TryResolve(out instance))
                throw new Exception("Interface type is not registered.");

            return instance;
        }

        public bool CanResolve<T>() where T : class
        {
            bool canResolve = _factories.ContainsKey(typeof (T));

            if ((!canResolve) && (_parent != null))
            {
                return _parent.CanResolve<T>();
            }

            return canResolve;
        }

        public bool TryResolve<T>(out T instance) where T : class
        {
            IInstanceFactory instanceFactory;
            instance = null;

            if (_factories.TryGetValue(typeof(T), out instanceFactory))
            {
                instance = instanceFactory.Create<T>();
            }
            else if (_parent != null)
            {
                return _parent.TryResolve(out instance);
            }

            return (instance != null);
        }

        internal bool TryGetFactory(Type interfaceType, out IInstanceFactory instanceFactory)
        {
            return _factories.TryGetValue(interfaceType, out instanceFactory);
        }

        internal bool ContainsFactory(Type interfaceType)
        {
            return _factories.ContainsKey(interfaceType);
        }

        private void CreateInstanceFactory(Type interfaceType, Type implementationType, string lifetime)
        {
            var lifetimeFactory = Lifetimes.GetLifetimeFactory(lifetime);
            var instanceFactory = lifetimeFactory.GetInstanceFactory(this, implementationType);

            _factories.AddOrUpdate(interfaceType, t => instanceFactory,
                (t, of) =>
                {
                    (of as IDisposable)?.Dispose();
                    return instanceFactory;
                });
        }
    }
}