using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Driver;
using Play.Common.Settings;

namespace Play.Common.MongoDb {
  public static class Extensions {
    public static IServiceCollection AddMongo (this IServiceCollection services) {
      // Add services to the container.
      //this bson serializer is used to whenever we want to store a Guid and datetimeoffset, it will store it as a string
      //so it shows a user friendly string 
      BsonSerializer.RegisterSerializer (new GuidSerializer (BsonType.String));
      BsonSerializer.RegisterSerializer (new DateTimeOffsetSerializer (BsonType.String));

      //we are defining the mongodb and to be injected later on in the mongoRepository ctor
      services.AddSingleton (serviceProvider => {

        var configuration = serviceProvider.GetService<IConfiguration> ();
        var serviceSettings = configuration.GetSection (nameof (ServiceSettings)).Get<ServiceSettings> ();
        var mongoDbSettings = configuration.GetSection (nameof (MongoDbSettings)).Get<MongoDbSettings> ();
        //used to connect to db
        var mongoDbClient = new MongoClient (mongoDbSettings?.ConnectionString);
        //the database name where the collections reside in
        return mongoDbClient.GetDatabase (serviceSettings?.ServiceName);

      });
      return services;
    }
    public static IServiceCollection AddMongoRepository<T> (this IServiceCollection services, string collectionName) where T : IEntity {
      services.AddSingleton<IRepository<T>> (serviceProvider => {
        var database = serviceProvider.GetService<IMongoDatabase> ();
        return new MongoRepository<T> (database, collectionName);
      });
      return services;
    }
  }
}