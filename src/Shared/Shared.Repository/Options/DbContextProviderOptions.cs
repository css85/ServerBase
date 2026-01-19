using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Text;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;

namespace Shared.Repository.Options;

public class DbContextProviderOptions : IDbContextOptionsExtension
{
    private DbContextOptionsExtensionInfo _info;

    public DatabaseProviderType Provider { get; private set; }

    public DbContextProviderOptions()
    {
    }

    public DbContextProviderOptions([NotNull] DbContextProviderOptions copyFrom)
    {
        Provider = copyFrom.Provider;
    }

    protected DbContextProviderOptions Clone()
        => new DbContextProviderOptions(this);

    public virtual DbContextProviderOptions WithProvider(DatabaseProviderType provider)
    {
        var clone = Clone();
        clone.Provider = provider;
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

                    builder.Append("Extension.Provider ")
                        .Append(Extension.Provider)
                        .Append(" ");

                    _logFragment = builder.ToString();
                }

                return _logFragment;
            }
        }

        private new DbContextProviderOptions Extension
            => (DbContextProviderOptions)base.Extension;

        public override int GetServiceProviderHashCode()
        {
            if (_serviceProviderHash == null)
            {
                var hashCode = new HashCode();
                hashCode.Add(Extension.Provider);

                _serviceProviderHash = hashCode.ToHashCode();
            }

            return _serviceProviderHash.Value;
        }


        public override bool ShouldUseSameServiceProvider(DbContextOptionsExtensionInfo other)
            => other is ExtensionInfo otherInfo &&
               Extension.Provider == otherInfo.Extension.Provider;

        public override void PopulateDebugInfo(IDictionary<string, string> debugInfo)
        {
            debugInfo["Shared.Repository.DbContextProviderOptions"] = HashCode.Combine(Extension.Provider).ToString(CultureInfo.InvariantCulture);
        }
    }

    public DbContextOptionsExtensionInfo Info => _info ??= new ExtensionInfo(this);
}