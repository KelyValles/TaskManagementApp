import { ChangeDetectionStrategy, Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatTableModule } from '@angular/material/table';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatCardModule } from '@angular/material/card';

import { UserService } from '../../../core/services/user.service';
import { FeedbackService } from '../../../core/services/feedback.service';
import { User } from '../../../core/models/user.model';

@Component({
  selector: 'app-user-list',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MatTableModule,
    MatFormFieldModule,
    MatInputModule,
    MatButtonModule,
    MatIconModule,
    MatCardModule
  ],
  templateUrl: './user-list.component.html',
  styleUrl: './user-list.component.scss'
})
export class UserListComponent implements OnInit {
  private readonly fb = inject(FormBuilder);
  private readonly userService = inject(UserService);
  private readonly feedback = inject(FeedbackService);

  readonly users = signal<User[]>([]);
  readonly columns = [ 'name', 'email'];
  readonly submitting = signal(false);

  readonly form = this.fb.nonNullable.group({
    name: ['', [Validators.required, Validators.maxLength(100)]],
    email: ['', [Validators.required, Validators.email, Validators.maxLength(100)]]
  });

  ngOnInit(): void {
    this.refresh();
  }

  refresh(): void {
    this.userService.list().subscribe(users => this.users.set(users));
  }

  submit(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }
    this.submitting.set(true);
    this.userService.create(this.form.getRawValue()).subscribe({
      next: created => {
        this.users.update(list => [...list, created]);
        this.form.reset({ name: '', email: '' });
        this.submitting.set(false);
        this.feedback.success(
          'Usuario creado',
          `${created.name} se registró correctamente.`
        );
      },
      error: () => this.submitting.set(false)
    });
  }
}
