using Core.Database;
using Core.Entities;
using Core.Features.Carros.ProductosMasCaros;
using Dapper;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Text;

namespace Core.Features.Carros.EliminarCarro;

public class EliminarCarroRequest : IRequest<bool>
{
    public int Id { get; set; }

    public class EliminarCarroHandler : IRequestHandler<EliminarCarroRequest, bool>
    {
        private readonly IDbContext _dbContext;

        public EliminarCarroHandler(IDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<bool> Handle(EliminarCarroRequest request, CancellationToken cancellationToken)
        {
            var sql = @"DELETE FROM CarroItems WHERE IdCarro = @Id
                        DELETE FROM Carros WHERE Id = @Id;";
            var rowsAffected = await _dbContext.Connection.ExecuteAsync(sql, new { Id = request.Id });
            return rowsAffected > 0;
        }
    }
    public class EliminarCarroValidator : AbstractValidator<EliminarCarroRequest>
    {
        public EliminarCarroValidator()
        {
            RuleFor(x => x.Id)
                .GreaterThan(0).WithMessage("El ID del carro es obligatorio");
        }
    }
}
