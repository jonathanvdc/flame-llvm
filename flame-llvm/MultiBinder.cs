using System;
using System.Collections.Generic;
using System.Linq;
using Flame.Compiler.Visitors;

namespace Flame.LLVM
{
    /// <summary>
    /// A binder that delegates queries to zero or more child binders.
    /// </summary>
    public sealed class MultiBinder : IBinder
    {
        public MultiBinder(IEnvironment Environment)
        {
            this.Environment = Environment;
            this.subBinders = new List<IBinder>();
            this.registeredAsms = new HashSet<IAssembly>();
        }

        private List<IBinder> subBinders;
        private HashSet<IAssembly> registeredAsms;

        /// <inheritdoc/>
        public IEnvironment Environment { get; private set; }

        /// <summary>
        /// Adds the given binder to this multi-binder.
        /// </summary>
        /// <param name="Binder">The binder to add.</param>
        public void AddBinder(IBinder Binder)
        {
            subBinders.Add(Binder);
        }

        /// <summary>
        /// Adds a binder for the given assembly to this multi-binder,
        /// if it hasn't been added yet.
        /// </summary>
        /// <param name="Assembly">The assembly to create a binder from.</param>
        public bool AddAssembly(IAssembly Assembly)
        {
            if (registeredAsms.Add(Assembly))
            {
                AddBinder(Assembly.CreateBinder());
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <inheritdoc/>
        public IType BindType(QualifiedName Name)
        {
            foreach (var item in subBinders)
            {
                var result = item.BindType(Name);
                if (result != null)
                    return result;
            }
            return null;
        }

        /// <inheritdoc/>
        public IEnumerable<IType> GetTypes()
        {
            return subBinders.SelectMany<IBinder, IType>(GetTypes);
        }

        private static IEnumerable<IType> GetTypes(IBinder Binder)
        {
            return Binder.GetTypes();
        }
    }
}