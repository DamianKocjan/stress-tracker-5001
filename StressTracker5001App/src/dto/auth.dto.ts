export interface LoginDto {
  email: string;
  password: string;
}

export interface RegisterDto {
  username: string;
  email: string;
  password: string;
}

export interface RequestPasswordResetDto {
  email: string;
}

export interface ConfirmPasswordResetDto {
  token: string;
  newPassword: string;
  confirmPassword: string;
}

export interface RequestEmailChangeDto {
  newEmail: string;
  password: string;
}

export interface ConfirmEmailChangeDto {
  token: string;
}

export interface UpdatePasswordDto {
  currentPassword: string;
  newPassword: string;
  confirmPassword: string;
}

export interface DeleteAccountDto {
  password: string;
  confirmDeletion: boolean;
}

export interface ResendVerificationEmailDto {
  email: string;
}
