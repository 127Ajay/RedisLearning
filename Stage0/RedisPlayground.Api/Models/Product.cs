namespace RedisPlayground.Api.Models
{
    public class Product
    {
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public decimal Price { get; set; }

        public string Category { get; set; } = string.Empty;

        public bool IsActive { get; set; }
    }

    public class ProductTTL
    {
        public int Id { get; set; }
        public int Seconds { get; set; }
    }

}
