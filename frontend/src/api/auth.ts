import { apiFetch } from './base';

export interface LoginRequest {
  correo: string;
  contrasena: string;
}

export interface LoginResponse {
  exito: boolean;
  mensaje: string;
  token: string;
}

export async function login(data: LoginRequest): Promise<LoginResponse> {
  return apiFetch<LoginResponse>('auth/login', {
    method: 'POST',
    body: JSON.stringify(data),
  });
}
