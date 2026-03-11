import { Injectable } from '@angular/core';
import { BehaviorSubject } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class ThemeService {
  private darkMode = false;
  private darkModeSubject = new BehaviorSubject<boolean>(false);
  darkMode$ = this.darkModeSubject.asObservable();

  constructor() {
    // Check saved preference
    const saved = localStorage.getItem('darkMode');
    if (saved === 'true') {
      this.enableDarkMode();
    }
  }

  toggleTheme(): void {
    if (this.darkMode) {
      this.disableDarkMode();
    } else {
      this.enableDarkMode();
    }
  }

  private enableDarkMode(): void {
    document.body.classList.add('dark-mode');
    this.darkMode = true;
    this.darkModeSubject.next(true);
    localStorage.setItem('darkMode', 'true');
  }

  private disableDarkMode(): void {
    document.body.classList.remove('dark-mode');
    this.darkMode = false;
    this.darkModeSubject.next(false);
    localStorage.setItem('darkMode', 'false');
  }

  isDarkMode(): boolean {
    return this.darkMode;
  }
}