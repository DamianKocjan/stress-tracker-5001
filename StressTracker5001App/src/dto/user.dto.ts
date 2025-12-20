export interface UserDto {
  id: number;
  username: string;
  createdAt: string;
  updatedAt: string;
}

export interface UserUpdateDto {
  username: string;
}
