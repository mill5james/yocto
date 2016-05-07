﻿using System;

namespace mill5.yocto
{
    public interface IContainer
    {
        IChildContainer GetChildContainer();
        IRegistration Register<T,V>() where V : T;
        T Resolve<T>() where T : class;
        bool CanResolve<T>() where T : class;
        bool TryResolve<T>(out T instance) where T : class;
    }
}