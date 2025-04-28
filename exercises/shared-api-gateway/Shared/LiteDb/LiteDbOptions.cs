using System;
using LiteDB;

namespace Configuration.LiteDb;

public class LiteDbOptions(string databaseName, Action<LiteDatabase> databaseInitializer)
{
    public string DatabaseLocation { get; set; } = string.Empty;
    public string DatabaseName { get; set; } = databaseName;
    public Action<LiteDatabase> DatabaseInitializer { get; set; } = databaseInitializer;
}