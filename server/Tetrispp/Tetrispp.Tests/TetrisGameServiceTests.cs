using System;
using Tetrispp.Models;
using Tetrispp.Services;

namespace Tetrispp.Tests
{
    public class TetrisGameServiceTests
    {
        private readonly TetrisGameService _gameService;

        public TetrisGameServiceTests()
        {
            _gameService = new TetrisGameService();
        }

        [Fact]
        public void CreateNewBlock_ReturnsValidTetromino()
        {
            // Act
            var block = _gameService.CreateNewBlock();
            
            // Assert
            Assert.NotNull(block);
            Assert.IsType<Tetromino>(block);
            Assert.True(Enum.IsDefined(typeof(TetrominoType), block.Type));
            Assert.Equal(0, block.Rotation);
            Assert.Equal(3, block.Position.X); // standardmässig in der Mitte des Grids platzieren
            Assert.Equal(0, block.Position.Y);
        }

        [Fact]
        public void InitializePlayerState_SetsCorrectInitialValues()
        {
            // Arrange
            const int userId = 42;

            // Act
            var playerState = _gameService.InitializePlayerState(userId);

            // Assert
            Assert.Equal(userId, playerState.UserId);
            Assert.NotNull(playerState.CurrentBlock);
            Assert.NotNull(playerState.NextBlock);
            Assert.Equal(0, playerState.Score);
            Assert.Equal(0, playerState.LinesCleared);
            Assert.False(playerState.IsGameOver);
            Assert.Null(playerState.CompletedLines);

            // prüfen, dass das grid nur mit 0en initialisiert ist
            for (int x = 0; x < 10; x++)
            {
                for (int y = 0; y < 20; y++)
                {
                    Assert.Equal(0, playerState.Grid[x, y]);
                }
            }
        }

        [Fact]
        public void MoveBlock_Left_ValidPosition_MovesBlockAndReturnsTrue()
        {
            // Arrange
            var playerState = _gameService.InitializePlayerState(1);
            var initialX = playerState.CurrentBlock.Position.X;
            var initialY = playerState.CurrentBlock.Position.Y;

            // Act
            var result = _gameService.MoveBlock(playerState, "LEFT");

            // Assert
            Assert.True(result);
            Assert.Equal(initialX - 1, playerState.CurrentBlock.Position.X);
            Assert.Equal(initialY, playerState.CurrentBlock.Position.Y); // Y muss gleich bleiben
        }

        [Fact]
        public void MoveBlock_Right_ValidPosition_MovesBlockAndReturnsTrue()
        {
            // Arrange
            var playerState = _gameService.InitializePlayerState(1);
            var initialX = playerState.CurrentBlock.Position.X;
            var initialY = playerState.CurrentBlock.Position.Y;

            // Act
            var result = _gameService.MoveBlock(playerState, "RIGHT");

            // Assert
            Assert.True(result);
            Assert.Equal(initialX + 1, playerState.CurrentBlock.Position.X);
            Assert.Equal(initialY, playerState.CurrentBlock.Position.Y); // Y muss gleich bleiben
        }

        [Fact]
        public void MoveBlock_Down_ValidPosition_MovesBlockAndReturnsTrue()
        {
            // Arrange
            var playerState = _gameService.InitializePlayerState(1);
            var initialX = playerState.CurrentBlock.Position.X;
            var initialY = playerState.CurrentBlock.Position.Y;

            // Act
            var result = _gameService.MoveBlock(playerState, "DOWN");

            // Assert
            Assert.True(result);
            Assert.Equal(initialX, playerState.CurrentBlock.Position.X); // X muss gleich bleiben
            Assert.Equal(initialY + 1, playerState.CurrentBlock.Position.Y);
        }

        [Fact]
        public void RotateBlock_ValidPosition_RotatesBlockAndReturnsTrue()
        {
            // Arrange
            var playerState = _gameService.InitializePlayerState(1);
            var initialRotation = playerState.CurrentBlock.Rotation;

            // Act
            var result = _gameService.RotateBlock(playerState);

            // Assert
            Assert.True(result);
            Assert.Equal((initialRotation + 1) % 4, playerState.CurrentBlock.Rotation);
        }

        [Fact]
        public void MoveBlock_OutOfBounds_ReturnsFalse()
        {
            // Arrange
            var playerState = _gameService.InitializePlayerState(1);
            
            // Manuell den Block an den linken Rand setzen
            playerState.CurrentBlock.Position = new Position(0, 5);
            
            // Act - versuchen, den Block nach links zu bewegen (ausserhalb des Spielfelds)
            var result = _gameService.MoveBlock(playerState, "LEFT");
            
            // Assert
            Assert.False(result);
            Assert.Equal(0, playerState.CurrentBlock.Position.X); // Position sollte unverändert bleiben
        }

        [Fact]
        public void MoveBlock_IntoOccupiedCell_ReturnsFalse()
        {
            // Arrange
            var playerState = _gameService.InitializePlayerState(1);
            
            playerState.Grid[2, 1] = 1;
            playerState.Grid[2, 2] = 1;
            
            // Block als O-Block definieren, der 2x2 Zellen belegt
            playerState.CurrentBlock.Position = new Position(3, 1);
            playerState.CurrentBlock.Type = TetrominoType.O;
            playerState.CurrentBlock.Rotation = 0;
            
            // Act - versuchen, den Block nach links zu bewegen (in besetzte Zellen)
            var result = _gameService.MoveBlock(playerState, "LEFT");
            
            // Assert
            Assert.False(result);
            Assert.Equal(3, playerState.CurrentBlock.Position.X); // Position sollte unverändert bleiben
        }

        [Fact]
        public void MoveBlock_Down_AtBottom_PlacesBlockAndGeneratesNewBlock()
        {
            // Arrange
            var playerState = _gameService.InitializePlayerState(1);
            var oldNextBlock = playerState.NextBlock;
            
            // Block ganz nach unten setzen
            playerState.CurrentBlock.Position = new Position(3, 19);
            playerState.CurrentBlock.Type = TetrominoType.O; // O-Block ist 2x2
            playerState.CurrentBlock.Rotation = 0;
            
            // Act - versuchen, den Block nach unten zu bewegen
            var result = _gameService.MoveBlock(playerState, "DOWN");
            
            // Assert
            Assert.True(result);
            Assert.Equal(oldNextBlock, playerState.CurrentBlock); // Neuer aktueller Block sollte der alte NextBlock sein
            Assert.NotNull(playerState.NextBlock); // Sollte einen neuen NextBlock generieren
            Assert.NotEqual(oldNextBlock, playerState.NextBlock); // Neuer NextBlock sollte nicht der alte NextBlock sein
        }

        [Fact]
        public void RotateBlock_InvalidRotation_ReturnsFalse()
        {
            // Arrange
            var playerState = _gameService.InitializePlayerState(1);
            
            // Null-Check hinzufügen
            Assert.NotNull(playerState.CurrentBlock);
            
            // Block näher am rechten Rand positionieren, wo ein T-Block nicht rotieren kann
            playerState.CurrentBlock.Position = new Position(8, 5);
            playerState.CurrentBlock.Type = TetrominoType.T;
            playerState.CurrentBlock.Rotation = 0;
            
            // Act - versuchen, den Block zu drehen
            var result = _gameService.RotateBlock(playerState);
            
            // Assert
            Assert.False(result);
            Assert.Equal(0, playerState.CurrentBlock.Rotation); // Rotation sollte unverändert bleiben
        }

        [Fact]
        public void AddCompletedLinesToPlayer_AddsLinesToBottom()
        {
            // Arrange
            var playerState = _gameService.InitializePlayerState(1);
            
            // Leeres Grid sicherstellen
            for (int x = 0; x < 10; x++)
                for (int y = 0; y < 20; y++)
                    playerState.Grid[x, y] = 0;
            
            // Erstellen einer vollständigen Testzeile
            var completedLine = new List<int[]> { 
                new int[10] { 1, 2, 3, 4, 5, 6, 7, 1, 2, 3 } 
            };
            
            // Act
            _gameService.AddCompletedLinesToPlayer(playerState, completedLine);
            
            // Assert
            // Die unterste Zeile sollte jetzt die hinzugefügte Zeile sein (mit negativen Werten)
            for (int x = 0; x < 10; x++)
            {
                Assert.Equal(-completedLine[0][x], playerState.Grid[x, 19]);
            }
            
            // Alle anderen Zeilen sollten immer noch leer sein
            for (int y = 0; y < 19; y++)
                for (int x = 0; x < 10; x++)
                    Assert.Equal(0, playerState.Grid[x, y]);
        }
    }
}