using Core.Entities;
using Core.Database;
using Dapper;
using FluentValidation;
using MediatR;

namespace Core.Features.Usuarios.Login;


public class LoginRequest : IRequest<Usuario>
{
    public string Nombre { get; set; }
    public string Clave { get; set; }

    public class LoginHandler : IRequestHandler<LoginRequest, Usuario>
    {
        private readonly IDbContext _dbContext;

        public LoginHandler(IDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<Usuario> Handle(LoginRequest request, CancellationToken cancellationToken)
        {
            var sql = "SELECT Id, Nombre, Dni, Vip FROM Usuarios WHERE Nombre = @Nombre AND Clave = @Clave";
            var usuario = await _dbContext.Connection.QueryFirstOrDefaultAsync<Usuario>(
                sql, new { Nombre = request.Nombre, Clave = request.Clave });

            return usuario;
        }

    }

    public class LoginValidator : AbstractValidator<LoginRequest>
    {
        public LoginValidator()
        {
            RuleFor(x => x.Nombre)
                .NotEmpty().WithMessage("El Nombre es obligatorio");

            RuleFor(x => x.Clave)
                .NotEmpty().WithMessage("La Clave es obligatoria");
        }
    }
}