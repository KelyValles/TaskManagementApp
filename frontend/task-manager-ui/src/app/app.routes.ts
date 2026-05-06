import { Routes } from '@angular/router';

export const routes: Routes = [
  {
    path: 'tasks',
    loadComponent: () =>
      import('./features/tasks/task-list/task-list.component')
        .then(m => m.TaskListComponent)
  },
  {
    path: 'tasks/new',
    loadComponent: () =>
      import('./features/tasks/task-form/task-form.component')
        .then(m => m.TaskFormComponent)
  },
  {
    path: 'users',
    loadComponent: () =>
      import('./features/users/user-list/user-list.component')
        .then(m => m.UserListComponent)
  },
  { path: '', pathMatch: 'full', redirectTo: 'tasks' },
  { path: '**', redirectTo: 'tasks' }
];
