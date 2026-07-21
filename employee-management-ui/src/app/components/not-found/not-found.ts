import { Component, inject } from '@angular/core';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { TranslatePipe } from '@ngx-translate/core';

@Component({
  selector: 'app-not-found',
  imports: [RouterLink, TranslatePipe],
  templateUrl: './not-found.html',
  styleUrl: './not-found.css',
})
export class NotFoundComponent
{
  private route = inject(ActivatedRoute);

  isExpiredSession = this.route.snapshot.queryParamMap.get('reason') === 'expired';
}