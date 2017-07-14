using System;
using Microsoft.Extensions.Configuration;
using Platform.Utils.Agents;
using Platform.Utils.Agents.Interfaces;
using Platform.Utils.Ioc;

namespace Platform.Service.NetCore.IoC
{
    using SimpleInjector;
    using Utils.Events.Manager;
    using Utils.Redis;
    using Utils.Workflow.PetriNets.Impl;

    public class AgentModule : IPlatformIocModule
    {
        public void Register(SimpleInjector.Container container, IConfigurationRoot config)
        {
            container.RegisterSingleton<IAgentStorage, AgentStorage>();
            container.RegisterSingleton<AgentBus>();
            container.RegisterSingleton<WorkflowManager>();


            container.RegisterSingleton(new RedisPubSub(
       new RedisConnection(config.GetSection("ConnectionStrings")["RedisServer"])));

            container.RegisterSingleton<ITemplateStorage, TemplateStorage>();
            container.RegisterSingleton<IAgentManager, AgentManager>();
        }
    }
}
