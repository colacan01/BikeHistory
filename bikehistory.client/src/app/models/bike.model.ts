//export interface BikeFrame {
//  id: number;
//  frameNumber: string;
//  manufacturerId: number;
//  manufacturerName?: string;
//  brandId: number;
//  brandName?: string;
//  bikeTypeId: number;
//  bikeTypeName?: string;
//  model?: string;
//  manufactureYear?: number;
//  color?: string;
//  currentOwnerId: string;
//  currentOwnerName?: string;
//  registeredDate: Date;
//}

import { Manufacturer, Brand, BikeType } from "./catalog.model";
import { User } from "./auth.model";

export interface BikeFrame {
  id: number;
  frameNumber: string;
  manufacturer?: Manufacturer;
  brand?: Brand;
  bikeType?: BikeType;
  model?: string;
  manufactureYear?: number;
  color?: string;
  currentOwnerId: string;
  currentOwner?: User;
  registeredDate: Date;
  images?: BikeImage[];
  primaryImage?: BikeImage;
}

export interface BikeImage {
  id: number;
  bikeFrameId: number;
  fileName: string;
  originalFileName: string;
  filePath: string;
  contentType: string;
  fileSize: number;
  isPrimary: boolean;
  uploadedDate: Date;
  uploadedBy: string;
  description?: string;
  isDeleted: boolean;
  deletedDate?: Date;
}

export interface ImageUploadResult {
  uploadedImages: BikeImage[];
  errors: string[];
  totalUploaded: number;
  totalErrors: number;
}

export interface BikeFrameRegisterRequest {
  frameNumber: string;
  manufacturerId: number;
  brandId: number;
  bikeTypeId: number;
  model?: string;
  manufactureYear?: number;
  color?: string;
}

export interface BikeFrameUpdateRequest {
  manufacturerId: number;
  brandId: number;
  bikeTypeId: number;
  model?: string;
  manufactureYear?: number;
  color?: string;
}

export interface OwnershipTransferRequest {
  newOwnerId: string;
  notes?: string;
}

export interface OwnershipRecord {
  id: number;
  bikeFrameId: number;
  previousOwnerId: string;
  previousOwnerName?: User;
  newOwnerId: string;
  newOwnerName?: User;
  transferDate: Date;
  notes?: string;
}
