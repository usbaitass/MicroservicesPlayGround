import { Component, inject } from '@angular/core';
import { HttpClientService } from '../../networks/http-client.service';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-messages',
  imports: [CommonModule],
  templateUrl: './messages.html',
})
export class Messages {
  httpService = inject(HttpClientService);

  messages: Message[] = [];

  getMessages() {
    this.httpService.get(`/messages`).subscribe({
      next: (data) => {
        this.messages = data;
      },
    });
  }
}

export interface Message {
  id: number;
  content: string;
  status?: string;
  createAt?: string;
}
