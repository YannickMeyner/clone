"use client";

import { useRouter } from 'next/navigation';
import { useEffect } from 'react';
import styles from "./page.module.css";
import Board from "./components/Board";
import { useAuth } from './context/auth-context';

export default function Home() {
  const { isAuthenticated, loading, user, logout } = useAuth();
  const router = useRouter();

  useEffect(() => {
    // Redirect to login if not authenticated
    if (!isAuthenticated && !loading) {
      router.push('/login');
    }
  }, [isAuthenticated, loading, router]);

  if (loading) {
    return (
      <div className={styles.page}>
        <main className={styles.main}>
          <h1>Loading...</h1>
        </main>
      </div>
    );
  }

  if (!isAuthenticated) {
    return null;
  }

  return (
    <div className={styles.page}>
      <header className={styles.header}>
        <div className={styles.userInfo}>
          Welcome, {user?.username}!
        </div>
        <button onClick={logout} className={styles.logoutButton}>
          Logout
        </button>
      </header>
      <main className={styles.main}>
        <h1>Tutris 👻🥳</h1>
        <Board />
      </main>
    </div>
  );
}
