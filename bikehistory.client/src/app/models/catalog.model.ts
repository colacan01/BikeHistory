export interface Manufacturer {
  id: number;
  name: string;
  description?: string;
  countryOfOrigin?: string;
  website?: string;
}

export interface Brand {
  id: number;
  name: string;
  description?: string;
  manufacturerId?: number;
  manufacturer?: Manufacturer;
}

export interface BikeType {
  id: number;
  name: string;
  description?: string;
}
