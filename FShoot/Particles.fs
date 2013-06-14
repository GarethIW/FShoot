namespace FShoot

open System
open Microsoft.Xna.Framework
open Microsoft.Xna.Framework.Graphics
open Microsoft.Xna.Framework.Content

module Particles =

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
            
    let AddParticle particles particle = particle :: particles
    let RemoveInactiveParticles particles = particles |> List.filter(fun p -> p.Alpha > 0.0f)

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