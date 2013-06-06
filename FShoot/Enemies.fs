namespace FShoot

open System
open Microsoft.Xna.Framework
open Microsoft.Xna.Framework.Graphics
open Microsoft.Xna.Framework.Content
open FShoot.Particles
open FShoot.Projectiles

module Enemies = 

    type Enemy() = 
        [<DefaultValue>] val mutable Position : Vector2
        [<DefaultValue>] val mutable Target : Vector2
        [<DefaultValue>] val mutable Speed : float32
        [<DefaultValue>] val mutable Health : float32
        [<DefaultValue>] val mutable Tint : Color
        [<DefaultValue>] val mutable Size : float32
        [<DefaultValue>] val mutable Active : bool
        let mutable hitbox = Rectangle(0,0,1,1)
        let shape = Array2D.init 8 8 (fun x y -> false)                 
            
        member this.Update(gameTime:GameTime, waveSpeed: float32, bounds: Rectangle) =
            this.Target.X <- this.Target.X + waveSpeed

            this.Position <- Vector2.Lerp(this.Position, this.Target, this.Speed)

            if this.Health <= 0.0f then
                this.Active <- false

            hitbox <- Rectangle(int((this.Position + ((Vector2(-3.0f,-3.0f) * this.Size) + (Vector2.One * -(this.Size)))).X),
                                 int((this.Position + ((Vector2(-3.0f,-3.0f) * this.Size) + (Vector2.One * -(this.Size)))).Y),
                                 int((Vector2(8.0f, 8.0f) * this.Size).X),
                                 int((Vector2(8.0f, 8.0f) * this.Size).Y))
            
            ParticleManager.Instance.Spawn(Rectangle(1,1,1,1), 
                                                        this.Position,
                                                        Vector2(0.0f,0.0f), Vector2.Zero,
                                                        0.0f,
                                                        0.1f,
                                                        this.Tint,
                                                        ((this.Size * 4.0f) * 0.5f) + (float32(Helper.Rand.NextDouble()) * ((this.Size * 4.0f) * 0.8f)),
                                                        0.0f, 0.0f,
                                                        0.3f)

            // We don't *draw* an enemy, per se, we spawn a new particle for each active "pixel" in the 8x8 grid
            // We can alter the life and fade speed to produce trails, but need to balance for speed
            this.Active <- false
            for y in 0 .. 7 do
                for x in 0 .. 7 do
                    if shape.[x,y] then
                        // As long as one "pixel" of the enemy is still intact, the enemy is active.
                        this.Active <- true
                        ParticleManager.Instance.Spawn(Rectangle(1,1,1,1), 
                                                        this.Position + ((Vector2(-3.0f,-3.0f) * this.Size) + (Vector2.One * -(this.Size/2.0f))) + (Vector2(float32 x, float32 y) * this.Size),
                                                        Vector2(0.0f,1.0f), Vector2.Zero,
                                                        0.0f,
                                                        0.1f,
                                                        this.Tint,
                                                        (this.Size * 0.5f) + (float32(Helper.Rand.NextDouble()) * (this.Size * 0.8f)),
                                                        0.0f, 0.0f,
                                                        1.0f)
                        

        member this.CheckCollision(p:Projectile) =
            if hitbox.Contains(int p.Position.X, int p.Position.Y) then
                for y in 0 .. 7 do
                    for x in 0 .. 7 do
                        if shape.[x,y] then
                            let phb = Rectangle(int((this.Position + ((Vector2(-3.0f,-3.0f) * this.Size) + (Vector2.One * -(this.Size*1.4f)))).X) + int((Vector2(float32 x, float32 y) * this.Size).X),
                                                int((this.Position + ((Vector2(-3.0f,-3.0f) * this.Size) + (Vector2.One * -(this.Size*1.4f)))).Y) + int((Vector2(float32 x, float32 y) * this.Size).Y),
                                                int(this.Size * 1.8f),
                                                int(this.Size * 1.8f))
                            if phb.Contains(int p.Position.X, int p.Position.Y) then
                                shape.[x,y] <- false
                                p.Life <- 0.0f
                                ParticleManager.Instance.Spawn(Rectangle(1,1,1,1), 
                                                        this.Position + ((Vector2(-3.0f,-3.0f) * this.Size) + (Vector2.One * -(this.Size/2.0f))) + (Vector2(float32 x, float32 y) * this.Size),
                                                        Vector2(-0.5f + (float32(Helper.Rand.NextDouble())), -2.0f), Vector2(0.0f,0.1f),
                                                        1000.0f,
                                                        0.01f,
                                                        this.Tint,
                                                        this.Size,
                                                        0.0f, -0.5f + (float32(Helper.Rand.NextDouble())),
                                                        1.0f)
        
        member this.GenerateShape() =
            // Generate a random shape for the ship
            // It's an 8x8 grid of "pixels" mirrored vertically
            for y in 0 .. 7 do
                for x in 0 .. 3 do
                    shape.[x,y] <- Helper.Rand.Next(2) = 1
                    shape.[7-x,y] <- shape.[x,y]

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

        // Generate a new wave of enemies
        // Again, this is random and mirrored vertically
        // We can alter the number of columns, rows and spacing of the wave grid
        // as well as the size of the spawned enemies
        member this.NewWave(bounds: Rectangle) =
            let mutable pos = Vector2(float32 bounds.Center.X, float32 bounds.Top - 400.0f)
            for y in 0 .. waveRows-1 do
                for x in 0 .. (waveColumns/2)-1 do
                    if Helper.Rand.Next(2) = 0 then
                        // Enemies are spawned at a single point off the top of the screen, with their targets set to their position in the wave grid
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
                    ProjectileManager.Instance.Projectiles.ForEach(fun p -> e.CheckCollision(p))

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
            en.GenerateShape()




