using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Text;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;

namespace Shared.Repository.Options;

public class DbContextShardIndexOptions : IDbContextOptionsExtension
{
    private DbContextOptionsExtensionInfo _info;

    public int ShardIndex { get; private set; }

    public DbContextShardIndexOptions()
    {
    }

    public DbContextShardIndexOptions([NotNull] DbContextShardIndexOptions copyFrom)
    {
        ShardIndex = copyFrom.ShardIndex;
    }

    protected DbContextShardIndexOptions Clone()
        => new DbContextShardIndexOptions(this);

    public virtual DbContextShardIndexOptions WithShardIndex(int shardIndex)
    {
        var clone = Clone();
        clone.ShardIndex = shardIndex;
        return clone;
    }

    public void ApplyServices(IServiceCollection services)
    {
    }

    public void Validate(IDbContextOptions options)
    {
    }

    private sealed class ExtensionInfo : DbContextOptionsExtensionInfo
    {
        private int? _serviceProviderHash;
        private string _logFragment;

        public ExtensionInfo(IDbContextOptionsExtension extension) : base(extension)
        {
        }

        public override bool IsDatabaseProvider => false;

        public override string LogFragment
        {
            get
            {
                if (_logFragment == null)
                {
                    var builder = new StringBuilder();

                    builder.Append("Extension.ShardIndex ")
                        .Append(Extension.ShardIndex)
                        .Append(" ");

                    _logFragment = builder.ToString();
                }

                return _logFragment;
            }
        }

        private new DbContextShardIndexOptions Extension
            => (DbContextShardIndexOptions)base.Extension;

        public override int GetServiceProviderHashCode()
        {
            if (_serviceProviderHash == null)
            {
                var hashCode = new HashCode();
                hashCode.Add(Extension.ShardIndex);

                _serviceProviderHash = hashCode.ToHashCode();
            }

            return _serviceProviderHash.Value;
        }


        public override bool ShouldUseSameServiceProvider(DbContextOptionsExtensionInfo other)
            => other is ExtensionInfo otherInfo &&
               Extension.ShardIndex == otherInfo.Extension.ShardIndex;

        public override void PopulateDebugInfo(IDictionary<string, string> debugInfo)
        {
            debugInfo["Shared.Repository.DbContextProviderOptions"] = HashCode.Combine(Extension.ShardIndex).ToString(CultureInfo.InvariantCulture);
        }
    }

    public DbContextOptionsExtensionInfo Info => _info ??= new ExtensionInfo(this);
}