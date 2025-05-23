export interface GamePlayer {
  userId: number;
  username: string;
  linesCleared: number;
}

export interface ActiveGame {
  roomId: string;
  players: GamePlayer[];
}

export async function getActiveGames(baseUrl: string): Promise<ActiveGame[]> {
  const response = await fetch(`${baseUrl}/api/game/active`);

  if (!response.ok) {
    let errorMessage = 'Failed to fetch active games';
    try {
      const error = await response.json();
      errorMessage = error.message || errorMessage;
    } catch (e) {
      console.error('Error fetching active games:', e);
    }
    throw new Error(errorMessage);
  }

  return response.json();
}