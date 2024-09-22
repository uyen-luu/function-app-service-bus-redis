namespace IPS.Grow.Func.Models
{
    public class ProductResponse : ProductMessage
    {
        public required int Id { get; set; }
        public required string[] Categories { get; set; }
    }
}
