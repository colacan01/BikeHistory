import { BikeFrame } from "./bike.model";
import { User } from "./auth.model";

export enum ServiceType {
  Maintenance = 0,
  Repair = 1,
  Inspection = 2,
  Upgrade = 3,
  Custom = 4
}

export enum ServiceStatus {
  Scheduled = 0,
  InProgress = 1,
  Completed = 2,
  Canceled = 3
}

export interface BikeServiceRecord {
  id: number;
  serviceDate: Date;
  bikeFrameId: number;
  bikeFrame?: BikeFrame;
  serviceType: ServiceType;
  serviceDetails: string;
  serviceShopId: string;
  serviceShop?: User;
  cost?: number;
  partDetails?: string;
  warrantyInfo?: string;
  serviceStatus: ServiceStatus;
  nextServiceDate?: Date;
  createdDate: Date;
  modifiedDate?: Date;
}
