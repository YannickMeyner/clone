export interface LoginRequest {
    username: string;
    password: string;
}
  
  export interface RegisterRequest {
    username: string;
    password: string;
}
  
  export interface AuthResponse {
    success: boolean;
    message?: string;
    token?: string;
    userId?: number;
    username?: string;
}
  
  export interface User {
    id: number;
    username: string;
}
  
  export interface AuthState {
    user: User | null;
    token: string | null;
    isAuthenticated: boolean;
    loading: boolean;
    error: string | null;
}