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
        <h1>Tutris 👻🥳</h1>
        <div className={styles.userInfo}>
          Welcome, {user?.username}!
        </div>
        <div className={styles.navButtons}>
          <button onClick={() => router.push('/active-games')} className={styles.navButton}>
            Active Games
          </button>
          <button onClick={() => router.push('/highscores')} className={styles.navButton}>
            Highscores
          </button>
          <button onClick={logout} className={styles.logoutButton}>
            Logout
          </button>
        </div>
      </header>
      <main className={styles.main}>
        <Board />
      </main>
    </div>
  );
}