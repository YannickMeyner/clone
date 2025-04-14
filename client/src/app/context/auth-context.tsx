"use client";

import { createContext, useContext, useEffect, useState } from 'react';
import { login, register } from '../api/auth-api';
import { AuthState, LoginRequest, RegisterRequest, User } from '../models/auth-types';

interface AuthContextType extends AuthState {
  login: (credentials: LoginRequest) => Promise<void>;
  register: (credentials: RegisterRequest) => Promise<void>;
  logout: () => void;
}

const defaultAuthState: AuthState = {
  user: null,
  token: null,
  isAuthenticated: false,
  loading: true,
  error: null,
};

const AuthContext = createContext<AuthContextType>({
  ...defaultAuthState,
  login: async () => {},
  register: async () => {},
  logout: () => {},
});

export const useAuth = () => useContext(AuthContext);

export const AuthProvider = ({ children }: { children: React.ReactNode }) => {
  const [state, setState] = useState<AuthState>(defaultAuthState);

  useEffect(() => {
    const token = localStorage.getItem('auth_token');
    const userJson = localStorage.getItem('auth_user');
    
    if (token && userJson) {
      try {
        const user = JSON.parse(userJson) as User;
        setState({
          user,
          token,
          isAuthenticated: true,
          loading: false,
          error: null,
        });
      } catch (error) {
        localStorage.removeItem('auth_token');
        localStorage.removeItem('auth_user');
        setState({
          ...defaultAuthState,
          loading: false,
        });
      }
    } else {
      setState({
        ...defaultAuthState,
        loading: false,
      });
    }
  }, []);

  const loginHandler = async (credentials: LoginRequest) => {
    setState(prev => ({ ...prev, loading: true, error: null }));
    
    try {
      const response = await login(credentials);
      
      if (response.success && response.token && response.userId && response.username) {
        const user: User = {
          id: response.userId,
          username: response.username
        };
        
        localStorage.setItem('auth_token', response.token);
        localStorage.setItem('auth_user', JSON.stringify(user));
        
        setState({
          user,
          token: response.token,
          isAuthenticated: true,
          loading: false,
          error: null,
        });
      } else {
        throw new Error(response.message || 'Login failed');
      }
    } catch (error) {
      setState(prev => ({
        ...prev,
        loading: false,
        error: error instanceof Error ? error.message : 'An unknown error occurred',
      }));
    }
  };

  const registerHandler = async (credentials: RegisterRequest) => {
    setState(prev => ({ ...prev, loading: true, error: null }));
    
    try {
      const response = await register(credentials);
      
      if (response.success && response.token && response.userId && response.username) {
        const user: User = {
          id: response.userId,
          username: response.username
        };
        
        localStorage.setItem('auth_token', response.token);
        localStorage.setItem('auth_user', JSON.stringify(user));
        
        setState({
          user,
          token: response.token,
          isAuthenticated: true,
          loading: false,
          error: null,
        });
      } else {
        throw new Error(response.message || 'Registration failed');
      }
    } catch (error) {
      setState(prev => ({
        ...prev,
        loading: false,
        error: error instanceof Error ? error.message : 'An unknown error occurred',
      }));
    }
  };

  const logoutHandler = () => {
    localStorage.removeItem('auth_token');
    localStorage.removeItem('auth_user');
    
    setState({
      user: null,
      token: null,
      isAuthenticated: false,
      loading: false,
      error: null,
    });
  };

  return (
    <AuthContext.Provider
      value={{
        ...state,
        login: loginHandler,
        register: registerHandler,
        logout: logoutHandler,
      }}
    >
      {children}
    </AuthContext.Provider>
  );
};