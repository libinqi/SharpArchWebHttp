namespace SharpArchWebHttp
{
    using System;
    using System.Net.Http;
    using System.Web.Http.Controllers;
    using System.Web.Http.Dispatcher;

    using Castle.Windsor;

    /// <summary>
    /// Activates HTTP controllers using Castle Windsor.
    /// </summary>
    public class WindsorHttpControllerActivator : IHttpControllerActivator
    {
        private readonly IWindsorContainer container;

        /// <summary>
        /// Initializes a new instance of the <see cref="WindsorHttpControllerActivator" /> class.
        /// </summary>
        /// <param name="container">The container.</param>
        public WindsorHttpControllerActivator(IWindsorContainer container)
        {
            if (container == null) throw new ArgumentNullException("container");

            this.container = container;
        }

        /// <summary>
        /// Creates the specified request.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <param name="controllerDescriptor">The controller descriptor.</param>
        /// <param name="controllerType">Type of the controller.</param>
        /// <returns>A controller.</returns>
        public IHttpController Create(HttpRequestMessage request, HttpControllerDescriptor controllerDescriptor, Type controllerType)
        {
            if (controllerDescriptor == null) throw new ArgumentNullException("controllerDescriptor");

            var name = controllerDescriptor.ControllerType.FullName.ToLowerInvariant();

            return this.container.Kernel.Resolve<IHttpController>(name);
        }
    }
}
