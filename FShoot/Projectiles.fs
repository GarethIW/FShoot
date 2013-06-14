namespace FShoot

open System
open System.Collections
open System.Collections.Generic
open System.Linq
open Microsoft.Xna.Framework
open Microsoft.Xna.Framework.Graphics
open Microsoft.Xna.Framework.Content
open FShoot.Particles
    
module Projectiles = 

    type ProjectileOwner =
        | Hero = 0
        | Enemy = 1

    type Projectile(owner, pos, speed, life, tint, size) as this = 
        [<DefaultValue>] val mutable Owner : ProjectileOwner
        [<DefaultValue>] val mutable Position : Vector2
        [<DefaultValue>] val mutable Speed : Vector2
        [<DefaultValue>] val mutable Life : float32
        [<DefaultValue>] val mutable Tint : Color
        [<DefaultValue>] val mutable Size : float32
        [<DefaultValue>] val mutable Active : bool
        do
            this.Active <- true
            this.Owner <- owner
            this.Position <- pos
            this.Speed <- speed
            this.Life <- life
            this.Tint <- tint
            this.Size <- size

        member this.Update (addParticle:Particle->unit) (gameTime:GameTime) =
            this.Life <- this.Life - float32 gameTime.ElapsedGameTime.TotalMilliseconds
            this.Position <- this.Position + this.Speed

            if this.Life <= 0.0f then
                this.Active <- false

            for y in 0.0f .. 4.0f .. 30.0f do
                let dir = if this.Speed.Y < 0.0f then 1.0f else -1.0f
                addParticle {
                    Source = Rectangle(1,1,1,1)
                    Position = this.Position + (Vector2(0.0f, y) * dir)
                    Speed = Vector2.Zero
                    SpeedDelta = Vector2.Zero
                    Life = 0.0f
                    FadeSpeed = 0.1f
                    Tint = this.Tint
                    Scale = this.Size
                    Rotation = 0.0f
                    RotationSpeed = 0.0f
                    Alpha = (1.0f/30.0f) * (30.0f - y)
                    Active = true
                    }

    type ProjectileManager() as this =
        [<DefaultValue>] val mutable Projectiles : List<Projectile>
        static let instance = new ProjectileManager()
        static member internal Instance = instance 
        do
            this.Projectiles <- new List<Projectile>()

        member this.Update addParticle (gameTime:GameTime) =
            this.Projectiles.ForEach(fun p -> p.Update addParticle (gameTime))
            this.Projectiles.RemoveAll(fun p -> not p.Active) |> ignore
            

        member this.Spawn(owner, pos, speed, life, tint, size) =
            this.Projectiles.Add(Projectile(owner, pos, speed, life, tint, size))


   






