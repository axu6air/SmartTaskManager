import { Injectable, inject } from '@angular/core';
import { ApiService } from '@core/http/api.service';
import { RegisteredUser, RegisterUserRequest } from '@models/auth';
import { Role, RolesMatrix } from '@models/role';
import { Observable, map } from 'rxjs';

@Injectable({ providedIn: 'root' })
export class RegisterApi {
  private readonly api = inject(ApiService);

  register(body: RegisterUserRequest): Observable<RegisteredUser> {
    return this.api.post('/auth/register', body);
  }

  /** Roles to choose from; needs roles.view (the caller falls back to the default role without it). */
  roles(): Observable<Role[]> {
    return this.api.get<RolesMatrix>('/roles').pipe(map((matrix) => matrix.roles));
  }
}
