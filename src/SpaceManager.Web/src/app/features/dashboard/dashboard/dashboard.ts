import { Component, inject } from '@angular/core';
import { RouterLink } from '@angular/router';
import { AuthService } from '../../../core/services/auth.service';

/** Protected home: current user + links + logout. */
@Component({
  selector: 'app-dashboard',
  imports: [RouterLink],
  styleUrl: './dashboard.css',
  templateUrl: './dashboard.html',
})
export class Dashboard {
  readonly auth = inject(AuthService);
}
