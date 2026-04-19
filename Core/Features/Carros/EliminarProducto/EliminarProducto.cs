using Core.Database;
using Core.Entities;
using Core.Features.Carros.AgregarProducto;
using Dapper;
using FluentValidation;
using MediatR;

namespace Core.Features.Carros.EliminarProducto;

public class EliminarProductoRequest : IRequest<Carro>
{
    public int IdCarro { get; set; }
    public int IdProducto { get; set; }

    public class EliminarProductoRequestHandler : IRequestHandler<EliminarProductoRequest, Carro>
    {
        private readonly IDbContext _dbContext;

        public EliminarProductoRequestHandler(IDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<Carro> Handle(EliminarProductoRequest request, CancellationToken cancellationToken)
        {
            var dbConnection = _dbContext.Connection;
            var existe = await Shared.CarroHelper.ExisteCarroAsync(dbConnection, request.IdCarro);
            if (!existe)
                throw new Exceptions.NotFoundException("No se encontro el Carro");

            var sql = @"DELETE FROM CarroItems WHERE IdCarro = @IdCarro AND IdProducto = @IdProducto;";
            await dbConnection.ExecuteAsync(sql, new { request.IdCarro, request.IdProducto });

            var carro = await Shared.CarroHelper.GetCarroCompletoAsync(dbConnection, request.IdCarro);

            // 2. Ejecutar las reglas de negocio sobre el objeto en memoria
            carro.Total = Shared.CarroHelper.CalcularTotalConReglas(carro);
            
            // 3. Persistir el nuevo total en la BD
            await Shared.CarroHelper.ActualizarTotalAsync(dbConnection, carro.Id, carro.Total);

            return carro;
        }
    }
    public class EliminarProductoValidator : AbstractValidator<EliminarProductoRequest>
    {
        public EliminarProductoValidator()
        {
            RuleFor(x => x.IdCarro)
                .GreaterThan(0).WithMessage("El ID del carro es obligatorio");
            RuleFor(x => x.IdProducto)
                .GreaterThan(0).WithMessage("El ID del producto es obligatorio");
        }
    }
}
