export interface BoardCreateDto {
  Name: string;
  Description: string;
}

export interface BoardDto {
  Id: number;
  Name: string;
  Description: string;
  OwnerId: number;
  Owner: {
    Id: number;
    Email: string;
    Username: string;
    CreatedAt: string;
    UpdatedAt: string;
  };
  CreatedAt: string;
  UpdatedAt: string;
}
