import { AuthResponse, LoginRequest, RegisterRequest } from '../models/auth-types';

const API_BASE_URL = process.env.API_URL || "http://localhost:5001";

export async function login(credentials: LoginRequest): Promise<AuthResponse> {
  const response = await fetch(`${API_BASE_URL}/api/auth/login`, {
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

export async function register(credentials: RegisterRequest): Promise<AuthResponse> {
  const response = await fetch(`${API_BASE_URL}/api/auth/register`, {
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