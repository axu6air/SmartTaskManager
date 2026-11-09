import { Guid, OffsetDateString } from './api';

/** Mirrors ProjectSummary (a GET /api/projects row). */
export interface ProjectSummary {
  id: Guid;
  name: string;
  description: string | null;
  startDate: OffsetDateString | null;
  endDate: OffsetDateString | null;
}

/** Mirrors CreateProjectRequest and UpdateProjectRequest (same fields). The response is a ProjectSummary plus member IDs. */
export interface CreateProjectRequest {
  name: string;
  description: string | null;
  startDate: OffsetDateString | null;
  endDate: OffsetDateString | null;
}

/** Mirrors UserSummary (GET /api/users?search=). */
export interface UserSummary {
  id: Guid;
  fullName: string;
  email: string;
}

/** Mirrors ProjectMemberDetails (GET /api/projects/{id}/members). */
export interface ProjectMember {
  userId: Guid;
  fullName: string;
  email: string;
  isManager: boolean;
}
