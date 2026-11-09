import { Injectable, inject } from '@angular/core';
import { ApiService } from '@core/http/api.service';
import { Guid } from '@models/api';
import { RoleGrant, RolesMatrix } from '@models/role';
import { Observable } from 'rxjs';

@Injectable({ providedIn: 'root' })
export class RolesApi {
  private readonly api = inject(ApiService);

  matrix(): Observable<RolesMatrix> {
    return this.api.get('/roles');
  }

  grant(roleId: Guid, permissionId: Guid, scope: number): Observable<RoleGrant> {
    return this.api.put(`/roles/${roleId}/permissions/${permissionId}`, { scope });
  }

  revoke(roleId: Guid, permissionId: Guid): Observable<void> {
    return this.api.send('DELETE', `/roles/${roleId}/permissions/${permissionId}`);
  }
}
