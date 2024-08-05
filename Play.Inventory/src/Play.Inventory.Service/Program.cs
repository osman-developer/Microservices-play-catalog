using System.Net.Http;
using MassTransit;
using Play.Common.MassTransit;
using Play.Common.MongoDb;
using Play.Inventory.Service.Clients;
using Play.Inventory.Service.Entities;
using Polly;
using Polly.Timeout;

var builder = WebApplication.CreateBuilder (args);

builder.Services.AddMongo ()
    .AddMongoRepository<InventoryItem> ("inventoryitems")
    .AddMongoRepository<CatalogItem> ("catalogitems")
    .AddMassTransitWithRabbitMQ ();

Random jitterer = new Random ();

var service = builder.Services;
//so we use the polly method in order to async wait for reply, then we add the exponential trying or retrires 
//if the catalog micro fails in order to keep trying after polly seconds (e.g:1)
//so if the timeout exception is thrown, we do the retry we set e.g:5 times to do retry, with exponentioal wait between retry (2,4,8..)
//it is important to define the transienthttppolicy for retry before the polly waiting
//it is important to define the circuit breaker beffore retry before the polly waiting and after the retry policy

builder.Services.AddHttpClient<CatalogClient> (client => {
        client.BaseAddress = new Uri ("http://localhost:5000/api");
    })
    .AddTransientHttpErrorPolicy (builder => builder.Or<TimeoutRejectedException> ().WaitAndRetryAsync (5,
        retryAttempt => TimeSpan.FromSeconds (Math.Pow (2, retryAttempt)) +
        TimeSpan.FromMilliseconds (jitterer.Next (0, 1000)),
        //we add the jitter so to avoid overwhelming the server with calls having exactly same seconds, so each call might differ in ms
        //the onretry section is not so important, we can remove it, we can do loggin iside it
        onRetry: (outcome, timespan, retryAttempt) => {
            Console.WriteLine ($"Delaying for {timespan.TotalSeconds} seconds , then making retry {retryAttempt}");
            //not an ideal way to log
            // var serviceProvider = service.BuildServiceProvider ();
            // serviceProvider.GetService<ILogger<CatalogClient>> ()?.LogWarning ($"Delaying for {timespan.TotalSeconds} seconds , then making retry {retryAttempt}");
        }
    ))
    .AddTransientHttpErrorPolicy (builder => builder.Or<TimeoutRejectedException> ().CircuitBreakerAsync (
        //numb of allowed fail times before openning the circuit
        3,
        TimeSpan.FromSeconds (15),
        onBreak: (outcome, timespan) => {
            var serviceProvider = service.BuildServiceProvider ();
            serviceProvider.GetService<ILogger<CatalogClient>> ()?.LogWarning ($"Openning the circuit for  {timespan.TotalSeconds} seconds..");
        },
        onReset: () => {
            var serviceProvider = service.BuildServiceProvider ();
            serviceProvider.GetService<ILogger<CatalogClient>> ()?.LogWarning ($"closing the circuit");

        }
    ))
    .AddPolicyHandler (Policy.TimeoutAsync<HttpResponseMessage> (1));

builder.Services.AddControllers (options => {
    options.SuppressAsyncSuffixInActionNames = false;
});
// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer ();
builder.Services.AddSwaggerGen ();

var app = builder.Build ();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment ()) {
    app.UseSwagger ();
    app.UseSwaggerUI ();
}

app.UseHttpsRedirection ();

app.UseAuthorization ();

app.MapControllers ();

app.Run ();