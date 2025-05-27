import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { BikeFrame } from '../../models/bike.model';
import { BikeService } from '../../services/bike.service';

@Component({
  selector: 'app-bike-transfer',
  templateUrl: './bike-transfer.component.html',
  styles: []
})
export class BikeTransferComponent implements OnInit {
  bikeId!: number;
  bike: BikeFrame | null = null;
  transferForm!: FormGroup;
  loading = false;
  transferring = false;
  submitted = false;
  error = '';

  constructor(
    private formBuilder: FormBuilder,
    private route: ActivatedRoute,
    private router: Router,
    private bikeService: BikeService
  ) { }

  ngOnInit(): void {
    this.route.params.subscribe(params => {
      this.bikeId = +params['id'];
      this.loadBikeDetails();
    });

    this.transferForm = this.formBuilder.group({
      newOwnerId: ['', [Validators.required, Validators.email]],
      notes: ['']
    });
  }

  // Convenience getter for easy access to form fields
  get f() { return this.transferForm.controls; }

  loadBikeDetails(): void {
    this.loading = true;
    this.bikeService.getBikeFrameById(this.bikeId)
      .subscribe({
        next: bike => {
          this.bike = bike;
          this.loading = false;
        },
        error: error => {
          this.error = 'Failed to load bike details. Please try again later.';
          this.loading = false;
          console.error('Error loading bike details:', error);
        }
      });
  }

  onSubmit(): void {
    this.submitted = true;

    // Stop here if form is invalid
    if (this.transferForm.invalid) {
      return;
    }

    this.transferring = true;
    this.bikeService.transferOwnership(this.bikeId, {
      newOwnerId: this.f['newOwnerId'].value,
      notes: this.f['notes'].value
    })
      .subscribe({
        next: () => {
          this.router.navigate(['/bikes'], { 
            queryParams: { transferred: true } 
          });
        },
        error: error => {
          this.error = error.error?.message || 'Failed to transfer ownership. Please try again.';
          this.transferring = false;
        }
      });
  }
  
  // Method to navigate back to bike details
  goBack(): void {
    this.router.navigate(['/bikes', this.bikeId]);
  }
}
