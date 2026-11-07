using MediatR;
using SmartTaskManager.Application.Common.Results;

namespace SmartTaskManager.Application.Features.Projects.Queries.GetProject;

public sealed record GetProjectQuery(Guid ProjectId) : IRequest<Result<ProjectResponse>>;
