using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using Frodo.Common;
using Frodo.Core.Events;

namespace Frodo.Core
{
    public abstract class Entity : IDomainEventsSource
    {
        private readonly List<IDomainEvent> _events;

        public Guid Id { get; set; }

        protected Entity()
        {
            Id = GlobalIdGenerator.NewId();
            _events = new List<IDomainEvent>();
        }

        public void Publish(IDomainEvent @event)
        {
            _events.Add(@event);
        }

        public HashSet<Guid> CheckConsistency(HashSet<Guid> checkedObjects = null)
        {
            var result = checkedObjects ?? new HashSet<Guid>();

            CheckDefaultConsistency(result);
            CheckConsistencySpecific();

            var validationResults = new List<ValidationResult>();
            var isValid = Validator.TryValidateObject(this, new ValidationContext(this), validationResults);

            if (isValid == false)
            {
                throw new EntityConsistencyException(this,
                    string.Join("; ", validationResults.Select(x => "[" + string.Join(", ", x.MemberNames) + "]: " + x.ErrorMessage)));
            }

            return result;
        }

        private void CheckDefaultConsistency(HashSet<Guid> checkedObjects)
        {
            if (checkedObjects.Contains(Id) && Id != Guid.Empty)
            {
                return;
            }

            var properties =
                GetType()
                    .GetProperties(BindingFlags.Instance | BindingFlags.Public)
                    .Where(x => x.CanRead && x.CanWrite)
                    .ToList();

            var subEntities = new List<Entity>();

            foreach (var property in properties)
            {
                if (typeof(Entity).IsAssignableFrom(property.PropertyType))
                {
                    var subEntity = property.GetValue(this) as Entity;
                    if (subEntity != null)
                    {
                        subEntities.Add(subEntity);
                    }

                    continue;
                }

                if (property.Name == nameof(Id) && property.PropertyType == typeof(Guid))
                {
                    var id = (Guid)property.GetValue(this);

                    if (id == Guid.Empty)
                    {
                        property.SetValue(this, GlobalIdGenerator.NewId());
                    }

                    continue;
                }

                var collectionItemType = property.PropertyType.GetCollectionItemType();
                if (collectionItemType != null)
                {
                    var originValue = property.GetValue(this);
                    var subEntityCollection = originValue as IEnumerable;

                    if (subEntityCollection != null)
                    {
                        subEntities.AddRange(subEntityCollection.OfType<Entity>());
                    }
                }
            }

            checkedObjects.Add(Id);

            foreach (var subEntity in subEntities)
            {
                subEntity.CheckConsistency(checkedObjects);
            }
        }

        protected void CheckRelationIsNotEmpty(Guid id)
        {
            if (id == Guid.Empty)
            {
                throw new EntityConsistencyException(this, "Relation should not be empty");
            }
        }

        protected virtual void CheckConsistencySpecific()
        {
        }

        public override string ToString()
        {
            return string.Concat("[", GetType().Name, ", ", nameof(Id), " = ", Id, "]");
        }

        public ICollection<IDomainEvent> ExtractEvents()
        {
            var result = new List<IDomainEvent>(_events);
            _events.Clear();
            return result;
        }        
    }
}