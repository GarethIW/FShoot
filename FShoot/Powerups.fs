namespace FShoot

open System
open System.Collections
open System.Collections.Generic
open System.Linq
open Microsoft.Xna.Framework
open Microsoft.Xna.Framework.Graphics
open Microsoft.Xna.Framework.Content
open FShoot.Particles

module Powerups = 

    type Powerup(pos, speed, life) as this = 
        [<DefaultValue>] val mutable Position : Vector2
        [<DefaultValue>] val mutable Speed : Vector2
        [<DefaultValue>] val mutable Life : float32
        [<DefaultValue>] val mutable Active : bool

        do
            this.Active <- true
            this.Position <- pos
            this.Speed <- speed
            this.Life <- life

        member this.Update (addParticle:Particle->unit) (gameTime:GameTime) =
            this.Life <- this.Life - float32 gameTime.ElapsedGameTime.TotalMilliseconds
            this.Position <- this.Position + this.Speed

            if this.Life <= 0.0f then
                this.Active <- false

            for r in 1.0f .. 6.0f do
                addParticle {
                    Source = Rectangle(1,1,1,1)
                    Position = this.Position + Vector2(20.0f * float32(Math.Cos(gameTime.TotalGameTime.TotalMilliseconds * (float(r) / float(2)))), 20.0f * float32(Math.Sin(gameTime.TotalGameTime.TotalMilliseconds * (float(r) / float(2)))))
                    Speed = Vector2.Zero
                    SpeedDelta = Vector2.Zero
                    Life = 0.0f
                    FadeSpeed = 0.2f
                    Tint = Color.DarkOrange
                    Scale = 6.0f
                    Rotation = 0.0f
                    RotationSpeed = 0.0f
                    Alpha = 1.0f
                    Active = true
                    }

    type PowerupManager() as this =
        [<DefaultValue>] val mutable Powerups : List<Powerup>
        [<DefaultValue>] val mutable KillsSinceLastPowerup : int
        static let instance = new PowerupManager()
        static member internal Instance = instance 
        do
            this.KillsSinceLastPowerup <- 0
            this.Powerups <- new List<Powerup>()

        member this.Update addParticle (gameTime:GameTime) =
            this.Powerups.ForEach(fun p -> p.Update addParticle (gameTime))
            this.Powerups.RemoveAll(fun p -> not p.Active) |> ignore

        member this.Spawn(pos, speed, life) =
            this.Powerups.Add(Powerup(pos, speed, life))


   






