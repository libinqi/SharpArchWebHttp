namespace SharpArchWebHttp
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Reflection;

    using Castle.MicroKernel;

    /// <summary>
    /// Contains extension methods that extend <see cref="IKernel" /> with property injection
    /// related features.
    /// </summary>
    public static class WindsorPropertyInjectionKernelExtensions
    {
        private static readonly object PropertyCacheLock = new object();
        private static readonly Dictionary<Type, PropertyInfo[]> PropertyCache = new Dictionary<Type, PropertyInfo[]>();

        /// <summary>
        /// Resolves the items in the specified list.
        /// </summary>
        /// <typeparam name="T">The item type.</typeparam>
        /// <param name="kernel">The kernel.</param>
        /// <param name="items">The items.</param>
        public static void ResolveProperties<T>(this IKernel kernel, IEnumerable<T> items)
        {
            if (kernel == null) throw new ArgumentNullException("kernel");
            if (items == null) throw new ArgumentNullException("items");

            foreach (var item in items)
            {
                InjectProperties(kernel, item);
            }
        }

        /// <summary>
        /// Performs property injection on the specified target.
        /// </summary>
        /// <param name="kernel">The kernel.</param>
        /// <param name="target">The target.</param>
        public static void InjectProperties(this IKernel kernel, object target)
        {
            if (kernel == null) throw new ArgumentNullException("kernel");
            if (target == null) throw new ArgumentNullException("target");

            var type = target.GetType();

            foreach (var property in GetProperties(type))
            {
                if (property.CanWrite && kernel.HasComponent(property.PropertyType))
                {
                    var value = kernel.Resolve(property.PropertyType);

                    try
                    {
                        property.SetValue(target, value, null);
                    }
                    catch (Exception exception)
                    {
                        throw new InvalidOperationException(
                            string.Format(
                                CultureInfo.CurrentCulture, 
                                "Error setting property {0} on type {1}.", 
                                property.Name, 
                                type.FullName), 
                            exception);
                    }
                }
            }
        }

        /// <summary>
        /// Gets the public instance properties of the specified type.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns>An array of <see cref="PropertyInfo" /> instances.</returns>
        private static IEnumerable<PropertyInfo> GetProperties(Type type)
        {
            lock (PropertyCacheLock)
            {
                if (!PropertyCache.ContainsKey(type))
                {
                    PropertyCache[type] = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);
                }

                return PropertyCache[type];
            }
        }
    }
}
