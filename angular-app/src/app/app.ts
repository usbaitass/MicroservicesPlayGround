import { Component, signal } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { Messages } from './components/messages/messages';
import { SendMessage } from './components/send-message/send-message';

@Component({
  selector: 'app-root',
  imports: [RouterOutlet, Messages, SendMessage],
  templateUrl: './app.html',
  styleUrl: './app.css',
})
export class App {
  protected readonly title = signal('angular-app');
}
