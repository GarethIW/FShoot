namespace FShoot

open System
open System.Collections
open System.Collections.Generic
open System.Linq
open Microsoft.Xna.Framework
open Microsoft.Xna.Framework.Graphics
open Microsoft.Xna.Framework.Content

module Particles = 

    type Particle(source, pos, speed, speeddelta, life, fade, tint, scale, rot, rotspeed, startingalpha) as this = 
        [<DefaultValue>] val mutable Position : Vector2
        [<DefaultValue>] val mutable Speed : Vector2
        [<DefaultValue>] val mutable SpeedDelta : Vector2
        [<DefaultValue>] val mutable Life : float32
        [<DefaultValue>] val mutable FadeSpeed : float32
        [<DefaultValue>] val mutable Alpha : float32
        [<DefaultValue>] val mutable Tint : Color
        [<DefaultValue>] val mutable Source : Rectangle
        [<DefaultValue>] val mutable Scale : float32
        [<DefaultValue>] val mutable Rotation : float32
        [<DefaultValue>] val mutable RotationSpeed : float32
        [<DefaultValue>] val mutable Active : bool
        do
            this.Active <- true
            this.Position <- pos
            this.Speed <- speed
            this.SpeedDelta <- speeddelta
            this.Life <- life
            this.FadeSpeed <- fade
            this.Tint <- tint
            this.Scale <- scale
            this.Source <- source
            this.Rotation <- rot
            this.RotationSpeed <- rotspeed
            this.Alpha <- startingalpha

        member this.Update(gameTime:GameTime) =
            this.Life <- this.Life - float32 gameTime.ElapsedGameTime.TotalMilliseconds
            this.Speed <- this.Speed + this.SpeedDelta
            this.Position <- this.Position + this.Speed
            this.Rotation <- this.Rotation + this.RotationSpeed
            
            if this.Life <= 0.0f then
                if this.Alpha >= 0.0f then
                    this.Alpha <- this.Alpha - this.FadeSpeed
                else
                    this.Active <- false


    type ParticleManager() =
        let MAX_PARTICLES = 1000
        let Particles = new List<Particle>()
        let mutable particleTexture = Unchecked.defaultof<_>
        static let instance = new ParticleManager()
        static member internal Instance = instance 

        member this.LoadContent(content:ContentManager) = 
            particleTexture <- content.Load<_>("particles")

        member this.Update(gameTime:GameTime) =
            Particles.ForEach(fun p -> p.Update(gameTime))
            Particles.RemoveAll(fun p -> not p.Active)
            ()

        member this.Draw(spriteBatch:SpriteBatch) =
            spriteBatch.Begin()
            Particles.ForEach(fun p ->
                spriteBatch.Draw(particleTexture, p.Position, 
                                 System.Nullable(p.Source), 
                                 p.Tint * p.Alpha, 
                                 p.Rotation, 
                                 Vector2(float32 p.Source.Width, float32 p.Source.Height) / 2.0f, 
                                 p.Scale, 
                                 SpriteEffects.None, 1.0f))
            spriteBatch.End()

        member this.Spawn(source, pos, speed, speeddelta, life, fade, tint, scale, rot, rotspeed, startingalpha) =
            Particles.Add(Particle(source, pos, speed, speeddelta, life, fade, tint, scale, rot, rotspeed, startingalpha))

   




