using PipServices.Commons.Data;
using PipServices.Oss.MongoDb;

using System;
using System.Threading.Tasks;

using MongoDB.Bson;
using MongoDB.Driver;

namespace PipServices.Azure.Persistence
{
    public class CosmosMongoDbPartitionPersistence<T, K> : IdentifiableMongoDbPersistence<T, K>
        where T : IIdentifiable<K>
        where K : class
    {
        private string _partitionKey;

        public CosmosMongoDbPartitionPersistence(string collectionName, string partitionKey)
            : base(collectionName)
        {
            _partitionKey = partitionKey;
        }

        public override async Task OpenAsync(string correlationId)
        {
            await base.OpenAsync(correlationId);

            if (!await CollectionExistsAsync())
            {
                await CreatePartitionCollectionAsync(correlationId);
            }
            else
            {
                _logger.Warn(correlationId, $"OpenAsync: Skip to create partition collection.");
            }
        }

        public new async Task<T> DeleteByIdAsync(string correlationId, K id)
        {
            var builder = Builders<T>.Filter;
            var filter = builder.Empty;
            var key = GetPartitionKey(id);

            filter &= builder.Eq(x => x.Id, id);
            filter &= builder.Eq(_partitionKey, key);

            var options = new FindOneAndDeleteOptions<T>();
            var result = await _collection.FindOneAndDeleteAsync(filter, options);

            _logger.Trace(correlationId, $"Deleted from {_collectionName} with id = {id} and {_partitionKey} = {key}");

            return result;
        }

        public new async Task<T> ModifyByIdAsync(string correlationId, K id, UpdateDefinition<T> updateDefinition)
        {
            if (id == null || updateDefinition == null)
            {
                return default(T);
            }

            var builder = Builders<T>.Filter;
            var filter = builder.Empty;
            var key = GetPartitionKey(id);

            filter &= builder.Eq(x => x.Id, id);
            filter &= builder.Eq(_partitionKey, key);

            var result = await ModifyAsync(correlationId, filter, updateDefinition);

            _logger.Trace(correlationId, $"Modified in {_collectionName} with id = {id} and {_partitionKey} = {key}");

            return result;
        }

        public new async Task<T> UpdateAsync(string correlationId, T item)
        {
            var identifiable = item as IIdentifiable<K>;
            if (identifiable == null || item.Id == null)
            {
                return default(T);
            }

            var builder = Builders<T>.Filter;
            var filter = builder.Empty;
            var key = GetPartitionKey(identifiable.Id);

            filter &= builder.Eq(x => x.Id, identifiable.Id);
            filter &= builder.Eq(_partitionKey, key);

            var options = new FindOneAndReplaceOptions<T>
            {
                ReturnDocument = ReturnDocument.After,
                IsUpsert = false
            };
            var result = await _collection.FindOneAndReplaceAsync(filter, item, options);

            _logger.Trace(correlationId, $"Updated in {_collectionName} with id = {identifiable.Id} and {_partitionKey} = {key}");

            return result;
        }

        public async Task<T> GetOneByFilterAsync(string correlationId, FilterDefinition<T> filter)
        {
            var result = await _collection.Find(filter).FirstOrDefaultAsync();

            if (result == null)
            {
                _logger.Trace(correlationId, $"GetOneByFilter: nothing found in {_collectionName} with filter.");
                return default(T);
            }

            _logger.Trace(correlationId, $"GetOneByFilter: retrieved first element from {_collectionName} by filter.");

            return result;
        }

        protected virtual async Task CreatePartitionCollectionAsync(string correlationId)
        {
            try
            {
                // Specific CosmosDB command that creates partition collection (it raises exception for MongoDB)
                await _database.RunCommandAsync(new BsonDocumentCommand<BsonDocument>(new BsonDocument
                {
                    {"shardCollection", $"{_database.DatabaseNamespace.DatabaseName}.{_collectionName}"},
                    {"key", new BsonDocument {{ _partitionKey, "hashed"}}}
                }));
            }
            catch (Exception exception)
            {
                _logger.Error(correlationId, exception, $"CreatePartitionCollectionAsync: Failed to create partition collection.");
            }

            _collection = _database.GetCollection<T>(_collectionName);
        }

        protected virtual string GetPartitionKey(K id)
        {
            return string.Empty;
        }

        protected virtual async Task<bool> CollectionExistsAsync()
        {
            var collections = await _database.ListCollectionsAsync(new ListCollectionsOptions
            {
                Filter = new BsonDocument("name", _collectionName)
            });

            return await collections.AnyAsync();
        }

        protected virtual async Task<U> ExecuteWithRetriesAsync<U>(string correlationId, Func<Task<U>> invokeFunc, int maxRetries = 3)
        {
            for (var retry = 1; retry <= maxRetries; retry++)
            {
                try
                {
                    return await invokeFunc();
                }
                catch (MongoConnectionException mongoConnectionException)
                {
                    _logger.Error(correlationId, $"MongoConnectionException happened on {retry}/{maxRetries} attempt.");

                    if (retry >= maxRetries)
                    {
                        throw mongoConnectionException;
                    }

                    await Task.Delay(1000);
                }
                catch (Exception exception)
                {
                    throw exception;
                }
            }

            return await Task.FromResult(default(U));
        }
    }
}
