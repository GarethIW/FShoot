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
open FShoot.Particles
open FShoot.Enemies

type FShootGame() as x =
    inherit Game()
    let graphics = new GraphicsDeviceManager(x)
    let mutable spriteBatch = Unchecked.defaultof<_>
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
        spriteBatch <- new SpriteBatch (graphics.GraphicsDevice)

        ParticleManager.Instance.LoadContent(x.Content)

    override x.Update (gameTime:GameTime) = 
        let boundsRect = x.GraphicsDevice.Viewport.Bounds
        boundsRect.Inflate(-100, 0)

        ParticleManager.Instance.Update(gameTime)   
        EnemyManager.Instance.Update(gameTime, boundsRect)   
               
        base.Update (gameTime)

    /// This is called when the game should draw itself. 
    override x.Draw (gameTime:GameTime) =
        graphics.GraphicsDevice.Clear (Color.White)

        ParticleManager.Instance.Draw(spriteBatch)

        base.Draw (gameTime)




