namespace SharpArchWebHttp
{
    using System;
    using System.Linq;
    using System.Reflection;

    using Castle.Core;
    using Castle.MicroKernel.Registration;
    using Castle.Windsor;

    /// <summary>
    /// Contains Castle Windsor related HTTP controller extension methods.
    /// </summary>
    public static class WindsorControllerExtensions
    {
        /// <summary>
        /// Registers the specified HTTP controllers.
        /// </summary>
        /// <param name="container">The container.</param>
        /// <param name="controllerTypes">The controller types.</param>
        /// <returns>A container.</returns>
        public static IWindsorContainer RegisterControllers(this IWindsorContainer container, params Type[] controllerTypes)
        {
            if (container == null) throw new ArgumentNullException("container");
            if (controllerTypes == null) throw new ArgumentNullException("controllerTypes");

            foreach (var type in controllerTypes.Where(ControllerExtensions.IsHttpController))
            {
                container.Register(
                    Component
                        .For(type).Named(type.FullName.ToLower())
                        .LifeStyle.Is(LifestyleType.Transient));
            }

            return container;
        }

        /// <summary>
        /// Registers the HTTP controllers that are found in the specified assemblies.
        /// </summary>
        /// <param name="container">The container.</param>
        /// <param name="assemblies">The assemblies.</param>
        /// <returns>A container.</returns>
        public static IWindsorContainer RegisterControllers(this IWindsorContainer container, params Assembly[] assemblies)
        {
            if (container == null) throw new ArgumentNullException("container");
            if (assemblies == null) throw new ArgumentNullException("assemblies");

            foreach (var assembly in assemblies)
            {
                RegisterControllers(container, assembly.GetExportedTypes());
            }

            return container;
        }
    }
}
