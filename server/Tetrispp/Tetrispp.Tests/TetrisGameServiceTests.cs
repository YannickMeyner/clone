using System;
using Tetrispp.Models;
using Tetrispp.Services;
using Tetrispp.Tetris.Randomizer;

namespace Tetrispp.Tests
{
    public class TetrisGameServiceTests
    {
        private readonly TetrisGameService _gameService;

        public TetrisGameServiceTests()
        {
            _gameService = new TetrisGameService(new PickOneRandomizer());
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
            playerState.NextBlock = new Tetromino(TetrominoType.I);

            // Block ganz nach unten setzen
            playerState.CurrentBlock.Position = new Position(3, 18);
            playerState.CurrentBlock.Type = TetrominoType.O;
            playerState.CurrentBlock.Rotation = 0;

            // Act - versuchen, den Block nach unten zu bewegen
            var result = _gameService.MoveBlock(playerState, "DOWN");

            // Assert
            Assert.True(result);
            Assert.Equal(TetrominoType.O, playerState.CurrentBlock.Type);
            Assert.NotNull(playerState.NextBlock);
            Assert.NotEqual(playerState.CurrentBlock.Type, playerState.NextBlock.Type);
        }

        [Fact]
        public void RotateBlock_WithWallKick_SuccessfullyRotatesBlock()
        {
            // Arrange
            var playerState = _gameService.InitializePlayerState(1);
            Assert.NotNull(playerState.CurrentBlock);

            // Einen T-Block direkt an der rechten Wand positionieren
            // Eine normale Rotation wäre hier nicht möglich, aber Wall-Kicks sollten funktionieren
            playerState.CurrentBlock.Position = new Position(8, 5);
            playerState.CurrentBlock.Type = TetrominoType.T;
            playerState.CurrentBlock.Rotation = 0;

            var originalPosition = new Position(
                playerState.CurrentBlock.Position.X,
                playerState.CurrentBlock.Position.Y
            );

            // Act -> Versuch, den Block zu drehen
            var result = _gameService.RotateBlock(playerState);

            // Assert
            Assert.True(result); // Die Rotation sollte erfolgreich sein dank Wall-Kicks
            Assert.Equal(1, playerState.CurrentBlock.Rotation); // Die Rotation sollte von 0 auf 1 geändert worden sein
            Assert.NotEqual(originalPosition.X, playerState.CurrentBlock.Position.X); // Bei einem T-Block an der Wand (X=8) wird erwartet, dass er nach links verschoben wird
            Assert.Equal(originalPosition.X - 1, playerState.CurrentBlock.Position.X); // Gemäss SRS-Regeln sollte der erste Test (-1, 0) erfolgreich sein
            Assert.Equal(originalPosition.Y, playerState.CurrentBlock.Position.Y);
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