import { Component, input } from '@angular/core';

/**
 * Minimal reusable status line for lists/forms:
 *   loading → "Loading…", error → message, empty → "Nothing to show yet".
 */
@Component({
  selector: 'app-state-message',
  styleUrl: './state-message.css',
  templateUrl: './state-message.html',
})
export class StateMessage {
  readonly kind = input<'loading' | 'error' | 'empty'>('loading');
  readonly message = input('');
}
