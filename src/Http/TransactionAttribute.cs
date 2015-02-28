namespace SharpArchWebHttp
{
    using System.Data;
    using System.Web.Http.Controllers;
    using System.Web.Http.Filters;

    using global::SharpArch.NHibernate;

    /// <summary>
    /// An attribute that implies a transaction.
    /// </summary>
    public class TransactionAttribute : ActionFilterAttribute
    {
        /// <summary>
        /// Optionally holds the factory key to be used when beginning/committing a transaction.
        /// </summary>
        private readonly string factoryKey;

        /// <summary>
        /// When used, assumes the <see cref="factoryKey" /> to be NHibernateSession.DefaultFactoryKey.
        /// </summary>
        public TransactionAttribute()
        {
            IsolationLevel = IsolationLevel.Unspecified;
        }

        /// <summary>
        /// Overrides the default <see cref="factoryKey" /> with a specific factory key.
        /// </summary>
        /// <param name="factoryKey">The factory key.</param>
        public TransactionAttribute(string factoryKey)
            : this()
        {
            this.factoryKey = factoryKey;
        }

        /// <summary>
        /// Gets or sets a value indicating whether to rollback on model state error.
        /// </summary>
        public bool RollbackOnModelStateError { get; set; }

        /// <summary>
        /// Gets or sets the isolation level.
        /// </summary>
        public IsolationLevel IsolationLevel { get; set; }

        /// <summary>
        /// Occurs before the action method is invoked.
        /// </summary>
        /// <param name="actionContext">The action context.</param>
        public override void OnActionExecuting(HttpActionContext actionContext)
        {
            base.OnActionExecuting(actionContext);

            var session = NHibernateSession.CurrentFor(this.GetEffectiveFactoryKey());

            if (this.IsolationLevel != IsolationLevel.Unspecified)
            {
                session.BeginTransaction(this.IsolationLevel);
            }
            else
            {
                session.BeginTransaction();
            }
        }

        /// <summary>
        /// Occurs after the action method is invoked.
        /// </summary>
        /// <param name="filterContext">The filter context.</param>
        public override void OnActionExecuted(HttpActionExecutedContext filterContext)
        {
            base.OnActionExecuted(filterContext);

            var effectiveFactoryKey = this.GetEffectiveFactoryKey();
            var currentTransaction = NHibernateSession.CurrentFor(effectiveFactoryKey).Transaction;

            try
            {
                if (currentTransaction.IsActive)
                {
                    if (filterContext.Exception != null || this.ShouldRollback(filterContext))
                    {
                        currentTransaction.Rollback();
                    }
                    else
                    {
                        currentTransaction.Commit();
                    }
                }
            }
            finally
            {
                currentTransaction.Dispose();
            }
        }

        /// <summary>
        /// Gets the effective factory key.
        /// </summary>
        /// <returns>A string.</returns>
        private string GetEffectiveFactoryKey()
        {
            return string.IsNullOrEmpty(factoryKey) ? SessionFactoryKeyHelper.GetKey() : factoryKey;
        }

        /// <summary>
        /// Returns a value indicating whether to rollback or not, based on the specified context.
        /// </summary>
        /// <param name="filterContext">The filter context.</param>
        /// <returns><c>true</c> to rollback, <c>false</c> otherwise.</returns>
        private bool ShouldRollback(HttpActionExecutedContext filterContext)
        {
            return this.RollbackOnModelStateError && !filterContext.ActionContext.ModelState.IsValid;
        }
    }
}
