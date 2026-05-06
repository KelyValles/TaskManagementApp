export interface User {
  id: number;
  name: string;
  email: string;
}

export interface CreateUserPayload {
  name: string;
  email: string;
}
