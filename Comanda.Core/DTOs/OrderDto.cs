namespace Comanda.Core.DTOs;

public record OrderItemDto(Guid Id, string ProductName, int Quantity);

public record CreateOrderItemDto(string ProductName, int Quantity);

public record CreateOrderRequest(int TableNumber, List<CreateOrderItemDto> Items);

public record UpdateOrderStatusRequest(string Status);

public record OrderResponse(
    Guid Id,
    int TableNumber,
    string Status,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    Guid CreatedByUserId,
    List<OrderItemDto> Items
);
