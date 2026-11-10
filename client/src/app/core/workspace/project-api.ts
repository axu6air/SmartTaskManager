import { Injectable, inject } from '@angular/core';
import { ApiService } from '@core/http/api.service';
import { Guid, PagedResponse } from '@models/api';
import { HttpContext } from '@angular/common/http';
import { SKIP_LOADING } from '@core/http/http-context';
import { CreateProjectRequest, ProjectMember, ProjectSummary, UpdateProjectRequest, UserSummary } from '@models/project';
import { Observable } from 'rxjs';

@Injectable({ providedIn: 'root' })
export class ProjectApi {
  private readonly api = inject(ApiService);

  list(page: number, pageSize: number): Observable<PagedResponse<ProjectSummary>> {
    return this.api.get('/projects', { params: { page, pageSize } });
  }

  create(body: CreateProjectRequest): Observable<ProjectSummary> {
    return this.api.post('/projects', body);
  }

  update(projectId: Guid, body: UpdateProjectRequest): Observable<ProjectSummary> {
    return this.api.put(`/projects/${projectId}`, body);
  }

  /** Soft delete on the server; the project's tasks go with it. */
  delete(projectId: Guid): Observable<void> {
    return this.api.send('DELETE', `/projects/${projectId}`);
  }

  members(projectId: Guid): Observable<ProjectMember[]> {
    return this.api.get(`/projects/${projectId}/members`);
  }

  /** Adds the user, or changes their manager flag if already a member. */
  assignMember(projectId: Guid, userId: Guid, isManager: boolean): Observable<unknown> {
    return this.api.patch(`/projects/${projectId}/assign`, { userId, isManager });
  }

  /** The server also unassigns the member's tasks in the project. */
  removeMember(projectId: Guid, userId: Guid): Observable<void> {
    return this.api.send('DELETE', `/projects/${projectId}/members/${userId}`);
  }

  /** At most 20 active users matching name or email; needs users.view. */
  searchUsers(search: string): Observable<UserSummary[]> {
    return this.api.get('/users', { params: { search }, context: new HttpContext().set(SKIP_LOADING, true) });
  }
}
