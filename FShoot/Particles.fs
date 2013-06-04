namespace FShoot

open System
open Microsoft.Xna.Framework
open Microsoft.Xna.Framework.Graphics
open Microsoft.Xna.Framework.Content

module Particles = 

    type Particle() = 
        [<DefaultValue>] val mutable Position : Vector2
        [<DefaultValue>] val mutable Speed : Vector2
        [<DefaultValue>] val mutable Life : float32
        [<DefaultValue>] val mutable Alpha : float32
        [<DefaultValue>] val mutable Active : bool

        member this.Update(gameTime:GameTime, gravity) =
            this.Life <- this.Life - float32 gameTime.ElapsedGameTime.TotalMilliseconds
            this.Speed.Y <- this.Speed.Y + gravity
            this.Position <- this.Position + this.Speed
            
            if this.Life <= 0.0f then
                if this.Alpha >= 0.0f then
                    this.Alpha <- this.Alpha - 0.01f
                else
                    this.Active <- false

    type ParticleManager() =
        let MAX_PARTICLES = 5000
        let GRAVITY = 0.025f
        let Particles = [| for i in 0 .. MAX_PARTICLES -> new Particle() |]
        let mutable particleTexture = Unchecked.defaultof<_>

        member this.LoadContent(content:ContentManager) = 
            particleTexture <- content.Load<_>("particles")

        member this.Update(gameTime:GameTime) =
            for i in 0 .. Particles.Length - 1 do
                Particles.[i].Update(gameTime, GRAVITY)

        member this.Draw(spriteBatch:SpriteBatch) =
            spriteBatch.Begin()
            for i in 0 .. Particles.Length - 1 do
                spriteBatch.Draw(particleTexture, Particles.[i].Position, System.Nullable(Rectangle(11,0,15,8)), Color.White * Particles.[i].Alpha)
            spriteBatch.End()

        member this.Spawn(pos, speed, life) =
            let part = Array.find<Particle>(fun p -> p.Active = false) Particles
            part.Position <- pos
            part.Speed <- speed
            part.Life <- life
            part.Alpha <- 1.0f
            part.Active <- true




