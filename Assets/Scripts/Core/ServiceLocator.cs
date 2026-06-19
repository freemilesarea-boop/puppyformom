using System;
using System.Collections.Generic;

namespace PuppyForMom.Core
{
    /// <summary>
    /// Tiny service registry so systems can find each other without hard singletons everywhere.
    /// Managers register themselves in Awake and unregister in OnDestroy.
    /// </summary>
    public static class ServiceLocator
    {
        private static readonly Dictionary<Type, object> Services = new Dictionary<Type, object>();

        public static void Register<T>(T service) where T : class
        {
            Services[typeof(T)] = service;
        }

        public static void Unregister<T>() where T : class
        {
            Services.Remove(typeof(T));
        }

        public static T Get<T>() where T : class
        {
            return Services.TryGetValue(typeof(T), out var s) ? s as T : null;
        }

        public static bool TryGet<T>(out T service) where T : class
        {
            service = Get<T>();
            return service != null;
        }

        public static void Clear() => Services.Clear();
    }
}
