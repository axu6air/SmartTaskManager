import { Injectable, inject } from '@angular/core';
import { ApiService } from '@core/http/api.service';
import { Guid, ListQuery, PagedResponse } from '@models/api';
import {
  CreateTaskRequest,
  ProjectTask,
  TaskListFilters,
  TaskStatus,
  UpdateTaskRequest,
} from '@models/task';
import { toParams } from '@shared/utils/query-params';
import { Observable } from 'rxjs';

@Injectable({ providedIn: 'root' })
export class TaskApi {
  private readonly api = inject(ApiService);

  list(projectId: Guid, query: ListQuery & TaskListFilters): Observable<PagedResponse<ProjectTask>> {
    return this.api.get(`/projects/${projectId}/tasks`, { params: toParams(query) });
  }

  create(projectId: Guid, body: CreateTaskRequest): Observable<ProjectTask> {
    return this.api.post(`/projects/${projectId}/tasks`, body);
  }

  update(task: ProjectTask, body: UpdateTaskRequest): Observable<ProjectTask> {
    return this.api.put(`/projects/${task.projectId}/tasks/${task.id}`, body);
  }

  assign(task: ProjectTask, assigneeId: Guid | null): Observable<ProjectTask> {
    return this.api.patch(`/projects/${task.projectId}/tasks/${task.id}/assign`, { assigneeId });
  }

  changeStatus(task: ProjectTask, status: TaskStatus): Observable<ProjectTask> {
    return this.api.patch(`/projects/${task.projectId}/tasks/${task.id}/status`, { status });
  }

  delete(task: ProjectTask): Observable<void> {
    return this.api.send('DELETE', `/projects/${task.projectId}/tasks/${task.id}`);
  }
}
