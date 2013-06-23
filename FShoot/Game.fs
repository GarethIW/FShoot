namespace FShoot

open System
open Microsoft.Xna.Framework
open Microsoft.Xna.Framework.Audio
open Microsoft.Xna.Framework.Graphics
open Microsoft.Xna.Framework.Input
open Microsoft.Xna.Framework.Content
open Microsoft.Xna.Framework.Media
open FShoot.Particles
open FShoot.Projectiles
open FShoot.Enemies
open FShoot.Powerups
open FShoot.Text
open FShoot.Audio

type GameState = {
    Particles : Particle list
}

type FShootGame() as x =
    inherit Game()
    let graphics = new GraphicsDeviceManager(x)
    let mutable spriteBatch = Unchecked.defaultof<_>

    let mutable particleTexture = Unchecked.defaultof<_>

    let mutable hero = Unchecked.defaultof<_>

    let mutable lks = Keyboard.GetState()
    let mutable lgs = GamePad.GetState(PlayerIndex.One)

    let mutable gameState = { Particles = [] }

    let mutable bestScore = 0
    let mutable bestWave = 0
    let mutable lastScore = 0
    let mutable lastWave = 0

    // Awesome gamestate here!
    let mutable showingTitleScreen = true
    let mutable titlePopTime = 0.0f
    let mutable titlePopPos = Vector2(0.0f,0.0f)

    do x.Content.RootDirectory <- "content"
       graphics.PreferredBackBufferWidth <- 1280
       graphics.PreferredBackBufferHeight <- 720
       graphics.GraphicsProfile <- GraphicsProfile.HiDef
       graphics.IsFullScreen <- false

    /// Overridden from the base Game.Initialize. Once the GraphicsDevice is setup,
    /// we'll use the viewport to initialize some values.
    override x.Initialize() = 
        base.Initialize()

    override x.LoadContent() =
        spriteBatch <- new SpriteBatch (graphics.GraphicsDevice)
        hero <- new Hero(Vector2(float32 x.GraphicsDevice.Viewport.Bounds.Center.X, float32 x.GraphicsDevice.Viewport.Bounds.Bottom - 50.0f), Color.DeepSkyBlue)
        //ParticleManager.Instance.LoadContent(x.Content)
        AudioManager.Instance.LoadContent(x.Content)
        particleTexture <- x.Content.Load<Texture2D>("particles")


    override x.Update (gameTime:GameTime) = 
        if x.IsActive then
            let ks = Keyboard.GetState()
            let gs = GamePad.GetState(PlayerIndex.One)
            
            let addParticle p = 
                    gameState <- { gameState with Particles = AddParticle gameState.Particles p }

            // Starfield
            addParticle {
                Source = Rectangle(1,1,1,1)
                Position = Vector2((float32(Helper.Rand.NextDouble()) * float32 x.GraphicsDevice.Viewport.Width), -10.0f)
                Speed = Vector2(0.0f,2.0f + (float32(Helper.Rand.NextDouble())*4.0f))
                SpeedDelta = Vector2.Zero
                Life = 3000.0f
                FadeSpeed = 0.1f
                Tint = Color(Vector3.One * float32(Helper.Rand.NextDouble()) * 0.5f)
                Scale = 1.0f + (float32(Helper.Rand.NextDouble()) * (2.0f))
                Rotation = 0.0f
                RotationSpeed = 0.0f
                Alpha = 1.0f
                Active = true
                }

            // Title screen! (this is *not* how to do gamestate, but 1GAM does not give me time to write a gamestate management engine in F#!)
            if showingTitleScreen then 
                
                if titlePopTime > 0.0f then titlePopTime <- titlePopTime - float32 gameTime.ElapsedGameTime.TotalMilliseconds
                if Helper.Rand.Next(100) = 1 then 
                    titlePopTime <- float32(Helper.Rand.NextDouble()) * 500.0f
                    titlePopPos <- (Vector2(float32 x.GraphicsDevice.Viewport.Width, float32 x.GraphicsDevice.Viewport.Height)/2.0f - 
                                    Vector2(float32 x.GraphicsDevice.Viewport.Width, float32 x.GraphicsDevice.Viewport.Height)/4.0f) +
                                    Vector2(float32(Helper.Rand.NextDouble()) * (float32 x.GraphicsDevice.Viewport.Width / 2.0f), float32(Helper.Rand.NextDouble()) * (float32 x.GraphicsDevice.Viewport.Height / 2.0f))

                

                TextManager.Instance.DrawText(Vector2(float32 x.GraphicsDevice.Viewport.Width, float32 x.GraphicsDevice.Viewport.Height)/2.0f + Vector2(0.0f, 100.0f),
                                              "SPACE TO PLAY",
                                              5.0f,
                                              1.0f,
                                              0.0f,
                                              Color.Purple,
                                              0.0f,
                                              false)

                TextManager.Instance.DrawText(Vector2(0.0f, float32 x.GraphicsDevice.Viewport.Height) + Vector2(200.0f, - 50.0f),
                                              sprintf "BEST - %i WAVE %i" bestScore bestWave,
                                              3.0f,
                                              1.0f,
                                              0.0f,
                                              Color.Black,
                                              0.0f,
                                              false)

                TextManager.Instance.DrawText(Vector2(float32 x.GraphicsDevice.Viewport.Width, float32 x.GraphicsDevice.Viewport.Height) + Vector2(-200.0f, - 50.0f),
                                              sprintf "PREV - %i WAVE %i" lastScore lastWave,
                                              3.0f,
                                              1.0f,
                                              0.0f,
                                              Color.Black,
                                              0.0f,
                                              false)

              TextManager.Instance.DrawText addParticle ((if titlePopTime > 0.0f then titlePopPos else Vector2(float32 x.GraphicsDevice.Viewport.Width, float32 x.GraphicsDevice.Viewport.Height)/2.0f),
                                          "FSHOOT",
                                          (if titlePopTime > 0.0f then 35.0f else 20.0f),
                                          20.0f,
                                          0.0f,
                                          Color.Red,
                                          0.0f,
                                          true)
//

//                TextManager.Instance.DrawText(Vector2(200.0f, 600.0f),
//                                              "TEST TEXT",
//                                              5.0f,
//                                              1.0f,
//                                              0.0f,
//                                              Color.Black,
//                                              false)
//
//                TextManager.Instance.DrawText(Vector2(600.0f, 600.0f),
//                                              "DOT.MATRIX",
//                                              5.0f,
//                                              2.0f,
//                                              0.5f,
//                                              Color.Black,
//                                              false)
//
//                TextManager.Instance.DrawText(Vector2(800.0f, 500.0f),
//                                              "JITTERY",
//                                              6.0f,
//                                              2.0f,
//                                              1.0f,
//                                              Color.Magenta,
//                                              true)

              if (ks.IsKeyDown(Keys.Z) || ks.IsKeyDown(Keys.Space) || ks.IsKeyDown(Keys.LeftControl) || ks.IsKeyDown(Keys.Enter)) &&
                   (lks.IsKeyDown(Keys.Z) = false && lks.IsKeyDown(Keys.Space) = false && lks.IsKeyDown(Keys.LeftControl) = false && lks.IsKeyDown(Keys.Enter) = false) then 
                    showingTitleScreen <- false
                    hero <- new Hero(Vector2(float32 x.GraphicsDevice.Viewport.Bounds.Center.X, float32 x.GraphicsDevice.Viewport.Bounds.Bottom - 50.0f), Color.DeepSkyBlue)
                    EnemyManager.Instance.Reset()
                    ProjectileManager.Instance.Reset()


            // In-game! (Not showing title screen)
            else
                let boundsRect = x.GraphicsDevice.Viewport.Bounds
                boundsRect.Inflate(-100, 0)

                

                
                if ks.IsKeyDown(Keys.Left) || ks.IsKeyDown(Keys.A) then hero.Move(-1.0f) // hero.Speed.X <- hero.Speed.X - 0.3f
                else if ks.IsKeyDown(Keys.Right) || ks.IsKeyDown(Keys.D) then hero.Move (1.0f) //then hero.Speed.X <- hero.Speed.X + 0.3f
                else hero.Speed.X <- MathHelper.Lerp(hero.Speed.X, 0.0f, 0.1f)

                if ks.IsKeyDown(Keys.Z) || ks.IsKeyDown(Keys.Space) || ks.IsKeyDown(Keys.LeftControl) || ks.IsKeyDown(Keys.Enter) then hero.Fire()

                hero.Update addParticle (gameTime, boundsRect)
                ProjectileManager.Instance.Update addParticle (gameTime) |> ignore 
                PowerupManager.Instance.Update addParticle (gameTime) |> ignore
                EnemyManager.Instance.Update addParticle (gameTime, boundsRect, hero) |> ignore

                TextManager.Instance.DrawText(Vector2(float32 x.GraphicsDevice.Viewport.Bounds.Center.X, 20.0f), sprintf "%i" hero.Score, 5.0f, 2.0f, 0.0f, Color.Black, 0.0f, false)

                if not hero.Active then 
                    showingTitleScreen <- true
                    if hero.Score > bestScore then 
                        bestScore <- hero.Score
                        bestWave <- EnemyManager.Instance.WaveNumber

                    lastScore <- hero.Score
                    lastWave <- EnemyManager.Instance.WaveNumber
                    
            
            gameState <- { gameState with Particles = gameState.Particles |> List.map(fun p -> UpdateParticle gameTime p) }
            gameState <- { gameState with Particles = gameState.Particles |> RemoveInactiveParticles }

            x.Window.Title <- sprintf "Particles: %i" gameState.Particles.Length

            //x.Window.Title <- sprintf "Particles %i" ParticleManager.Instance.Particles.Count

            lks <- ks
            lgs <- gs

        base.Update (gameTime)

    /// This is called when the game should draw itself. 
    override x.Draw (gameTime:GameTime) =
        graphics.GraphicsDevice.Clear (Color.White)

        //ParticleManager.Instance.Draw(spriteBatch)
        gameState.Particles |> DrawParticles spriteBatch particleTexture


        base.Draw (gameTime)




