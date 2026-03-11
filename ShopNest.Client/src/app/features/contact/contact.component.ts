import { Component, Inject } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { HttpClient } from '@angular/common/http';
import { ToastrService } from 'ngx-toastr';

@Component({
  selector: 'app-contact',
  templateUrl: './contact.component.html',
  styleUrls: ['./contact.component.scss']
})
export class ContactComponent {
  contactForm: FormGroup;
  isLoading = false;

  constructor(
    private fb: FormBuilder,
    private http: HttpClient,
    @Inject(ToastrService) private toastr: ToastrService
  ) {
    this.contactForm = this.fb.group({
      subject: ['', [Validators.required, Validators.minLength(5)]],
      message: ['', [Validators.required, Validators.minLength(20)]]
    });
  }

  onSubmit(): void {
    if (this.contactForm.invalid) return;

    this.isLoading = true;

    this.http.post('https://localhost:7271/api/contact', this.contactForm.value)
      .subscribe({
        next: () => {
          this.toastr.success('Message sent successfully! We will get back to you soon. 😊');
          this.contactForm.reset();
          this.isLoading = false;
        },
        error: () => {
          this.toastr.error('Failed to send message. Please try again.');
          this.isLoading = false;
        }
      });
  }

  isFieldInvalid(field: string): boolean {
    const control = this.contactForm.get(field);
    return !!(control && control.invalid && control.touched);
  }
}