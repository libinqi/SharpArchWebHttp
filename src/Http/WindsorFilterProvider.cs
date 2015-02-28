namespace SharpArchWebHttp
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Web.Http;
    using System.Web.Http.Controllers;
    using System.Web.Http.Filters;

    using Castle.Windsor;

    /// <summary>
    /// A Castle Windsor filter provider that supports property injection.
    /// </summary>
    public class WindsorFilterProvider : IFilterProvider
    {
        private readonly IWindsorContainer container;

        /// <summary>
        /// Initializes a new instance of the <see cref="WindsorFilterProvider" /> class.
        /// </summary>
        /// <param name="container">The container.</param>
        public WindsorFilterProvider(IWindsorContainer container)
        {
            if (container == null) throw new ArgumentNullException("container");

            this.container = container;
        }

        /// <summary>
        /// Returns an enumeration of filters.
        /// </summary>
        /// <param name="configuration">The HTTP configuration.</param>
        /// <param name="actionDescriptor">The action descriptor.</param>
        /// <returns>An enumeration of filters.</returns>
        public IEnumerable<FilterInfo> GetFilters(HttpConfiguration configuration, HttpActionDescriptor actionDescriptor)
        {
            if (configuration == null) throw new ArgumentNullException("configuration");
            if (actionDescriptor == null) throw new ArgumentNullException("actionDescriptor");

            var globalFilters =
                configuration.Filters.Select(n => new FilterInfo(n.Instance, FilterScope.Global));

            var controllerFilters = actionDescriptor
                .ControllerDescriptor
                .GetFilters()
                .Select(n => new FilterInfo(n, FilterScope.Controller));

            var actionFilters = actionDescriptor
                .GetFilters()
                .Select(n => new FilterInfo(n, FilterScope.Action));

            var filters = globalFilters.Concat(controllerFilters.Concat(actionFilters));
            this.container.Kernel.ResolveProperties(filters.Select(n => n.Instance));

            return filters;
        }
    }
}
