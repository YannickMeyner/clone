"use client";

import { useState } from 'react';
import LoginForm from './login-form';
import RegisterForm from './register-form';
import styles from './auth.module.css';

export default function Auth() {
  const [showLogin, setShowLogin] = useState(true);

  return (
    <div className={styles.authContainer}>
      {showLogin ? (
        <LoginForm onSwitchToRegister={() => setShowLogin(false)} />
      ) : (
        <RegisterForm onSwitchToLogin={() => setShowLogin(true)} />
      )}
    </div>
  );
}