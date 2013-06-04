namespace FShoot

open System
open Microsoft.Xna.Framework
open Microsoft.Xna.Framework.Audio
open Microsoft.Xna.Framework.Graphics
open Microsoft.Xna.Framework.Input
open Microsoft.Xna.Framework.Input.Touch
open Microsoft.Xna.Framework.Storage
open Microsoft.Xna.Framework.Content
open Microsoft.Xna.Framework.Media

type FShootGame() as x =
    inherit Game()
    static let rnd = System.Random()
    let graphics = new GraphicsDeviceManager(x)
    let particleManager = new Particles.ParticleManager()
    let mutable spriteBatch = Unchecked.defaultof<_>
    let mutable logoTexture = Unchecked.defaultof<_>
    do x.Content.RootDirectory <- "content"
       graphics.IsFullScreen <- false

    /// Overridden from the base Game.Initialize. Once the GraphicsDevice is setup,
    /// we'll use the viewport to initialize some values.
    override x.Initialize() = 
        graphics.PreferredBackBufferWidth <- 1280
        graphics.PreferredBackBufferHeight <- 720
        graphics.ApplyChanges()

        base.Initialize()

    /// Load your graphics content.
    override x.LoadContent() =
        // Create a new SpriteBatch, which can be use to draw textures.
        spriteBatch <- new SpriteBatch (graphics.GraphicsDevice)

        particleManager.LoadContent(x.Content)

        // TODO: use this.Content to load your game content here eg.
        logoTexture <- x.Content.Load<_>("particles")

    /// Allows the game to run logic such as updating the world,
    /// checking for collisions, gathering input, and playing audio.
    override x.Update (gameTime:GameTime) =
        // TODO: Add your update logic here  
        
        particleManager.Spawn(Vector2(float32 graphics.GraphicsDevice.Viewport.Width/2.0f, 20.0f), Vector2(-3.0f + (float32(rnd.NextDouble()) * 6.0f), 0.0f), 2000.0f)
          
        particleManager.Update(gameTime)             
        base.Update (gameTime)

    /// This is called when the game should draw itself. 
    override x.Draw (gameTime:GameTime) =
        // Clear the backbuffer
        graphics.GraphicsDevice.Clear (Color.CornflowerBlue)

        particleManager.Draw(spriteBatch)

        spriteBatch.Begin()

        // draw the logo
        //spriteBatch.Draw (logoTexture, Vector2 (130.f, 200.f), Color.White);
        spriteBatch.End()

        //TODO: Add your drawing code here
        base.Draw (gameTime)




