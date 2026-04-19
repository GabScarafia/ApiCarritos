namespace Core.Entities;

public class Carro
{
    public int Id { get; set; }
    public IEnumerable<Producto> Items { get; set; }
    public int IdUsuario { get; set; }
    public int IdTipo { get; set; }
    public bool Cerrado { get; set; }
    public decimal Total { get; set; }
}
