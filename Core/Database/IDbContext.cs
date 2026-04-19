using System.Data;

namespace Core.Database;

public interface IDbContext
{
    IDbConnection Connection { get; }
}
