import { inject, Injectable } from '@angular/core';
import { environment } from '../../../environment';
import { Observable } from 'rxjs';
import { HttpClient } from '@angular/common/http';

@Injectable({
  providedIn: 'root'
})
export class HttpClientService {
  private http = inject(HttpClient);

  private apiUrl = environment.apiURL;

  public get(path: string) : Observable<any> {
    return this.http.get(this.apiUrl + path);
  }

  public post(path: string, item: any) : Observable<any> {
    console.log(this.apiUrl + path);
    return this.http.post(this.apiUrl + path, item);
  }
}
