using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using Dapper;
using Frodo.Common;
using Frodo.Infrastructure.Json;
using Frodo.Persistence.Mapping;

namespace Frodo.Persistence
{
    public sealed class SqliteStorageAdapter : IStorageAdapter
    {
        private readonly IJsonSerializer _jsonSerializer;

        public SqliteStorageAdapter(IJsonSerializer jsonSerializer)
        {
            _jsonSerializer = jsonSerializer;
        }

        public T Fetch<T>(Guid id, IDbConnection connection, IDbTransaction transaction) where T : Dao
        {
            return Fetch(id, typeof(T), connection, transaction) as T;
        }

        public Dao Fetch(Guid id, Type type, IDbConnection connection, IDbTransaction transaction)
        {
            // todo [kk]: check that type is derived from PersistenceModel

            var tableName = GetTableName(type);
            CreateTableIfNotExists(tableName, connection, transaction);

            var data = connection.ExecuteScalar<string>($@"SELECT [Data] FROM [{tableName}] WHERE Id = @id", new { id = Id2Str(id) }, transaction);
            if (data == null)
            {
                return null;
            }

            var model = _jsonSerializer.Deserialize(data, type) as Dao;
            return model;
        }

        public ICollection<T> Fetch<T>(Func<T, bool> predicate, IDbConnection connection, IDbTransaction transaction) where T : Dao
        {
            var result = FetchAll<T>(connection, transaction).Where(predicate).ToList();
            return result;
        }

        public ICollection<T> FetchAll<T>(IDbConnection connection, IDbTransaction transaction) where T : Dao
        {
            return FetchAll(typeof(T), connection, transaction).OfType<T>().ToList();
        }

        public ICollection<Dao> FetchAll(Type type, IDbConnection connection, IDbTransaction transaction)
        {
            var tableName = GetTableName(type);
            CreateTableIfNotExists(tableName, connection, transaction);
            var data = connection.Query<string>($@"SELECT [Data] FROM [{tableName}]", null, transaction);
            var models = data.Select(x => _jsonSerializer.Deserialize(x, type) as Dao).ToList();
            return models;
        }

        public void Transaction(Action<ITransactionWrapper> action)
        {
            using (var connection = CreateConnection())
            {
                connection.Open();
                using (var transaction = connection.BeginTransaction(IsolationLevel.ReadCommitted))
                {
                    var transactionWrapper = new TransactionWrapper(connection, transaction, this);
                    action(transactionWrapper);
                    transaction.Commit();
                }
            }
        }

        public TResult Transaction<TResult>(Func<ITransactionWrapper, TResult> action)
        {
            using (var connection = CreateConnection())
            {
                connection.Open();
                using (var transaction = connection.BeginTransaction(IsolationLevel.ReadCommitted))
                {
                    var transactionWrapper = new TransactionWrapper(connection, transaction, this);
                    var result = action(transactionWrapper);
                    transaction.Commit();
                    
                    return result;
                }
            }
        }

        public void Save<T>(T model, IDbConnection connection, IDbTransaction transaction) where T : Dao
        {
            Save(model, typeof(T), connection, transaction);
        }

        public void Save(Dao model, Type type, IDbConnection connection, IDbTransaction transaction)
        {
            // todo [kk]: check that type is derived from PersistenceModel

            var tableName = GetTableName(type);
            CreateTableIfNotExists(tableName, connection, transaction);

            var data = _jsonSerializer.Serialize(model);
            SaveData(tableName, model.Id, data, connection, transaction);
        }

        public void Delete<T>(Guid id, IDbConnection connection, IDbTransaction transaction) where T : Dao
        {
            Delete(id, typeof(T), connection, transaction);
        }

        public void Delete(Guid id, Type type, IDbConnection connection, IDbTransaction transaction)
        {
            var tableName = GetTableName(type);
            CreateTableIfNotExists(tableName, connection, transaction);
            connection.Execute($@"DELETE FROM [{tableName}] WHERE Id = @id", new { id = Id2Str(id) }, transaction);
        }

        public void DeleteAll(Type type, IDbConnection connection, IDbTransaction transaction)
        {
            var tableName = GetTableName(type);
            CreateTableIfNotExists(tableName, connection, transaction);
            connection.Execute($@"DELETE FROM [{tableName}]", null, transaction);
        }

        private static string GetTableName(Type type)
        {
            var tableName = type.Name;
            return tableName;
        }

        private static void SaveData(string tableName, Guid id, string data, IDbConnection connection, IDbTransaction transaction)
        {
            var idStr = Id2Str(id);
            connection.Execute(
                $@"
        UPDATE [{tableName}]
            SET
        Data = @data
        WHERE Id = @id",
                new { id = idStr, data = data },
                transaction
            );

            connection.Execute(
                $@"
        INSERT OR IGNORE INTO [{tableName}]
            (Id, Data)
        VALUES
            (@id, @data)",
                new { id = idStr, data = data },
                transaction
            );
        }

        private static string Id2Str(Guid id)
        {
            return id.ToString("D");
        }

        private static void CreateTableIfNotExists(string tableName, IDbConnection connection, IDbTransaction transaction)
        {
            connection.Execute(
                $@"
        CREATE TABLE IF NOT EXISTS [{tableName}] (
            [Id] NVARCHAR(36) NOT NULL PRIMARY KEY,
            [Data] NVARCHAR NOT NULL
        )",
                null,
                transaction);
        }

        private static SQLiteConnection CreateConnection()
        {
            var dbFilePath = GetDatabaseAbsolutePath();
            if (!File.Exists(dbFilePath))
            {
                var directoryName = Path.GetDirectoryName(dbFilePath);
                if (string.IsNullOrEmpty(directoryName) == false) Directory.CreateDirectory(directoryName);
                SQLiteConnection.CreateFile(dbFilePath);
            }

            var connection = new SQLiteConnection
            {
                ConnectionString =
                    new SQLiteConnectionStringBuilder { DataSource = dbFilePath, ForeignKeys = true, }
                        .ConnectionString
            };

            return connection;
        }

        private static string GetDatabaseAbsolutePath()
        {
            return PathUtils.ToAbsolutePath("~\\data\\frodo.db3");
        }
    }
}