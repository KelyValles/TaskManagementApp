import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import {
  CreateTaskPayload,
  Task,
  TaskFilter,
  UpdateTaskStatusPayload
} from '../models/task.model';
import { TaskStatus } from '../models/task-status';

@Injectable({ providedIn: 'root' })
export class TaskService {
  private readonly http = inject(HttpClient);
  private readonly base = '/api/tasks';

  list(filter: TaskFilter = {}): Observable<Task[]> {
    let params = new HttpParams();
    if (filter.userId)   params = params.set('userId', filter.userId);
    if (filter.status)   params = params.set('status', filter.status);
    if (filter.priority) params = params.set('priority', filter.priority);
    return this.http.get<Task[]>(this.base, { params });
  }

  create(payload: CreateTaskPayload): Observable<Task> {
    return this.http.post<Task>(this.base, payload);
  }

  changeStatus(id: number, newStatus: TaskStatus): Observable<Task> {
    const body: UpdateTaskStatusPayload = { newStatus };
    return this.http.put<Task>(`${this.base}/${id}/status`, body);
  }
}
