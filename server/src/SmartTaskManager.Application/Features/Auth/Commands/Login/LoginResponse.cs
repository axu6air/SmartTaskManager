namespace SmartTaskManager.Application.Features.Auth.Commands.Login;

public sealed record LoginResponse(string AccessToken, string RefreshToken, DateTimeOffset AccessTokenExpiresAt);
