using Api.Extensions;
using Core.Features.Carros.AgregarProducto;
using Core.Features.Carros.CrearCarro;
using Core.Features.Carros.EliminarCarro;
using Core.Features.Carros.EliminarProducto;
using Core.Features.Carros.ProductosMasCaros;
using MediatR;

namespace Api.Endpoints;

public static class CarroEndpoints
{
    private static readonly string _BaseUrl = "/api/carro";

    public static void MapCarrosEndpoints(this IEndpointRouteBuilder app)
    {
        app.CrearCarro();
        app.EliminarCarro();
        app.AgregarProducto();
        app.EliminarProducto();
        app.ProductosMasCaros();
    }

    private static void CrearCarro(this IEndpointRouteBuilder app)
    {
        app.MapPost($"{_BaseUrl}", async (CrearCarroRequest request, IMediator mediator) =>
        {
            var carro = await mediator.Send(request);
            return Results.Ok(carro);
        })
       .WithValidation<CrearCarroRequest>()
       .WithTags("Carros")
       .WithName("CrearCarro")
       .WithSummary("Crea un nuevo carro de compras")
       .WithDescription("Este endpoint recibe los datos iniciales y genera un carro de compras nuevo en el sistema.");
    }

    private static void EliminarCarro(this IEndpointRouteBuilder app)
    {
        app.MapDelete($"{_BaseUrl}/{{id}}", async (int id, IMediator mediator) =>
        {
            EliminarCarroRequest eliminarCarro = new() { Id = id };
            var carro = await mediator.Send(eliminarCarro);
            return Results.Ok(carro);
        })
        .WithValidation<EliminarCarroRequest>()
        .WithTags("Carros")
        .WithName("EliminarCarro")
        .WithSummary("Elimina un carro de compras")
        .WithDescription("Este endpoint recibe el id de un carro y lo elimina");
    }

    private static void AgregarProducto(this IEndpointRouteBuilder app)
    {
        app.MapPost($"{_BaseUrl}/{{id}}/productos", async (int id, AgregarProductoRequest request, IMediator mediator) =>
        {
            AgregarProductoRequest agregarProductoRequest = new() { IdProducto = request.IdProducto, IdCarro = id };
            var carro = await mediator.Send(agregarProductoRequest);
            return Results.Ok(carro);
        })
        .WithValidation<AgregarProductoRequest>()
        .WithTags("Carros")
        .WithName("AgregarProducto")
        .WithSummary("Agrega un producto a un carro de compras")
        .WithDescription("Este endpoint recibe el id de un carro, el id producto y crea la relacion");
    }

    private static void EliminarProducto(this IEndpointRouteBuilder app)
    {
        app.MapDelete($"{_BaseUrl}/{{id}}/productos/{{idProducto}}", async (int id, int idProducto, IMediator mediator) =>
        {
            var request = new EliminarProductoRequest { IdCarro = id, IdProducto = idProducto };
            var carro = await mediator.Send(request);
            return Results.Ok(carro);
        })
        .WithValidation<EliminarProductoRequest>()
        .WithTags("Carros")
        .WithName("EliminarProducto")
        .WithSummary("Elimina un producto de un carro de compras")
        .WithDescription("Este endpoint recibe el id de un carro, el id producto y elimina la relacion");
    }

    // Este enrealidad deberia ir en su archivo separado un comprasEndpoint
    private static void ProductosMasCaros(this IEndpointRouteBuilder app)
    {
        app.MapGet($"/api/compras/productos/mas-caros/{{dni}}", async (string dni, IMediator mediator) =>
        {
            var request = new ProductosMasCarosRequest { Dni = dni };
            var productos = await mediator.Send(request);
            return Results.Ok(productos);
        })
        .WithValidation<ProductosMasCarosRequest>()
        .WithTags("Compras")
        .WithName("ProductosMasCaros")
        .WithSummary("Recupera los productos mas caros comprados por el usuario")
        .WithDescription("A partir del dni de un usuario, recupera los productos mas caros comprados por el usuario en su historia");
    }
}
