using System;
using System.Collections.Generic;

namespace Frodo.Core.Repositories
{
    public interface IRepository<TEntity> where TEntity : Entity
    {
        TEntity Get(Guid id);

        IReadOnlyCollection<TEntity> GetAll();

        void Delete(Guid id);

        void DeleteAll();

        void Save(TEntity aggregate);

        TEntity FindSingle(Func<TEntity, bool> predicate);

        ICollection<TEntity> FindAll(Func<TEntity, bool> predicate);

        TEntity Clone(TEntity aggregate);
    }
}