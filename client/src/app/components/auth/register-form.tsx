"use client";

import { useState } from 'react';
import { useAuth } from '../../context/auth-context';
import styles from './auth.module.css';

interface RegisterFormProps {
  onSwitchToLogin: () => void;
}

export default function RegisterForm({ onSwitchToLogin }: RegisterFormProps) {
  const [username, setUsername] = useState('');
  const [password, setPassword] = useState('');
  const [confirmPassword, setConfirmPassword] = useState('');
  const [formError, setFormError] = useState<string | null>(null);
  const { register, error, loading } = useAuth();

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setFormError(null);
    
    if (password !== confirmPassword) {
      setFormError("Passwords don't match");
      return;
    }
    
    if (password.length < 6) {
      setFormError("Password must be at least 6 characters long");
      return;
    }
    
    await register({ username, password });
  };

  return (
    <div className={styles.authForm}>
      <h2>Register</h2>
      {(error || formError) && <div className={styles.error}>{formError || error}</div>}
      <form onSubmit={handleSubmit}>
        <div className={styles.formGroup}>
          <label htmlFor="username">Username</label>
          <input
            type="text"
            id="username"
            value={username}
            onChange={(e) => setUsername(e.target.value)}
            required
          />
        </div>
        <div className={styles.formGroup}>
          <label htmlFor="password">Password</label>
          <input
            type="password"
            id="password"
            value={password}
            onChange={(e) => setPassword(e.target.value)}
            required
          />
        </div>
        <div className={styles.formGroup}>
          <label htmlFor="confirmPassword">Confirm Password</label>
          <input
            type="password"
            id="confirmPassword"
            value={confirmPassword}
            onChange={(e) => setConfirmPassword(e.target.value)}
            required
          />
        </div>
        <button type="submit" disabled={loading} className={styles.submitButton}>
          {loading ? 'Registering...' : 'Register'}
        </button>
      </form>
      <p className={styles.switchText}>
        Already have an account?{' '}
        <button onClick={onSwitchToLogin} className={styles.switchButton}>
          Login
        </button>
      </p>
    </div>
  );
}