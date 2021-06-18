# SoftDelete
This is a demonstration of soft delete as well as a global query filter for soft deleted items

``ApplicatonDbContext.cs``
```c#
public override Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess,
            CancellationToken cancellationToken = new CancellationToken())
        {
            foreach (var entry in ChangeTracker.Entries())
            {
                var entity = entry.Entity;
                if (entry.State == EntityState.Deleted && entity is ISoftDelete)
                {
                    entry.State = EntityState.Modified;
                    entity.GetType().GetProperty("RecStatus")?.SetValue(entity, 'D');
                }
            }

            return base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
        }
```
Note : If we Only Check the  `EntityState.Deleted` So we need to add a filter every time to get Actual data like
```c#
_context.entity.where(x=>x.RecStatus == 'A')
```
But If we add the Global Filter like 

``ApplicationDbContext.cs``

```c#
public void SetGlobalQuery<T>(ModelBuilder builder) where T : GenericModel
        {
            builder.Entity<T>().HasQueryFilter(e => e.RecStatus.Equals('A'));
        }

        static readonly MethodInfo SetGlobalQueryMethod = typeof(ApplicationDbContext)
            .GetMethods(BindingFlags.Public | BindingFlags.Instance)
            .Single(t => t.IsGenericMethod && t.Name == "SetGlobalQuery");

        private void AddSafeDeleteGlobalQuery(ModelBuilder builder)
        {
            foreach (var type in builder.Model.GetEntityTypes())
            {
                if (type.BaseType == null && typeof(ISoftDelete).IsAssignableFrom(type.ClrType))
                {
                    var method = SetGlobalQueryMethod.MakeGenericMethod(type.ClrType);
                    method.Invoke(this, new object[] {builder});
                }
            }
        }
```
It Execute every time `SetGlobalQuery`, so no need to add filter to get Actual Data
