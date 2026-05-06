export type TaskStatus = 'Pending' | 'InProgress' | 'Done';

export const TASK_STATUSES: readonly TaskStatus[] = ['Pending', 'InProgress', 'Done'] as const;
