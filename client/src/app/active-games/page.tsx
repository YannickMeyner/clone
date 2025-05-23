"use client";

import { useRouter } from 'next/navigation';
import { useEffect, useState } from 'react';
import { useAuth } from '../context/auth-context';
import styles from '../highscores/highscores.module.css';
import { ActiveGame, getActiveGames } from '../api/game-api';

export default function ActiveGamesPage() {
  const { isAuthenticated, loading, user, logout } = useAuth();
  const router = useRouter();
  const [activeGames, setActiveGames] = useState<ActiveGame[]>([]);
  const [loadingGames, setLoadingGames] = useState(false);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    if (!isAuthenticated && !loading) {
      router.push('/login');
    } else if (isAuthenticated) {
      fetchActiveGames();
    }
  }, [isAuthenticated, loading, router]);

  const fetchActiveGames = async () => {
    try {
      setLoadingGames(true);
      const apiBaseUrl = process.env.NEXT_PUBLIC_API_BASE_URL ?? 'http://localhost:5001';
      const games = await getActiveGames(apiBaseUrl);
      setActiveGames(games);
      setError(null);
    } catch (err) {
      console.error('Error fetching active games:', err);
      setError('Failed to fetch active games');
    } finally {
      setLoadingGames(false);
    }
  };

  if (loading || !isAuthenticated) {
    return loading ? (
      <div className={styles.page}>
        <main className={styles.main}>
          <h1>Loading...</h1>
        </main>
      </div>
    ) : null;
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
          <button onClick={() => router.push('/highscores')} className={styles.navButton}>
            Highscores
          </button>
          <button onClick={logout} className={styles.logoutButton}>
            Logout
          </button>
        </div>
      </header>
      <main className={styles.main}>
        <h1 className={styles.title}>Active Games 🎮</h1>

        <button 
          onClick={fetchActiveGames} 
          className={styles.navButton}
          style={{ marginBottom: '20px' }}
          disabled={loadingGames}
        >
          {loadingGames ? 'Refreshing...' : 'Refresh Games'}
        </button>

        {error && <p className={styles.error}>{error}</p>}

        <div className={styles.highscoresContainer}>
          <table className={styles.highscoresTable}>
            <thead>
              <tr>
                <th>Player 1</th>
                <th>Lines Cleared</th>
                <th>Player 2</th>
                <th>Lines Cleared</th>
                <th>Actions</th>
              </tr>
            </thead>
            <tbody>
              {activeGames.length === 0 ? (
                <tr>
                  <td colSpan={4} className={styles.noScores}>No active games at the moment</td>
                </tr>
              ) : (
                activeGames.map((game) => (
                  <tr key={game.roomId}>
                    <td>{game.players[0].username}</td>
                    <td>{game.players[0].linesCleared}</td>
                    <td>{game.players[1].username}</td>
                    <td>{game.players[1].linesCleared}</td>
                    <td>
                      <button 
                        onClick={() => router.push(`/spectate?roomId=${game.roomId}`)} 
                        className={styles.navButton}
                        disabled={game.players.length < 2}
                      >
                        Spectate
                      </button>
                    </td>
                  </tr>
                ))
              )}
            </tbody>
          </table>
        </div>
      </main>
    </div>
  );
}