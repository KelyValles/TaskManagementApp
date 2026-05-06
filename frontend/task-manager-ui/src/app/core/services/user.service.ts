import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { CreateUserPayload, User } from '../models/user.model';

@Injectable({ providedIn: 'root' })
export class UserService {
  private readonly http = inject(HttpClient);
  private readonly base = '/api/users';

  list(): Observable<User[]> {
    return this.http.get<User[]>(this.base);
  }

  create(payload: CreateUserPayload): Observable<User> {
    return this.http.post<User>(this.base, payload);
  }
}
