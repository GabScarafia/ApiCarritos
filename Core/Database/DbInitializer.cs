using Core.Entities;
using Dapper;
using System.Data;

namespace Core.Database;

public static class DbInitializer //CLASE PARA GENERAR DATOS DE PRUEBA
{
    public static void Initialize(IDbConnection connection, bool isInMemory)
    {
        
        connection.Execute(@"
                CREATE TABLE IF NOT EXISTS Usuarios (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT, 
                    Nombre NVARCHAR(100), 
                    Clave NVARCHAR(100), 
                    Dni NVARCHAR(8), 
                    Vip BIT
                );
                CREATE TABLE IF NOT EXISTS Productos (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT, 
                    Descripcion NVARCHAR(100),  
                    Precio DECIMAL
                );
                CREATE TABLE IF NOT EXISTS CarroTipos (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT, 
                    Descripcion NVARCHAR(100)   
                );
                CREATE TABLE IF NOT EXISTS Carros (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT, 
                    IdUsuario INTEGER, 
                    IdTipo INTEGER,
                    Cerrado BIT DEFAULT(0),
                    Total DECIMAL,
                    FOREIGN KEY (IdUsuario) REFERENCES Usuarios(Id),
                    FOREIGN KEY (IdTipo) REFERENCES CarroTipos(Id)
                    
                );
                CREATE TABLE IF NOT EXISTS CarroItems (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT, 
                    IdCarro INTEGER, 
                    IdProducto INTEGER,
                    FOREIGN KEY (IdCarro) REFERENCES Carros(Id),
                    FOREIGN KEY (IdProducto) REFERENCES Productos(Id)
                );

                CREATE INDEX  IF NOT EXISTS idx_usuarios_dni ON Usuarios(Dni);
                CREATE INDEX  IF NOT EXISTS idx_carros_usuario ON Carros(IdUsuario);
                CREATE INDEX  IF NOT EXISTS idx_productos_precio ON Productos(Precio DESC);"
        );

        if (isInMemory)
            connection.SeedData();
      
    }

    private static void SeedData(this IDbConnection connection)
    {
        // 1. Seed de Usuarios (10 usuarios, 3 VIP)
        var userCount = connection.ExecuteScalar<int>("SELECT COUNT(*) FROM Usuarios");
        if (userCount == 0)
        {
            var usuarios = new[]
            {
                new { N = "Juan", Dni = "30123456", Vip = 1 },
                new { N = "Maria", Dni = "31234567", Vip = 1 },
                new { N = "Carlos", Dni = "32345678", Vip = 1 },
                new { N = "Ana", Dni = "33456789", Vip = 0 },
                new { N = "Luis", Dni = "34567890", Vip = 0 },
                new { N = "Elena", Dni = "35678901", Vip = 0 },
                new { N = "Pablo", Dni = "36789012", Vip = 0 },
                new { N = "Lucia", Dni = "37890123", Vip = 0 },
                new { N = "Jorge", Dni = "38901234", Vip = 0 },
                new { N = "Rosa", Dni = "39012345", Vip = 0 },
                new { N = "Gabi", Dni = "42155265", Vip = 1 } 
            };

            foreach (var u in usuarios)
            {
                connection.Execute(
                    "INSERT INTO Usuarios (Nombre, Clave, Dni, Vip) VALUES (@N, @N, @Dni, @Vip)",
                    u);
            }
        }

        var prodCount = connection.ExecuteScalar<int>("SELECT COUNT(*) FROM Productos");
        if (prodCount == 0)
        {
            string sqlProds = @"
            INSERT INTO Productos (Descripcion, Precio) VALUES 
            ('Arroz Integral 1kg', 1200), ('Fideos Tallarin 500g', 950), ('Aceite Girasol 1.5L', 2500),
            ('Leche Entera 1L', 1100), ('Yerba Mate 1kg', 3500), ('Azúcar Blanca 1kg', 1000),
            ('Harina 000 1kg', 850), ('Galletitas Dulces', 1200), ('Puré de Tomate', 700),
            ('Lentejas 400g', 1300), ('Atún en trozos', 1800), ('Café Molido 250g', 4200),
            ('Té común 50 saquitos', 1100), ('Mermelada de Frutilla', 1900), ('Manteca 200g', 1500),
            ('Queso Crema 290g', 2100), ('Pan de Molde', 1600), ('Detergente Lavavajilla', 1400),
            ('Jabón Blanco', 800), ('Papel Higiénico 4 rollos', 1700);";

            connection.Execute(sqlProds);
        }

        var carroTiposCount = connection.ExecuteScalar<int>("SELECT COUNT(*) FROM CarroTipos");
        if (carroTiposCount == 0)
        {
            var carroTipos = new[]
            {
                new { Descripcion = "Comun" },
                new { Descripcion = "Promocionable por Fecha Especial" },
                new { Descripcion = "Promocionable por Usuario VIP" },
            };

            foreach (var u in carroTipos)
            {
                connection.Execute(
                    "INSERT INTO CarroTipos (Descripcion) VALUES (@Descripcion)",
                    u);
            }
        }

        var carroCount = connection.ExecuteScalar<int>("SELECT COUNT(*) FROM Carros");
        if (carroCount == 0)
        {
            var random = new Random();
            var usuariosIds = connection.Query<int>("SELECT Id FROM Usuarios").ToList();
            var productosIds = connection.Query<int>("SELECT Id FROM Productos").ToList();

            foreach (var userId in usuariosIds)
            {
                // Creamos un carro por usuario
                int carroId = connection.QuerySingle<int>(@"
                INSERT INTO Carros (IdUsuario, IdTipo, Cerrado, Total) 
                VALUES (@userId, 1, 1, 0);
                SELECT last_insert_rowid();", new { userId });

                int cantidadItems =  15;
                
                for (int i = 0; i < cantidadItems; i++)
                {
                    int productoId = productosIds[random.Next(productosIds.Count)];
                    connection.Execute(
                        "INSERT INTO CarroItems (IdCarro, IdProducto) VALUES (@carroId, @productoId)",
                        new { carroId, productoId });
                }

                // Opcional: Actualizar el Total del carro sumando los precios de los productos
                connection.Execute(@"
                UPDATE Carros 
                SET Total = (SELECT SUM(p.Precio) 
                             FROM CarroItems ci 
                             JOIN Productos p ON ci.IdProducto = p.Id 
                             WHERE ci.IdCarro = @carroId)
                WHERE Id = @carroId", new { carroId });
            }
        }
    }
}
