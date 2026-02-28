using System.ComponentModel.DataAnnotations;

namespace SecureOrders.Application.Auth;

public sealed record RefreshRequest(
    [Required] string RefreshToken
);
