using System.Web.Mvc;
using EveAI;
using EveAI.Live;
using EveTools.Domain;
using EveTools.Infrastructure;
using Microsoft.Practices.Unity;
using MongoDB.Driver;
using Unity.Mvc5;

namespace EveTools.Web
{
    public static class UnityConfig
    {
        public static void RegisterComponents()
        {
			var container = new UnityContainer();

            container.RegisterType<MongoClient>(new ContainerControlledLifetimeManager(), new InjectionConstructor("mongodb://localhost"));
            container.RegisterType<MongoServer>(new ContainerControlledLifetimeManager(), new InjectionFactory(c => c.Resolve<MongoClient>().GetServer()));
            container.RegisterType<MongoDatabase>(new ContainerControlledLifetimeManager(), new InjectionFactory(c => c.Resolve<MongoServer>().GetDatabase("eveTools")));

            container.RegisterType<DataCore>(new ContainerControlledLifetimeManager(), new InjectionFactory(i => new EveApi(true).EveApiCore));

            container.RegisterType<IApiKeyRepository, ApiKeyRepository>(new HierarchicalLifetimeManager());
            container.RegisterType<IPricingRepository, PricingRepository>(new HierarchicalLifetimeManager());
            container.RegisterType<IPricingService, PricingService>(new HierarchicalLifetimeManager());
            // register all your components with the container here
            // it is NOT necessary to register your controllers
            
            // e.g. container.RegisterType<ITestService, TestService>();
            
            DependencyResolver.SetResolver(new UnityDependencyResolver(container));
        }
    }
}