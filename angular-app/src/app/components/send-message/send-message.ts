import { Component, inject } from '@angular/core';
import { HttpClientService } from '../../networks/http-client.service';
import { FormsModule } from '@angular/forms';

@Component({
  selector: 'app-send-message',
  imports: [FormsModule],
  templateUrl: './send-message.html',
})
export class SendMessage {
  http = inject(HttpClientService);

  messageText: string = '';

  public sendMessage() {
    const message = {
      content: this.messageText,
    };
    this.http.post(`/messages`, message).subscribe();
  }
}
