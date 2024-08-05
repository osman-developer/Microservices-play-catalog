using System.Reflection;
using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Play.Common.Settings;

namespace Play.Common.MassTransit {
  public static class Extensions {
    public static IServiceCollection AddMassTransitWithRabbitMQ (this IServiceCollection services) {
      services.AddMassTransit (configure => {
        //consumers are the ones that going to consume messages from rabbitmq queues
        configure.AddConsumers (Assembly.GetEntryAssembly ());

        configure.UsingRabbitMq ((context, configurator) => {
          var configuration = context.GetService<IConfiguration> ();
          var serviceSettings = configuration.GetSection (nameof (ServiceSettings)).Get<ServiceSettings> ();
          var rabbitMQSettings = configuration.GetSection (nameof (RabbitMQSettings)).Get<RabbitMQSettings> ();
          configurator.Host (rabbitMQSettings.Host);
          //to configure or to tell how the queues are created in rabbitmq
          configurator.ConfigureEndpoints (context, new KebabCaseEndpointNameFormatter (serviceSettings.ServiceName, false));
          //so when  a message is not being able to be consumed by a consumer, we try 3 times
          configurator.UseMessageRetry (retryConfigurator => {
            retryConfigurator.Interval (3, TimeSpan.FromSeconds (4));
          });
        });
      });
      return services;
    }
  }
}