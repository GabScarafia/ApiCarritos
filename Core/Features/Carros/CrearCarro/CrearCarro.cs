using Core.Database;
using Core.Entities;
using Core.Exceptions;
using Core.Features.Usuarios.Login;
using Dapper;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Configuration;

namespace Core.Features.Carros.CrearCarro;

public class CrearCarroRequest : IRequest<int>
{
    public string Dni { get; set; }

    public class CrearCarroHandler : IRequestHandler<CrearCarroRequest, int>
    {
        private readonly IDbContext _dbContext;
        private readonly IConfiguration _configuration;

        public CrearCarroHandler(IDbContext dbContext, IConfiguration configuration)
        {
            _dbContext = dbContext;
            _configuration = configuration;
        }

        public async Task<int> Handle(CrearCarroRequest request, CancellationToken cancellationToken)
        {
            var sql = "SELECT Id, Nombre, Dni, Vip FROM Usuarios WHERE Dni = @Dni";
            var usuario = await _dbContext.Connection.QueryFirstOrDefaultAsync<Usuario>(sql, new { Dni = request.Dni});

            if(usuario == null)
                throw new NotFoundException("No se encontro el usuario");

            var IdTipo = GetTipoCarro(usuario.Vip);

            sql = @"INSERT INTO Carros (IdUsuario, IdTipo, Cerrado, Total) 
                    VALUES (@IdUsuario, @IdTipo, @Cerrado, @Total); 
                    SELECT last_insert_rowid()";
            var carroId = await _dbContext.Connection.QueryFirstOrDefaultAsync<int>(sql, new { IdUsuario = usuario.Id, IdTipo = IdTipo, Cerrado = false, Total = 0 });
            return carroId;
        }

        /// <summary>
        /// Obtiene el tipo de carro según si el usuario es VIP o si es una fecha especial.
        /// </summary>
        private int GetTipoCarro(bool usuarioVip)
        {
            if(usuarioVip)
                return (int)TipoCarro.PromocionableVip;
            else
            {
                //ACOMODAR EN EL APPSETTINGS PARA DEFINIR SI ES FECHA ESPECIAL O NO
                bool fechaEspecial = _configuration["FechaEspecial"] == "true";

                if (fechaEspecial)
                    return (int)TipoCarro.PromocionablePorFechaEspecial; 
                else
                    return (int)TipoCarro.Comun; 
            }
        }
    }

    private enum TipoCarro : int
    {
        Comun = 1,
        PromocionablePorFechaEspecial = 2,
        PromocionableVip = 3
    }

    public class CrearCarroValidator : AbstractValidator<CrearCarroRequest>
    {
        public CrearCarroValidator()
        {
            RuleFor(x => x.Dni)
                .NotEmpty().WithMessage("El DNI es obligatorio")
                .MaximumLength(8).WithMessage("El DNI debe tener 8 caracteres");
        }
    }
}
