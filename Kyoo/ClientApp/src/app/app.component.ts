import { Component } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Event, Router, NavigationStart, NavigationEnd, NavigationCancel, NavigationError } from '@angular/router';
//import * as $ from "jquery";
//import "bootstrap";

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.scss']
})
export class AppComponent
{
  libraries: Library[];
  isLoading: boolean = false;

  constructor(http: HttpClient, private router: Router)
  {
    http.get<Library[]>("api/libraries").subscribe(result =>
    {
      this.libraries = result;
    }, error => console.error(error));

    this.router.events.subscribe((event: Event) =>
    {
      switch (true)
      {
        case event instanceof NavigationStart:
        {
          this.isLoading = true;
          break;
        }

        case event instanceof NavigationEnd:
        case event instanceof NavigationCancel:
        case event instanceof NavigationError:
        {
          this.isLoading = false;
          break;
        }
        default:
        {
          this.isLoading = false;
          break;
        }
      }
    });
  }
}

interface Library
{
  id: number;
  slug: string;
  name: string;
}
