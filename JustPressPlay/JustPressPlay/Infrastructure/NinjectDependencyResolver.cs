using JustPressPlay.Models.Repositories;
using Ninject;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace JustPressPlay.Infrastructure
{
    public class NinjectDependencyResolver : IDependencyResolver
    {
        private IKernel ninjectKernel;

        public NinjectDependencyResolver()
        {
            ninjectKernel = new StandardKernel();
            AddBindings();
        }

        public object GetService(Type serviceType)
        {
            return ninjectKernel.TryGet(serviceType);
        }

        public IEnumerable<object> GetServices(Type serviceType)
        {
            return ninjectKernel.GetAll(serviceType);
        }

        private void AddBindings()
        {
            ninjectKernel.Bind<IUnitOfWork>().To<UnitOfWork>(); // Generally probably not needed, but it may come up
        }
    }
}