using LiteDB;

namespace Configuration.LiteDb;

public interface ILiteDbContext
{
    LiteDatabase Database { get; }
}