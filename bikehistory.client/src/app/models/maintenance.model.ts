import { BikeFrame } from "./bike.model";
import { User } from "./auth.model";

export enum MaintenanceType {
  Maintenance = 'Maintenance', // 정비
  Repair = 'Repair',           // 수리
  Custom = 'Custom',           // 커스텀
  Self = 'Self'                // self
}

export enum PaymentMethod {
  Cash = 'Cash',               // 현금
  LocalCurrency = 'LocalCurrency', // 지역화폐
  CreditCard = 'CreditCard',   // 신용카드
  BankTransfer = 'BankTransfer', // 계좌이체
  Other = 'Other'              // 기타
}

export interface MaintenanceDetail {
  maintenanceId: string;
  seq: number;
  laborCost: number;
  partPrice: number;
  partName?: string;
  partSpecification?: string;
}

export interface Maintenance {
  id: string;
  maintenanceDate: Date;
  maintenanceType: MaintenanceType;
  storeId: string;
  store: User;
  ownerId: string;
  owner: User;
  bikeFrameId: number;
  bikeFrame: BikeFrame;
  totalAmount: number;
  paymentMethod: PaymentMethod;
  notes?: string;
  maintenanceDetails: MaintenanceDetail[];
}

export interface MaintenanceDetailDto {
  laborCost: number;
  partPrice: number;
  partName?: string;
  partSpecification?: string;
}

export interface MaintenanceCreateDto {
  maintenanceDate: Date;
  maintenanceType: MaintenanceType;
  storeId: string;
  ownerId: string;
  bikeFrameId: number;
  paymentMethod: PaymentMethod;
  notes?: string;
  details: MaintenanceDetailDto[];
}

export interface MaintenanceUpdateDto {
  id: string;
  maintenanceDate: Date;
  maintenanceType: MaintenanceType;
  paymentMethod: PaymentMethod;
  notes?: string;
  details: MaintenanceDetailDto[];
}
