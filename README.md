# Staircases

Game made using C#. WIP - Unfinished.

A challenging procedurally generated platformer built with C# and MonoGame, featuring dynamic difficulty scaling and persistent leaderboards.

## Features

- Procedurally generated endless staircase climbing
- Physics-based movement system
- Particle effects and sound system
- SQL Server integration for leaderboards
- Multiple stair types with unique behaviors
- Dynamic difficulty adjustment
- Spatial partitioning for efficient collision detection

## Technical Stack

- C# / .NET 6.0
- MonoGame 3.8
- SQL Server
- Entity Framework Core
- xUnit for testing

## Architecture

```
StaircaseGame/
├── src/
│   ├── Core/
│   │   ├── Game.cs
│   │   ├── Player.cs
│   │   └── Stairs/
│   ├── Graphics/
│   │   ├── ParticleSystem.cs
│   │   └── TextureManager.cs
│   ├── Audio/
│   │   └── SoundManager.cs
│   └── Data/
│       └── ScoreManager.cs
├── tests/
└── content/
```

## Setup

1. Prerequisites:
   ```bash
   dotnet tool install -g dotnet-ef
   ```

2. Database:
   ```bash
   dotnet ef database update
   ```

3. Build:
   ```bash
   dotnet build
   ```

4. Run:
   ```bash
   dotnet run
   ```

## Controls

- SPACE: Jump
- LEFT/RIGHT: Move
- ESC: Pause

## Development

### Running Tests
```bash
dotnet test
```

### Building Release
```bash
dotnet publish -c Release
```

## License

MIT License

## Contributing

1. Fork repository
2. Create feature branch
3. Submit pull request

## License

MIT License
