﻿using System;
using System.Collections.Generic;

namespace Frodo.Core.Repositories
{
    public static class RepositoryExtensions
    {
        public static User FindByLogin(this IRepository<User> repository, string login)
        {
            return repository.FindSingle(x => string.Equals(x.Login, login, StringComparison.OrdinalIgnoreCase));
        }
        
        public static User FindByEmail(this IRepository<User> repository, string email)
        {
            return repository.FindSingle(x => string.Equals(x.Email, email, StringComparison.OrdinalIgnoreCase));
        }

        public static ICollection<TimeEntry> NotExported(this IRepository<TimeEntry> repository, User user)
        {
            return repository.FindAll(x => x.UserId == user.Id && x.IsExported == false);
        }
    }
}