using System;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using SoftDeleteDemo.Entities;

namespace SoftDeleteDemo.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        private readonly IHttpContextAccessor _contextAccessor;
        private Guid _transactionSequence;

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options,
            IHttpContextAccessor contextAccessor)
            : base(options)
        {
            _contextAccessor = contextAccessor;
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            AddSafeDeleteGlobalQuery(builder);
            base.OnModelCreating(builder);
        }

        public DbSet<DemoModel> DemoModel { get; set; }

        public void SetGlobalQuery<T>(ModelBuilder builder) where T : GenericModel
        {
            builder.Entity<T>().HasQueryFilter(e => e.RecStatus.Equals('A'));
        }

        private static readonly MethodInfo SetGlobalQueryMethod = typeof(ApplicationDbContext)
            .GetMethods(BindingFlags.Public | BindingFlags.Instance)
            .Single(t => t.IsGenericMethod && t.Name == "SetGlobalQuery");

        private void AddSafeDeleteGlobalQuery(ModelBuilder builder)
        {
            foreach (var type in builder.Model.GetEntityTypes())
            {
                if (type.BaseType != null || !typeof(ISoftDelete).IsAssignableFrom(type.ClrType)) continue;
                var method = SetGlobalQueryMethod.MakeGenericMethod(type.ClrType);
                method.Invoke(this, new object[] { builder });
            }
        }

        public override Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess,
            CancellationToken cancellationToken = new CancellationToken())
        {
            foreach (var entry in ChangeTracker.Entries())
            {
                var entity = entry.Entity;
                if (entry.State != EntityState.Deleted || entity is not ISoftDelete) continue;
                entry.State = EntityState.Modified;
                entity.GetType().GetProperty("RecStatus")?.SetValue(entity, 'D');
            }

            BeforeSaveChanges();
            return base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
        }

        public override int SaveChanges()
        {
            foreach (var entry in ChangeTracker.Entries())
            {
                var entity = entry.Entity;
                if (entry.State != EntityState.Deleted || entity is not ISoftDelete) continue;
                entry.State = EntityState.Modified;
                entry.GetType().GetProperty("RecStatus")?.SetValue(entity, 'D');
            }

            BeforeSaveChanges();
            return base.SaveChanges();
        }

        private void BeforeSaveChanges()
        {
            ChangeTracker.DetectChanges();
            var changedEntries = ChangeTracker.Entries().Where(x =>
                x.State == EntityState.Added || x.State == EntityState.Modified || x.State == EntityState.Deleted);
            InitializeTransactionSequenceIfNeeded();
            foreach (var model in changedEntries)
            {
                switch (model.State)
                {
                    case EntityState.Added:
                        if (model.Entity is IRecordInfo recordInfo)
                        {
                            var username = _contextAccessor.HttpContext?.User?.Identity?.Name;
                            recordInfo.User = Users.FirstOrDefault(x => x.Email.Equals(username));
                        }

                        if (model.Entity is GenericModel baseModel)
                        {
                            AttachTransactionSequence(baseModel);
                        }

                        break;
                }
            }
        }

        private void AttachTransactionSequence(GenericModel entity)
        {
            entity.TransactionSequence = _transactionSequence.ToString();
        }

        private void InitializeTransactionSequenceIfNeeded()
        {
            _transactionSequence = Guid.NewGuid();
        }
    }
}