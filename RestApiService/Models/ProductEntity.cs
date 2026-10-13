namespace RestApiService.Models
{
    public class ProductEntity
    {
        public string Id { get; set; } = string.Empty; // Clave primaria
        public int Stock { get; set; }
    }
}