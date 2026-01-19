using System;
using Microsoft.EntityFrameworkCore;

namespace Shared.Repository.Database
{
    public class DatabaseOption
    {
        public Type DbContextType;

        public string[] ConnectionStrings;

        public int ShardCount => ConnectionStrings.Length;
        public bool EnableQueryLogging = true;
    }

    public class DatabaseRepositoryServiceOptions
    {
        public DatabaseOption[] DatabaseOptions;
    }
}