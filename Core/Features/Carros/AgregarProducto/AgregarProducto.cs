using Core.Database;
using Core.Entities;
using Core.Features.Carros.EliminarCarro;
using Dapper;
using FluentValidation;
using MediatR;
using System.Text.Json.Serialization;

namespace Core.Features.Carros.AgregarProducto;

public class AgregarProductoRequest : IRequest<Carro>
{
    [JsonIgnore]
    public int IdCarro { get; set; }
    public int IdProducto { get; set; }

    public class AgregarProductoRequestHandler : IRequestHandler<AgregarProductoRequest, Carro>
    {
        private readonly IDbContext _dbContext;

        public AgregarProductoRequestHandler(IDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<Carro> Handle(AgregarProductoRequest request, CancellationToken cancellationToken)
        {
            var dbConnection = _dbContext.Connection;
            var existe = await Shared.CarroHelper.ExisteCarroAsync(dbConnection, request.IdCarro);
            if (!existe)
                throw new Exceptions.NotFoundException("No se encontro el Carro");

            var sql = @"INSERT INTO CarroItems (IdCarro, IdProducto) VALUES (@IdCarro, @IdProducto);";
            await dbConnection.ExecuteAsync(sql, new {request.IdCarro,request.IdProducto });
            
            // 1. Traer el carro completo con los items de la BD
            var carro = await Shared.CarroHelper.GetCarroCompletoAsync(dbConnection, request.IdCarro);
            
            // 2. Ejecutar las reglas de negocio sobre el objeto en memoria
            carro.Total = Shared.CarroHelper.CalcularTotalConReglas(carro);
            
            // 3. Persistir el nuevo total en la BD
            await Shared.CarroHelper.ActualizarTotalAsync(dbConnection, carro.Id, carro.Total);

            return carro;
        }
    }
    public class AgregarProductoValidator : AbstractValidator<AgregarProductoRequest>
    {
        public AgregarProductoValidator()
        {
        //    RuleFor(x => x.IdCarro)
        //        .GreaterThan(0).WithMessage("El ID del carro es obligatorio");
            RuleFor(x => x.IdProducto)
                .GreaterThan(0).WithMessage("El ID del producto es obligatorio");
        }
    }
}
