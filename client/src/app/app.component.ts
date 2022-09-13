import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Component, OnInit } from '@angular/core';

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.css']
})
export class AppComponent implements OnInit {
  title = 'SocialApp';
  users: any;

  constructor(private http: HttpClient) { }

  ngOnInit() {
    this.getUsers();
  }

  getUsers() {
    const headers = new HttpHeaders({ 'Content-Type': 'application/json; charset=utf-8' });

    this.http.get('https://localhost:5001/api/users', { headers }).subscribe({
      next: (res) => this.users = res,
      error: (e) => console.error(e)
    });
  }
}
