import { Injectable, inject } from '@angular/core';
import { MatDialog, MatDialogRef } from '@angular/material/dialog';
import {
  FeedbackDialogComponent,
  FeedbackDialogData,
  FeedbackKind
} from '../components/feedback-dialog/feedback-dialog.component';

@Injectable({ providedIn: 'root' })
export class FeedbackService {
  private readonly dialog = inject(MatDialog);

  error(title: string, detail?: string): MatDialogRef<FeedbackDialogComponent> {
    return this.open('error', title, detail);
  }

  success(title: string, detail?: string): MatDialogRef<FeedbackDialogComponent> {
    return this.open('success', title, detail);
  }

  info(title: string, detail?: string): MatDialogRef<FeedbackDialogComponent> {
    return this.open('info', title, detail);
  }

  private open(
    kind: FeedbackKind,
    title: string,
    detail?: string
  ): MatDialogRef<FeedbackDialogComponent> {
    return this.dialog.open<FeedbackDialogComponent, FeedbackDialogData>(
      FeedbackDialogComponent,
      {
        data: { kind, title, detail },
        width: '360px',
        panelClass: 'feedback-dialog-panel',
        disableClose: false,
        autoFocus: 'dialog'
      }
    );
  }
}
