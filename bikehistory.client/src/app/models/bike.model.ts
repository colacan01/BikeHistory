export interface BikeFrame {
  id: number;
  frameNumber: string;
  manufacturerId: number;
  manufacturerName?: string;
  brandId: number;
  brandName?: string;
  bikeTypeId: number;
  bikeTypeName?: string;
  model?: string;
  manufactureYear?: number;
  color?: string;
  currentOwnerId: string;
  currentOwnerName?: string;
  registeredDate: Date;
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
  previousOwnerName?: string;
  newOwnerId: string;
  newOwnerName?: string;
  transferDate: Date;
  notes?: string;
}
