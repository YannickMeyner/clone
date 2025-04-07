# Workshop Web

Semester: FS25

Group Number: 3

Students:
- Yannick Meyner (yannick.meyner@students.fhnw.ch)
- Leon Lüthi (leon.luethi@students.fhnw.ch)
- Janick Lehmann (janick.lehmann@students.fhnw.ch)
  
## Project Idea

### Tetris - Tug of War
In this version of Tetris, two players compete against each other.
The game is played on two separate boards, each controlled by one player.
Can a player complete a line, the line is added to the other player's board and cannot be removed.
The game ends when one player reaches the top of their board.

## Notes

xxx

## Deploy
[Create a new release on GitHub](https://docs.github.com/en/repositories/releasing-projects-on-github/managing-releases-in-a-repository#creating-a-release) to trigger the GitHub Actions workflow.

The Docker image is then published on [Docker Hub](https://hub.docker.com/r/janicklehmann/fhnw-woweb-fs25-group3).

To run the image, execute the following commands:
```bash
docker pull janicklehmann/fhnw-woweb-fs25-group3:latest
docker run -d -p $PORT:80 janicklehmann/fhnw-woweb-fs25-group3
```

## Frontend

### Getting Started

First, run the development server:

```bash
npm run dev
# or
yarn dev
```

Open [http://localhost:3000](http://localhost:3000) with your browser to see the result.

You can start editing the page by modifying `app/page.tsx`. The page auto-updates as you edit the file.

This project uses [`next/font`](https://nextjs.org/docs/app/building-your-application/optimizing/fonts) to automatically optimize and load [Geist](https://vercel.com/font), a new font family for Vercel.

### Learn More

To learn more about Next.js, take a look at the following resources:

- [Next.js Documentation](https://nextjs.org/docs) - learn about Next.js features and API.
- [Learn Next.js](https://nextjs.org/learn) - an interactive Next.js tutorial.

You can check out [the Next.js GitHub repository](https://github.com/vercel/next.js) - your feedback and contributions are welcome!

## Backend

### Setup & Start
1. Abhängigkeiten installieren: `dotnet restore`
2. Projekt starten: `dotnet run`

Der Server läuft standardmässig unter:
- HTTP: http://localhost:5001
- HTTPS: https://localhost:7218

### WebSocket-Handling
- Spieler werden automatisch einem Spielraum zugewiesen.
- Nachrichten werden zwischen Spielern eines Raums weitergeleitet.
- Spielertrennungen werden erkannt und behandelt.

### Architektur
- `Program.cs`: Initialisiert den Webserver.
- `Startup.cs`: Konfiguriert WebSockets und den `GameConnectionManager`.
- `GameConnectionManager`: Verwaltet WebSocket-Verbindungen und Spiellogik.
- `GameRoom.cs`: Organisiert Spieler in Spielräume.
- `Player.cs`: Repräsentiert einen Spieler.
- `GameAction.cs`: Modelliert Spieleraktionen.