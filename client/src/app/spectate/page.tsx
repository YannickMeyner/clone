"use client";

import { useEffect, useState } from 'react';
import { useRouter, useSearchParams } from 'next/navigation';
import { useAuth } from '../context/auth-context';
import styles from '../components/board.module.css';
import { Pixel } from '../components/Pixel';

export default function SpectatePage() {
  const { isAuthenticated, loading, token } = useAuth();
  const router = useRouter();
  // Holt die roomId aus den URL-Parametern
  const roomId = useSearchParams().get('roomId');
  
  const [player1Grid, setPlayer1Grid] = useState<Pixel[]>([]);
  const [player2Grid, setPlayer2Grid] = useState<Pixel[]>([]);
  const [player1Info, setPlayer1Info] = useState({ username: '', linesCleared: 0, userId: 0 });
  const [player2Info, setPlayer2Info] = useState({ username: '', linesCleared: 0, userId: 0 });
  const [gameEnded, setGameEnded] = useState(false);
  const [winnerId, setWinnerId] = useState<number | null>(null);

  useEffect(() => {
    if (!isAuthenticated && !loading) {
      router.push('/login');
      return;
    }

    if (!roomId || !token) return;

    const ws = new WebSocket(`${process.env.NEXT_PUBLIC_WS_BASE_URL ?? 'ws://localhost:5001'}?token=${token}&spectate=true&roomId=${roomId}`);

    ws.onclose = (event) => {
      console.log('WebSocket closed:', event.code, event.reason);
    
      // Room existiert nicht
      if (event.code === 1007) {  // InvalidPayloadData
        alert("This game room no longer exists!");
        router.push('/active-games');
      }
    };

    ws.onmessage = (event) => {
      const message = JSON.parse(event.data);
      
      if (message.action === 'SPECTATE_UPDATE' && message.gameState?.players?.length >= 2) {
        const players = message.gameState.players;
        
        // Update player info and grids in one go
        setPlayer1Info({
          username: players[0].username,
          linesCleared: players[0].linesCleared,
          userId: players[0].userId
        });
        
        setPlayer2Info({
          username: players[1].username,
          linesCleared: players[1].linesCleared,
          userId: players[1].userId
        });
        
        if (players[0].grid) setPlayer1Grid(updatePixels(players[0].grid));
        if (players[1].grid) setPlayer2Grid(updatePixels(players[1].grid));

        // Check if game has ended
        if (!message.gameState.isGameActive) setGameEnded(true);
      } else if (message.action === 'GAME_OVER') {
        setGameEnded(true);
        setWinnerId(message.winnerId);
      }
    };

    return () => ws.close();
  }, [roomId, isAuthenticated, token, router, loading]);

  function updatePixels(grid: number[][]) {
    return grid.flat().map(value => new Pixel(value, {}));
  }

  if (loading || !isAuthenticated) {
    return <div className={styles.page}><main><h1>Loading...</h1></main></div>;
  }

  return (
    <div className={styles.spectateContainer}>
      <h1 className={styles.spectateTitle}>Spectating Game</h1>
      
      <div className={styles.boardsContainer}>
        {[
          { grid: player1Grid, info: player1Info },
          { grid: player2Grid, info: player2Info }
        ].map((player, index) => (
          <div key={index} className={styles.boardWrapper}>
            <div className={styles.playerHeader}>
              <span className={styles.playerName}>{player.info.username}</span>
              <span className={styles.linesInfo}>Lines cleared: {player.info.linesCleared}</span>
            </div>
            <div className={styles.board}>
              {player.grid.length > 0 && (
                <div className={styles.gridContainer}>
                  {player.grid.map((pixel, i) => pixel.render(i))}
                </div>
              )}
            </div>
          </div>
        ))}
      </div>
      
      {gameEnded && (
        <div className={styles.gameEndedMessage}>
          Game Ended
          {winnerId !== null && (
            <div className={styles.winnerInfo}>
              Winner: {winnerId === player1Info.userId ? player1Info.username : player2Info.username}
            </div>
          )}
        </div>
      )}
      
      <button 
        onClick={() => router.push('/active-games')} 
        className={styles.backButton}
      >
        Back to Active Games
      </button>
    </div>
  );
}