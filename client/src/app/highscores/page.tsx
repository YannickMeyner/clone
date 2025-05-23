"use client";

import { useRouter } from 'next/navigation';
import { useEffect, useState } from 'react';
import { useAuth } from '../context/auth-context';
import styles from './highscores.module.css';

interface Highscore {
  username: string;
  linesCleared: number;
  date: string;
}

export default function HighscoresPage() {
  const { isAuthenticated, loading, user, logout } = useAuth();
  const router = useRouter();
  const [highscores, setHighscores] = useState<Highscore[]>([]);
  const [loadingScores, setLoadingScores] = useState(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    // Redirect to login if not authenticated
    if (!isAuthenticated && !loading) {
      router.push('/login');
    }
  }, [isAuthenticated, loading, router]);

  useEffect(() => {
    if (isAuthenticated) {
      fetchHighscores();
    }
  }, [isAuthenticated]);

  const fetchHighscores = async () => {
    try {
      const API_BASE_URL = process.env.API_URL || "http://localhost:5001";
      const response = await fetch(`${API_BASE_URL}/api/score/highscores`);

      if (!response.ok) {
        throw new Error('Failed to fetch highscores');
      }

      const data = await response.json();
      setHighscores(data);
    } catch (err) {
      console.error('Error fetching highscores:', err);
    } finally {
      setLoadingScores(false);
    }
  };

  const formatDate = (dateString: string) => {
    const date = new Date(dateString);
    return date.toLocaleDateString();
  };

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
          <button onClick={() => router.push('/')} className={styles.navButton}>
            Play Game
          </button>
          <button onClick={() => router.push('/active-games')} className={styles.navButton}>
            Active Games
          </button>
          <button onClick={logout} className={styles.logoutButton}>
            Logout
          </button>
        </div>
      </header>
      <main className={styles.main}>
        <h1 className={styles.title}>Highscores 🏆</h1>

        {loadingScores ? (
          <p>Loading highscores...</p>
        ) : error ? (
          <p className={styles.error}>{error}</p>
        ) : (
          <div className={styles.highscoresContainer}>
            <table className={styles.highscoresTable}>
              <thead>
                <tr>
                  <th className={styles.rankColumn}>#</th>
                  <th>Player</th>
                  <th>Lines Cleared</th>
                  <th>Date</th>
                </tr>
              </thead>
              <tbody>
                {highscores.length === 0 ? (
                  <tr>
                    <td colSpan={4} className={styles.noScores}>No highscores yet!</td>
                  </tr>
                ) : (
                  highscores.map((score, index) => (
                    <tr key={index} className={user?.username === score.username ? styles.currentUser : ''}>
                      <td className={styles.rankColumn}>
                        {index === 0 ? '🥇' : index === 1 ? '🥈' : index === 2 ? '🥉' : index + 1}
                      </td>
                      <td>{score.username}</td>
                      <td>{score.linesCleared}</td>
                      <td>{formatDate(score.date)}</td>
                    </tr>
                  ))
                )}
              </tbody>
            </table>
          </div>
        )}
      </main>
    </div>
  );
}