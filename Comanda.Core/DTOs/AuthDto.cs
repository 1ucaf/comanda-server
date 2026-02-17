namespace Comanda.Core.DTOs;

public record LoginRequest(string Name, string Role);

public record LoginResponse(Guid Id, string Name, string Role);
