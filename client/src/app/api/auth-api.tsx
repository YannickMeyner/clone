import { AuthResponse, LoginRequest, RegisterRequest } from '../models/auth-types';

export async function login(baseUrl: string, credentials: LoginRequest): Promise<AuthResponse> {
  const response = await fetch(`${baseUrl}/api/auth/login`, {
    method: 'POST',
    headers: {
      'Content-Type': 'application/json',
    },
    body: JSON.stringify(credentials)
  });

  if (!response.ok) {
    let errorMessage = 'Failed to login';
    try {
      const error = await response.json();
      errorMessage = error.message || errorMessage;
    } catch (e) {
      console.error('Login error:', e);
    }
    throw new Error(errorMessage);
  }

  return response.json();
}

export async function register(baseUrl: string, credentials: RegisterRequest): Promise<AuthResponse> {
  const response = await fetch(`${baseUrl}/api/auth/register`, {
    method: 'POST',
    headers: {
      'Content-Type': 'application/json',
    },
    body: JSON.stringify(credentials)
  });

  if (!response.ok) {
    let errorMessage = 'Failed to register';
    try {
      const error = await response.json();
      errorMessage = error.message || errorMessage;
    } catch (e) {
      console.error('Registration error:', e);
    }
    throw new Error(errorMessage);
  }

  return response.json();
}