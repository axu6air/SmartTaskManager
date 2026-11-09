using MediatR;
using SmartTaskManager.Application.Common.Results;

namespace SmartTaskManager.Application.Features.Auth.Queries.GetMe;

public sealed record GetMeQuery : IRequest<Result<MeResponse>>;
