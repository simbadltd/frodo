using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using AutoMapper;
using Frodo.Core;
using Frodo.Core.Repositories;
using Frodo.Infrastructure.Ioc;
using Frodo.Persistence.Mapping;

namespace Frodo.Persistence
{
    public sealed class Repository<TEntity> : IRepository<TEntity> where TEntity : Entity
    {
        private readonly object _syncRoot = new object();

        private readonly Dictionary<Guid, TEntity> _cache = new Dictionary<Guid, TEntity>();

        private readonly IStorageAdapter _storageAdapter;

        private readonly Func<IMapper> _mapperFactory;

        private readonly IUnitOfWork _unitOfWork;

        private readonly IIocContainer _iocContainer;

        private readonly Lazy<Type> _aggregateRootProjectionType =
            new Lazy<Type>(ResolveAggregateRootProjectionType, true);

        private Type _persistenceModelType;

        private bool _allFetched;

        private Type PersistenceModelType
        {
            get
            {
                if (_persistenceModelType == null)
                {
                    lock (_syncRoot)
                    {
                        if (_persistenceModelType == null)
                        {
                            _persistenceModelType = CreatePersistenceModel().GetType();
                            return _persistenceModelType;
                        }
                    }
                }

                return _persistenceModelType;
            }
        }

        public Repository(IStorageAdapter storageAdapter, Func<IMapper> mapperFactory, IUnitOfWork unitOfWork,
            IIocContainer iocContainer)
        {
            _storageAdapter = storageAdapter;
            _mapperFactory = mapperFactory;
            _unitOfWork = unitOfWork;
            _iocContainer = iocContainer;
        }

        public TEntity Get(Guid id)
        {
            lock (_syncRoot)
            {
                return GetOrAdd(
                    id,
                    guid =>
                    {
                        var model = _storageAdapter.Transaction(tw => tw.Fetch(guid, PersistenceModelType));
                        return CreateMapper().Map(model, PersistenceModelType, typeof(TEntity)) as TEntity;
                    });
            }
        }

        public IReadOnlyCollection<TEntity> GetAll()
        {
            lock (_syncRoot)
            {
                if (_allFetched)
                {
                    return _cache.Values.ToList();
                }

                var mapper = CreateMapper();
                var all = _storageAdapter.Transaction(tw => tw.FetchAll(PersistenceModelType));
                var result = all.Select(x => mapper.Map(x, PersistenceModelType, typeof(TEntity)) as TEntity).ToList();
                RefreshCache(result);
                _allFetched = true;

                return result;
            }
        }

        public void Delete(Guid id)
        {
            lock (_syncRoot)
            {
                _unitOfWork.Delete(id, PersistenceModelType);
                InvalidateCache(id);
            }
        }

        public void DeleteAll()
        {
            lock (_syncRoot)
            {
                _unitOfWork.DeleteAll(PersistenceModelType);
                InvalidateCache();
            }
        }

        public TEntity FindSingle(Func<TEntity, bool> predicate)
        {
            lock (_syncRoot)
            {
                var aggregate = GetAll().SingleOrDefault(predicate);
                return aggregate;
            }
        }

        public ICollection<TEntity> FindAll(Func<TEntity, bool> predicate)
        {
            lock (_syncRoot)
            {
                var aggregates = GetAll().Where(predicate).ToList();
                return aggregates;
            }
        }

        public TEntity Clone(TEntity aggregate)
        {
            var model = CreatePersistenceModel();
            var modelType = model.GetType();
            var mapper = CreateMapper();
            mapper.Map(aggregate, model, typeof(TEntity), modelType);

            var clone = mapper.Map(model, modelType, typeof(TEntity)) as TEntity;
            Debug.Assert(clone != null);
            clone.Id = GlobalIdGenerator.NewId();

            return clone;
        }

        public void Save(TEntity aggregate)
        {
            // [kk] Здесь может быть проверка консистентности сущности

            lock (_syncRoot)
            {
                var events = aggregate.ExtractEvents();
                _unitOfWork.TrackEvents(events);

                var model = CreatePersistenceModel();
                var destinationType = model.GetType();
                CreateMapper().Map(aggregate, model, typeof(TEntity), destinationType);

                _unitOfWork.Save(model, destinationType);

                RefreshCache(aggregate);
            }
        }

        private IMapper CreateMapper()
        {
            return _mapperFactory();
        }

        private Dao CreatePersistenceModel()
        {
            return _iocContainer.Resolve(_aggregateRootProjectionType.Value) as Dao;
        }

        private static Type ResolveAggregateRootProjectionType()
        {
            return typeof(IEntityProjection<>).MakeGenericType(typeof(TEntity));
        }

        private TEntity GetOrAdd(Guid id, Func<Guid, TEntity> factory)
        {
            if (_cache.ContainsKey(id))
            {
                return _cache[id];
            }

            var result = factory(id);
            RefreshCache(result);

            return result;
        }

        private void RefreshCache(IEnumerable<TEntity> aggregates)
        {
            foreach (var aggregate in aggregates)
            {
                RefreshCache(aggregate);
            }
        }

        private void RefreshCache(TEntity aggregate)
        {
            if (aggregate == null) return;

            _cache[aggregate.Id] = aggregate;
        }

        private void InvalidateCache(Guid id)
        {
            if (_cache.ContainsKey(id) == false)
            {
                return;
            }

            _cache.Remove(id);
            _allFetched = false;
        }

        private void InvalidateCache()
        {
            _cache.Clear();
            _allFetched = false;
        }
    }
}