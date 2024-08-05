using System.Linq.Expressions;
using MongoDB.Driver;
using Play.Common;

namespace Play.Common.MongoDb {
  public class MongoRepository<T> : IRepository<T> where T : IEntity {

    //represents a collection in a MongoDB database
    private readonly IMongoCollection<T> dbCollection;
    //provides a type-safe API for building up both simple and complex MongoDB queries. 
    private readonly FilterDefinitionBuilder<T> filterBuilder = Builders<T>.Filter;

    //A collection is a grouping of MongoDB documents. Documents within a collection can have different fields.
    // A collection is the equivalent of a table in a relational database, so we pass the collection name in ctor e.g from program.cs
    public MongoRepository (IMongoDatabase database, string collectionName) {
      //get the instance of the collection
      dbCollection = database.GetCollection<T> (collectionName);
    }

    public async Task<IReadOnlyCollection<T>> GetAllAsync () {
      return await dbCollection.Find (filterBuilder.Empty).ToListAsync ();
    }

    public async Task<IReadOnlyCollection<T>> GetAllAsync (Expression<Func<T, bool>> filter) {
      return await dbCollection.Find (filter).ToListAsync ();
    }
    public async Task<T> GetAsync (Guid id) {
      FilterDefinition<T> filter = filterBuilder.Eq (entity => entity.Id, id);
      return await dbCollection.Find (filter).FirstOrDefaultAsync ();
    }
    public async Task<T> GetAsync (Expression<Func<T, bool>> filter) {
      return await dbCollection.Find (filter).FirstOrDefaultAsync ();
    }

    public async Task CreateAsync (T entity) {
      if (entity == null) {
        throw new ArgumentNullException (nameof (entity));
      }
      await dbCollection.InsertOneAsync (entity);
    }

    public async Task UpdateAsync (T entity) {
      if (entity == null) {
        throw new ArgumentNullException (nameof (entity));
      }
      FilterDefinition<T> filter = filterBuilder.Eq (existingEntity => existingEntity.Id, entity.Id);
      await dbCollection.ReplaceOneAsync (filter, entity);
    }

    public async Task RemoveAsync (Guid id) {
      FilterDefinition<T> filter = filterBuilder.Eq (existingEntity => existingEntity.Id, id);
      await dbCollection.DeleteOneAsync (filter);
    }

  }
}