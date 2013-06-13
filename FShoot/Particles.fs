namespace FShoot

open System
open System.Collections
open System.Collections.Generic
open System.Linq
open Microsoft.Xna.Framework
open Microsoft.Xna.Framework.Graphics
open Microsoft.Xna.Framework.Content

module ParticlesTwo =

    type Particle = { 
        Position : Vector2
        Speed : Vector2
        SpeedDelta : Vector2
        Life : float32
        FadeSpeed : float32
        Alpha : float32
        Tint : Color
        Source : Rectangle
        Scale : float32
        Rotation : float32
        RotationSpeed : float32 
        Active : bool }

    let mutable ParticlesList : Particle list = []
    let AddParticle particle = ParticlesList <- particle :: ParticlesList
    let RemoveInactiveParticles = ParticlesList <- ParticlesList |> List.filter(fun p -> p.Alpha > 0.0f)

    let UpdateParticle (gameTime:GameTime) particle = 
        let decay = 
            if particle.Life <= 0.0f && particle.Alpha >= 0.0f then particle.Alpha - particle.FadeSpeed else particle.Alpha               
        let deactivate = particle.Alpha <= 0.0f

        { particle with
                Life = particle.Life - float32 gameTime.ElapsedGameTime.TotalMilliseconds
                Speed = particle.Speed + particle.SpeedDelta
                Position = particle.Position + particle.Speed
                Rotation = particle.Rotation + particle.RotationSpeed
                Alpha = decay 
                Active = deactivate }

    let DrawParticle (spriteBatch:SpriteBatch) (texture:Texture2D) particle = 
        spriteBatch.Draw(texture, particle.Position, 
                         System.Nullable(particle.Source), 
                         particle.Tint * particle.Alpha, 
                         particle.Rotation, 
                         Vector2(float32 particle.Source.Width, float32 particle.Source.Height) / 2.0f, 
                         particle.Scale, 
                         SpriteEffects.None, 1.0f)

    let DrawParticles (spriteBatch:SpriteBatch) (texture:Texture2D) particles = 
        let drawParticle = DrawParticle spriteBatch texture
        spriteBatch.Begin()
        particles |> List.iter drawParticle
        spriteBatch.End()
        
        

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
        let Particles = new List<Particle>()
        let mutable particleTexture = Unchecked.defaultof<_>
        static let instance = new ParticleManager()
        static member internal Instance = instance 

        member this.LoadContent(content:ContentManager) = 
            particleTexture <- content.Load<_>("particles")

        member this.Update(gameTime:GameTime) =
            Particles.ForEach(fun p -> p.Update(gameTime))
            Particles.RemoveAll(fun p -> not p.Active) |> ignore

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

   




