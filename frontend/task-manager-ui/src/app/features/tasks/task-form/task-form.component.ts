import { ChangeDetectionStrategy, Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatCardModule } from '@angular/material/card';
import { MatChipsModule } from '@angular/material/chips';
import { MatDatepickerModule } from '@angular/material/datepicker';

import { UserService } from '../../../core/services/user.service';
import { TaskService } from '../../../core/services/task.service';
import { FeedbackService } from '../../../core/services/feedback.service';
import { User } from '../../../core/models/user.model';

@Component({
  selector: 'app-task-form',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    RouterLink,
    MatFormFieldModule,
    MatInputModule,
    MatSelectModule,
    MatButtonModule,
    MatIconModule,
    MatCardModule,
    MatChipsModule,
    MatDatepickerModule
  ],
  templateUrl: './task-form.component.html',
  styleUrl: './task-form.component.scss'
})
export class TaskFormComponent implements OnInit {
  private readonly fb = inject(FormBuilder);
  private readonly userService = inject(UserService);
  private readonly taskService = inject(TaskService);
  private readonly feedback = inject(FeedbackService);
  private readonly router = inject(Router);

  readonly users = signal<User[]>([]);
  readonly submitting = signal(false);

  readonly form = this.fb.nonNullable.group({
    title: ['', [Validators.required, Validators.maxLength(200)]],
    userId: this.fb.control<number | null>(null, Validators.required),
    priority: this.fb.nonNullable.control<'' | 'Low' | 'Medium' | 'High' | 'Critical'>(''),
    dueDate: this.fb.control<Date | null>(null),
    tags: this.fb.nonNullable.control('')
  });

  ngOnInit(): void {
    this.userService.list().subscribe(users => this.users.set(users));
  }

  submit(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    const raw = this.form.getRawValue();
    const additionalInfo = this.buildAdditionalInfo(raw);

    this.submitting.set(true);
    this.taskService.create({
      title: raw.title.trim(),
      userId: raw.userId!,
      additionalInfo
    }).subscribe({
      next: created => {
        this.submitting.set(false);
        this.feedback
          .success('Tarea creada', `"${created.title}" se creó correctamente.`)
          .afterClosed()
          .subscribe(() => this.router.navigate(['/tasks']));
      },
      error: () => this.submitting.set(false)
    });
  }

  private buildAdditionalInfo(raw: ReturnType<typeof this.form.getRawValue>): string | null {
    const tags = raw.tags
      .split(',')
      .map(t => t.trim())
      .filter(t => t.length > 0);

    const payload: Record<string, unknown> = {};
    if (raw.priority)        payload['priority'] = raw.priority;
    if (raw.dueDate)         payload['dueDate']  = formatDate(raw.dueDate);
    if (tags.length > 0)     payload['tags']     = tags;

    return Object.keys(payload).length > 0 ? JSON.stringify(payload) : null;
  }
}

function formatDate(d: Date): string {
  const year  = d.getFullYear();
  const month = String(d.getMonth() + 1).padStart(2, '0');
  const day   = String(d.getDate()).padStart(2, '0');
  return `${year}-${month}-${day}`;
}
