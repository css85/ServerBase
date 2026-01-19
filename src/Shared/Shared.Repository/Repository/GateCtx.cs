using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Shared.Entities.Models;
using Shared.Repository.Database;
using Shared.Repository.Services;
using Shared.Server.Define;
using System;

namespace Shared.Repository
{
    public sealed class GateCtx : PooledDbContext
    {
        public DbSet<GateServerVersionModel> ServerVersions { get; set; }
        public DbSet<GateServerInfoModel> ServerInfos { get; set; }
        public DbSet<GateServerMaintenanceModel> ServerMaintenances { get; set; }    
        public DbSet<GateCdnInfoModel> CdnInfos { get; set; }


        public GateCtx(DbContextOptions options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasCharSet("utf8mb4", null);

            modelBuilder.Entity<GateServerVersionModel>(e =>
            {
                e.HasKey(p => new { p.ClientVersion, p.OsType });
                e.Property(p => p.OsType).HasConversion(
                    v => v.ToString(),
                    v => (OSType)Enum.Parse(typeof(OSType), v));

                e.Property(p => p.ServerType).HasConversion(
                    v => v.ToString(),
                    v => (ServerLocationType)Enum.Parse(typeof(ServerLocationType), v));
            });

            modelBuilder.Entity<GateServerInfoModel>(e =>
            {
                e.HasKey(p => new {p.ServerType, p.NetServiceType });
                e.Property(p => p.ServerType).HasConversion(
                    v => v.ToString(),
                    v => (ServerLocationType)Enum.Parse(typeof(ServerLocationType), v));

                e.Property(p => p.NetServiceType).HasConversion(
                    v => v.ToString(),
                    v => (NetServiceType)Enum.Parse(typeof(NetServiceType), v));
            });

            modelBuilder.Entity<GateServerMaintenanceModel>(e =>
            {
                e.HasKey(p => p.ServerType);
                e.Property(p => p.ServerType).HasConversion(
                    v => v.ToString(),
                    v => (ServerLocationType)Enum.Parse(typeof(ServerLocationType), v));


                e.Property(p => p.IsServerInspection).HasDefaultValue(false).HasConversion(new BoolToZeroOneConverter<int>());

            });

            modelBuilder.Entity<GateCdnInfoModel>(e =>
            {
                e.HasKey(p => p.ServerType);
                e.Property(p => p.ServerType).HasConversion(
                    v => v.ToString(),
                    v => (ServerLocationType)Enum.Parse(typeof(ServerLocationType), v));
            });

            base.OnModelCreating(modelBuilder);
        }
    }
}

