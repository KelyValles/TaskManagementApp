import { TaskStatus } from './task-status';

export interface Task {
  id: number;
  title: string;
  userId: number;
  status: TaskStatus;
  createdAt: string;
  additionalInfo?: string | null;
  priority?: string | null;
  dueDate?: string | null;
}

export interface CreateTaskPayload {
  title: string;
  userId: number;
  additionalInfo?: string | null;
}

export interface UpdateTaskStatusPayload {
  newStatus: TaskStatus;
}

export interface TaskFilter {
  userId?: number;
  status?: TaskStatus;
  priority?: string;
}
