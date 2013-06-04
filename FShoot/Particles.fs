namespace FShoot

open System
open Microsoft.Xna.Framework
open Microsoft.Xna.Framework.Graphics
open Microsoft.Xna.Framework.Content

module Particles = 

    type Particle() = 
        [<DefaultValue>] val mutable Position : Vector2
        [<DefaultValue>] val mutable Speed : Vector2
        [<DefaultValue>] val mutable SpeedDelta : Vector2
        [<DefaultValue>] val mutable Life : float32
        [<DefaultValue>] val mutable Alpha : float32
        [<DefaultValue>] val mutable Tint : Color
        [<DefaultValue>] val mutable Source : Rectangle
        [<DefaultValue>] val mutable Scale : float32
        [<DefaultValue>] val mutable Rotation : float32
        [<DefaultValue>] val mutable RotationSpeed : float32
        [<DefaultValue>] val mutable Active : bool

        member this.Update(gameTime:GameTime) =
            this.Life <- this.Life - float32 gameTime.ElapsedGameTime.TotalMilliseconds
            this.Speed <- this.Speed + this.SpeedDelta
            this.Position <- this.Position + this.Speed
            this.Rotation <- this.Rotation + this.RotationSpeed
            
            if this.Life <= 0.0f then
                if this.Alpha >= 0.0f then
                    this.Alpha <- this.Alpha - 0.01f
                else
                    this.Active <- false

    type ParticleManager() =
        let MAX_PARTICLES = 5000
        let Particles = [| for i in 0 .. MAX_PARTICLES -> new Particle() |]
        let mutable particleTexture = Unchecked.defaultof<_>

        member this.LoadContent(content:ContentManager) = 
            particleTexture <- content.Load<_>("particles")

        member this.Update(gameTime:GameTime) =
            for i in 0 .. Particles.Length - 1 do
                Particles.[i].Update(gameTime)

        member this.Draw(spriteBatch:SpriteBatch) =
            spriteBatch.Begin()
            for i in 0 .. Particles.Length - 1 do
                spriteBatch.Draw(particleTexture, Particles.[i].Position, 
                                 System.Nullable(Particles.[i].Source), 
                                 Particles.[i].Tint * Particles.[i].Alpha, 
                                 Particles.[i].Rotation, 
                                 Vector2(float32 Particles.[i].Source.Width, float32 Particles.[i].Source.Height) / 2.0f, 
                                 Particles.[i].Scale, 
                                 SpriteEffects.None, 1.0f)
            spriteBatch.End()

        member this.Spawn(source, pos, speed, speeddelta, life, tint, scale, rot, rotspeed) =
            let part = Array.find<Particle>(fun p -> p.Active = false) Particles
            part.Position <- pos
            part.Speed <- speed
            part.SpeedDelta <- speeddelta
            part.Life <- life
            part.Tint <- tint
            part.Scale <- scale
            part.Source <- source
            part.Rotation <- rot
            part.RotationSpeed <- rotspeed
            part.Alpha <- 1.0f
            part.Active <- true




