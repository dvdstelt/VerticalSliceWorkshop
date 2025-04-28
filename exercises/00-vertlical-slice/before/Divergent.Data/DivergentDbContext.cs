using Configuration;
using Configuration.LiteDb;

namespace Divergent.Data;

public class DivergentDbContext(LiteDbOptions options) : LiteDbContext(options);