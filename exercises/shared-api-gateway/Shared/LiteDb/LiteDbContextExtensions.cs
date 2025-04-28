using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Configuration.LiteDb;

public static class LiteDbContextExtensions
{
    public static IEnumerable<T> GetAll<T>(this ILiteDbContext dbContext)
    {
        var collection = dbContext.Database.GetCollection<T>();
        return collection.Query().ToEnumerable();
    }

    public static IEnumerable<T> Where<T>(this ILiteDbContext dbContext, Expression<Func<T, bool>> predicate)
    {
        var collection = dbContext.Database.GetCollection<T>();
        return collection.Query().Where(predicate).ToEnumerable();
    }
}