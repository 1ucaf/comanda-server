namespace Comanda.Core.Entities;

public class Order
{
    public Guid Id { get; set; }
    public int TableNumber { get; set; }
    public OrderStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public Guid CreatedByUserId { get; set; }
    public User CreatedByUser { get; set; } = null!;
    public ICollection<OrderItem> Items { get; set; } = new List<OrderItem>();
}
