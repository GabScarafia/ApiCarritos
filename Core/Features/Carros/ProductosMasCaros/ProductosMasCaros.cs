using Core.Database;
using Core.Entities;
using Dapper;
using FluentValidation;
using MediatR;

namespace Core.Features.Carros.ProductosMasCaros;

public class ProductosMasCarosRequest : IRequest<IEnumerable<Producto>>
{
    public string Dni { get; set; }

    public class ProductosMasCarosHandler : IRequestHandler<ProductosMasCarosRequest, IEnumerable<Producto>>
    {
        private readonly IDbContext _dbContext;

        public ProductosMasCarosHandler(IDbContext dbContext)
        {
            _dbContext = dbContext;
        }
        public async Task<IEnumerable<Producto>> Handle(ProductosMasCarosRequest request, CancellationToken cancellationToken)
        {
            var dbConnection = _dbContext.Connection;
            var sql = @"SELECT DISTINCT p.Id, p.Descripcion, p.Precio
                        FROM Usuarios u
                        INNER JOIN Carros c ON u.Id = c.IdUsuario
                        INNER JOIN CarroItems ci ON c.Id = ci.IdCarro
                        INNER JOIN Productos p ON ci.IdProducto = p.Id
                        WHERE u.Dni = @Dni
                          AND c.Cerrado = 1
                        ORDER BY p.Precio DESC
                        LIMIT 4";
            var items = await dbConnection.QueryAsync<Producto>(sql, new { request.Dni });
            return items;
        }
    }

    public class ProductosMasCarosValidator : AbstractValidator<ProductosMasCarosRequest>
    {
        public ProductosMasCarosValidator()
        {
            RuleFor(x => x.Dni)
                .NotEmpty().WithMessage("El DNI es obligatorio")
                .Length(8).WithMessage("El DNI debe tener 8 caracteres");
        }
    }
}
