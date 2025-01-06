using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;
using System.Data.SqlClient;

namespace Staircases
{
    /// <summary>
    /// Main game class handling core game loop and state management
    /// </summary>
    public class StaircaseGame : Game 
    {
        private readonly GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;
        private Player _player;
        private List<Stair> _stairs;
        private readonly ScoreManager _scoreManager;
        private ParticleSystem _particleSystem;
        private SoundManager _soundManager;
        
        // Game state
        private float _difficulty = 1.0f;
        private GameState _currentState;
        private float _timeSinceLastStair;
        private const float STAIR_GENERATION_INTERVAL = 2.0f;
        
        public StaircaseGame()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            _scoreManager = new ScoreManager(
                ConfigurationManager.ConnectionStrings["GameDB"].ConnectionString
            );
            
            // Configure graphics
            _graphics.PreferredBackBufferWidth = 1280;
            _graphics.PreferredBackBufferHeight = 720;
            _graphics.ApplyChanges();
        }

        protected override void Initialize()
        {
            // Initialize game components
            _player = new Player(new Vector2(100, 400));
            _stairs = new List<Stair>();
            _particleSystem = new ParticleSystem(GraphicsDevice);
            _soundManager = new SoundManager(Content);
            _currentState = GameState.Playing;
            
            base.Initialize();
        }

        /// <summary>
        /// Generates procedural stairs with dynamic difficulty scaling
        /// </summary>
        private void GenerateStairs()
        {
            var rand = new Random();
            float lastX = _player.Position.X + GraphicsDevice.Viewport.Width * 0.5f;
            float lastY = _player.Position.Y;

            // Dynamic difficulty adjustment
            float gapMultiplier = 1.0f + (_player.Score / 1000.0f);
            float heightVariance = Math.Min(50 * (_player.Score / 500.0f), 100);

            Stair newStair = new Stair(
                new Vector2(lastX, lastY),
                TextureManager.GetTexture("stair"),
                rand.Next(3) // Random stair type
            );
            
            _stairs.Add(newStair);
            
            // Clean up off-screen stairs
            _stairs.RemoveAll(s => s.Position.X < _player.Position.X - 1000);
        }

        protected override void Update(GameTime gameTime)
        {
            float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;
            
            switch (_currentState)
            {
                case GameState.Playing:
                    UpdatePlaying(deltaTime);
                    break;
                case GameState.GameOver:
                    UpdateGameOver(deltaTime);
                    break;
            }

            base.Update(gameTime);
        }

        private void UpdatePlaying(float deltaTime)
        {
            // Update player
            _player.Update(Keyboard.GetState(), deltaTime);
            
            // Generate new stairs
            _timeSinceLastStair += deltaTime;
            if (_timeSinceLastStair >= STAIR_GENERATION_INTERVAL)
            {
                GenerateStairs();
                _timeSinceLastStair = 0;
            }
            
            // Update particle effects
            _particleSystem.Update(deltaTime);
            
            // Collision detection with spatial partitioning
            var activeStairs = _stairs.Where(s => 
                Math.Abs(s.Position.X - _player.Position.X) < 200
            );
            
            foreach (var stair in activeStairs)
            {
                if (_player.Bounds.Intersects(stair.Bounds))
                {
                    HandleCollision(_player, stair);
                }
            }
            
            // Check game over condition
            if (_player.Position.Y > GraphicsDevice.Viewport.Height)
            {
                TransitionToGameOver();
            }
        }

        private void HandleCollision(Player player, Stair stair)
        {
            // Only handle collision if player is falling
            if (player.Velocity.Y > 0)
            {
                player.OnStair = true;
                player.Position = new Vector2(
                    player.Position.X, 
                    stair.Position.Y - player.Height
                );
                
                // Spawn particles and play sound
                _particleSystem.SpawnLandingEffect(player.Position);
                _soundManager.PlaySound("land");
                
                // Handle special stair types
                stair.OnCollision(player);
            }
        }

        private void TransitionToGameOver()
        {
            _currentState = GameState.GameOver;
            _scoreManager.SaveScore(new Score {
                Points = _player.Score,
                TimeElapsed = _gameTime,
                StairsClimbed = _player.StairsClimbed,
                Timestamp = DateTime.UtcNow
            });
            
            _soundManager.PlaySound("gameover");
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);
            
            _spriteBatch.Begin(
                SpriteSortMode.Deferred,
                BlendState.AlphaBlend,
                SamplerState.LinearClamp,
                DepthStencilState.None,
                RasterizerState.CullNone,
                null,
                Matrix.CreateTranslation(new Vector3(-_player.Position.X + 400, 0, 0))
            );
            
            // Draw game elements
            foreach (var stair in _stairs)
                stair.Draw(_spriteBatch);
                
            _player.Draw(_spriteBatch);
            _particleSystem.Draw(_spriteBatch);
            
            // Draw UI elements without camera transform
            _spriteBatch.End();
            _spriteBatch.Begin();
            DrawUI();
            _spriteBatch.End();
            
            base.Draw(gameTime);
        }

        private void DrawUI()
        {
            // Draw score and other UI elements
            _spriteBatch.DrawString(
                _font,
                $"Score: {_player.Score}",
                new Vector2(10, 10),
                Color.White
            );
            
            if (_currentState == GameState.GameOver)
            {
                DrawGameOverScreen();
            }
        }
    }
}