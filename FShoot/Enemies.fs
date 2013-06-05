namespace FShoot

open System
open Microsoft.Xna.Framework
open Microsoft.Xna.Framework.Graphics
open Microsoft.Xna.Framework.Content
open FShoot.Particles

module Enemies = 

    type Enemy() = 
        [<DefaultValue>] val mutable Position : Vector2
        [<DefaultValue>] val mutable Target : Vector2
        [<DefaultValue>] val mutable Speed : float32
        [<DefaultValue>] val mutable Health : float32
        [<DefaultValue>] val mutable Tint : Color
        [<DefaultValue>] val mutable Size : float32
        [<DefaultValue>] val mutable Active : bool
        let shape = Array2D.init 8 8 (fun x y -> false)
        do
            for y in 0 .. 7 do
                for x in 0 .. 3 do
                    shape.[x,y] <- Helper.Rand.Next(2) = 1
                    shape.[7-x,y] <- shape.[x,y]
            
            
        member this.Update(gameTime:GameTime, waveSpeed: float32, bounds: Rectangle) =
            this.Target.X <- this.Target.X + waveSpeed
            this.Position <- Vector2.Lerp(this.Position, this.Target, this.Speed)

            
                

//            if Helper.Rand.Next(100) = 1 then
//                this.Target <- Vector2(float32 bounds.X + (float32(Helper.Rand.NextDouble()) * float32 bounds.Width), float32 bounds.Y + (float32(Helper.Rand.NextDouble()) * float32 bounds.Height))

            if this.Health <= 0.0f then
                this.Active <- false

           

            for y in 0 .. 7 do
                for x in 0 .. 7 do
                    if shape.[x,y] then
                        ParticleManager.Instance.Spawn(Rectangle(1,1,1,1), 
                                                        this.Position + ((Vector2(-3.0f,-3.0f) * this.Size) + (Vector2.One * -(this.Size/2.0f))) + (Vector2(float32 x, float32 y) * this.Size),
                                                        Vector2(0.0f,1.0f), Vector2.Zero,
                                                        50.0f,
                                                        0.1f,
                                                        this.Tint,
                                                        (this.Size * 0.5f) + (float32(Helper.Rand.NextDouble()) * (this.Size * 0.8f)),
                                                        0.0f, 0.0f)


    type EnemyManager() =
        let MAX_ENEMIES = 100
        let Enemies = [| for i in 0 .. MAX_ENEMIES -> new Enemy() |]
        let waveEnemySize = 8.0f
        let waveSpacing = 100.0f
        let waveColumns = 10
        let waveRows = 4
        let mutable waveSpeed = 2.0f
        static let instance = new EnemyManager()
        static member internal Instance = instance 

        member this.NewWave(bounds: Rectangle) =
            let mutable pos = Vector2(float32 bounds.Center.X, float32 bounds.Top - 400.0f)
            for y in 0 .. waveRows-1 do
                for x in 0 .. (waveColumns/2)-1 do
                    if Helper.Rand.Next(2) = 0 then
                        this.Spawn(Vector2(float32 bounds.Center.X, float32 bounds.Top - 400.0f), 0.02f, Vector2((float32 bounds.Center.X + (waveSpacing / 2.0f)) - (waveSpacing * (float32(waveColumns /2) - float32 x)), pos.Y + 450.0f), 100.0f, Color(Vector3(float32(Helper.Rand.NextDouble()) * 0.5f, float32(Helper.Rand.NextDouble()) * 0.5f, float32(Helper.Rand.NextDouble()) * 0.5f) + Vector3(0.3f,0.3f,0.3f)), waveEnemySize) 
                        this.Spawn(Vector2(float32 bounds.Center.X, float32 bounds.Top - 400.0f), 0.02f, Vector2((float32 bounds.Center.X - (waveSpacing / 2.0f)) + (waveSpacing * (float32(waveColumns /2) - float32 x)), pos.Y + 450.0f), 100.0f, Color(Vector3(float32(Helper.Rand.NextDouble()) * 0.5f, float32(Helper.Rand.NextDouble()) * 0.5f, float32(Helper.Rand.NextDouble()) * 0.5f) + Vector3(0.3f,0.3f,0.3f)), waveEnemySize) 
                pos.Y <- pos.Y + waveSpacing

        member this.Update(gameTime:GameTime, bounds: Rectangle) =
            let mutable activeCount = 0
            for e in Enemies do
                if e.Active then 
                    activeCount <- activeCount + 1
                    e.Update(gameTime, waveSpeed, bounds)
                    if waveSpeed > 0.0f && e.Position.X > float32 bounds.Right then waveSpeed <- -waveSpeed
                    if waveSpeed < 0.0f && e.Position.X < float32 bounds.Left then waveSpeed <- -waveSpeed
            

            if activeCount = 0 then
                this.NewWave(bounds)

        member this.Spawn(pos, speed, target, health, tint, size) =
            let en = Array.find<Enemy>(fun en -> en.Active = false) Enemies
            en.Position <- pos
            en.Target <- target
            en.Speed <- speed
            en.Health <- health
            en.Tint <- tint
            en.Size <- size
            en.Active <- true





