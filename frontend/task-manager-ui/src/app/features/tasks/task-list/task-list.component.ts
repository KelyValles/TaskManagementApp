import { ChangeDetectionStrategy, Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule, DatePipe } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { MatTableModule } from '@angular/material/table';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatSelectModule } from '@angular/material/select';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatCardModule } from '@angular/material/card';
import { MatMenuModule } from '@angular/material/menu';
import { MatTooltipModule } from '@angular/material/tooltip';

import { TaskService } from '../../../core/services/task.service';
import { UserService } from '../../../core/services/user.service';
import { Task } from '../../../core/models/task.model';
import { User } from '../../../core/models/user.model';
import { TASK_STATUSES, TaskStatus } from '../../../core/models/task-status';

@Component({
  selector: 'app-task-list',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [
    CommonModule,
    FormsModule,
    RouterLink,
    DatePipe,
    MatTableModule,
    MatFormFieldModule,
    MatSelectModule,
    MatButtonModule,
    MatIconModule,
    MatProgressSpinnerModule,
    MatCardModule,
    MatMenuModule,
    MatTooltipModule
  ],
  templateUrl: './task-list.component.html',
  styleUrl: './task-list.component.scss'
})
export class TaskListComponent implements OnInit {
  private readonly taskService = inject(TaskService);
  private readonly userService = inject(UserService);

  readonly statuses = TASK_STATUSES;
  readonly columns = ['title', 'user', 'status', 'priority', 'createdAt', 'actions'];

  readonly tasks = signal<Task[]>([]);
  readonly users = signal<User[]>([]);
  readonly loading = signal(false);
  readonly statusFilter = signal<TaskStatus | ''>('');

  ngOnInit(): void {
    this.userService.list().subscribe(users => this.users.set(users));
    this.refresh();
  }

  onFilterChange(value: TaskStatus | ''): void {
    this.statusFilter.set(value);
    this.refresh();
  }

  refresh(): void {
    this.loading.set(true);
    const status = this.statusFilter();
    this.taskService.list(status ? { status } : {}).subscribe({
      next: tasks => {
        this.tasks.set(tasks);
        this.loading.set(false);
      },
      error: () => this.loading.set(false)
    });
  }

  changeStatus(task: Task, newStatus: TaskStatus): void {
    if (task.status === newStatus) return;
    this.taskService.changeStatus(task.id, newStatus).subscribe({
      next: updated => {
        this.tasks.update(list =>
          list.map(t => (t.id === updated.id ? updated : t))
        );
      }
    });
  }

  userOf(id: number): User | undefined {
    return this.users().find(u => u.id === id);
  }

  statusLabel(status: TaskStatus): string {
    return { Pending: 'Pendiente', InProgress: 'En curso', Done: 'Completada' }[status];
  }

  statusIcon(status: TaskStatus): string {
    return { Pending: 'schedule', InProgress: 'autorenew', Done: 'check_circle' }[status];
  }

  priorityLabel(priority: string): string {
    const map: Record<string, string> = {
      Low: 'Baja',
      Medium: 'Media',
      High: 'Alta',
      Critical: 'Crítica'
    };
    return map[priority] ?? priority;
  }
}
