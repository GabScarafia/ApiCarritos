using Core.Entities;
using Dapper;
using System.Data;

namespace Core.Features.Carros.Shared;

public static class CarroHelper
{
    public static decimal CalcularTotalConReglas(Carro carro)
    {
        var cantidadProductos = carro.Items.Count();
        var subtotal = carro.Items.Sum(x => x.Precio);

        if (cantidadProductos == 5)
        {
            return subtotal * 0.80m; // Descuento del 20%
        }

        if (cantidadProductos > 10)
        {
            if (carro.IdTipo == 1) 
                return Math.Max(0, subtotal - 200m);
            else if (carro.IdTipo == 2) 
                return Math.Max(0, subtotal - 500m);
            else if (carro.IdTipo == 3) 
            {
                var productoMasBarato = carro.Items.Min(x => x.Precio);
                return Math.Max(0, subtotal - productoMasBarato - 700m);
            }
        }

        return subtotal;
    }
    public static async Task<bool> ExisteCarroAsync(IDbConnection connection, int idCarro)
    {
        var sql = "SELECT 1 FROM CARROS WHERE Id = @Id LIMIT 1;";
        var result = await  connection.ExecuteScalarAsync<bool>(sql, new { Id = idCarro });
        return result;
    }
    public static async Task ActualizarTotalAsync(IDbConnection connection, int idCarro, decimal total)
    {
        var sql = "UPDATE Carros SET Total = @Total WHERE Id = @Id;";
        await connection.ExecuteAsync(sql, new { Total = total, Id = idCarro });
    }

    public static async Task<Carro> GetCarroCompletoAsync(IDbConnection connection, int idCarro)
    {
        var sql = @"SELECT Id, IdUsuario, IdTipo, Cerrado, Total FROM Carros WHERE Id = @Id;";
        var carro = await connection.QueryFirstOrDefaultAsync<Carro>(sql, new { Id = idCarro });
        
        if (carro != null)
        {
            sql = @"SELECT p.Id, p.Descripcion, p.Precio 
                FROM CarroItems ci
                JOIN Productos p ON ci.IdProducto = p.Id
                WHERE ci.IdCarro = @Id";
            
            var carroItems = await connection.QueryAsync<Producto>(sql, new { Id = idCarro });
            carro.Items = carroItems.ToList();
        }
        
        return carro;
    }
}
