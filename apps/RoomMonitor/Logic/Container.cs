namespace Dashboard.Logic
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// IoC container
    /// </summary>
    internal class Container
    {
        /// <summary>
        /// The dictionary containing all referenes.
        /// </summary>
        private IDictionary<string, object> m_NamedReferences = new Dictionary<string, object>();

        /// <summary>
        /// Registers a new instance of the specified type.
        /// </summary>
        /// <typeparam name="T">The type of the instance</typeparam>
        /// <param name="instance">The instance to register for IoC</param>
        /// <exception cref="ArgumentException">Thrown in case an instance of <typeparamref name="T"/> is already registered.</exception>
        public void Register<T>(T instance)
            where T : class
            => RegisterNamed(instance.GetType().Name, instance);

        /// <summary>
        /// Registers a new instance of the specified type using a specific name.
        /// </summary>
        /// <typeparam name="T">The type of the instance</typeparam>
        /// <param name="name">The name of the registered instance</param>
        /// <param name="instance">The instance to register for IoC</param>
        /// <exception cref="ArgumentException">Thrown in case an instance with the name: <paramref name="name"/> is already registered.</exception>
        public void RegisterNamed<T>(string name, T instance)
        {
            if (m_NamedReferences.ContainsKey(name))
            {
                throw new ArgumentException("Instance with the same name already registered!", nameof(name));
            }
            m_NamedReferences.Add(name, instance);
        }

        /// <summary>
        /// Resolves a nameless instance.
        /// </summary>
        /// <typeparam name="T">The type of the instance</typeparam>
        /// <returns>The instance if registered, else <c>null</c></returns>
        public T Resolve<T>()
            where T : class
            => ResolveNamed<T>(typeof(T).Name);

        /// <summary>
        /// Resolves the instance by name.
        /// </summary>
        /// <typeparam name="T">The type of the instance</typeparam>
        /// <param name="name">The name of the instance</param>
        /// <returns>The instance if registered, else <c>null</c></returns>
        public T ResolveNamed<T>(string name)
            where T : class
        {
            object instance;
            if (!m_NamedReferences.TryGetValue(name, out instance))
            {
                return null;
            }
            return instance as T;
        }
    }
}
