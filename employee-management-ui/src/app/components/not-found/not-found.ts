import { Component, inject } from '@angular/core';
import { ActivatedRoute, RouterLink } from '@angular/router';

@Component({
  selector: 'app-not-found',
  imports: [RouterLink],
  templateUrl: './not-found.html',
  styleUrl: './not-found.css',
})
export class NotFoundComponent
{
  private route = inject(ActivatedRoute);

  isExpiredSession = this.route.snapshot.queryParamMap.get('reason') === 'expired';
}