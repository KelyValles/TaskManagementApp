import { ChangeDetectionStrategy, Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MAT_DIALOG_DATA, MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';

export type FeedbackKind = 'error' | 'success' | 'info';

export interface FeedbackDialogData {
  kind: FeedbackKind;
  title: string;
  detail?: string;
  okLabel?: string;
}

@Component({
  selector: 'app-feedback-dialog',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [CommonModule, MatDialogModule, MatButtonModule, MatIconModule],
  templateUrl: './feedback-dialog.component.html',
  styleUrl: './feedback-dialog.component.scss'
})
export class FeedbackDialogComponent {
  readonly data = inject<FeedbackDialogData>(MAT_DIALOG_DATA);
  private readonly ref = inject(MatDialogRef<FeedbackDialogComponent>);

  get icon(): string {
    switch (this.data.kind) {
      case 'success': return 'check';
      case 'info':    return 'info';
      default:        return 'close';
    }
  }

  close(): void {
    this.ref.close();
  }
}
