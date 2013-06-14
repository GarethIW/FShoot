namespace FShoot

open System
open Microsoft.Xna.Framework
open Microsoft.Xna.Framework.Graphics
open Microsoft.Xna.Framework.Content
open FShoot.Particles
open FShoot.Projectiles
open FShoot.Powerups

module Enemies = 

    type Enemy() = 
        [<DefaultValue>] val mutable IsBoss : bool
        [<DefaultValue>] val mutable Position : Vector2
        [<DefaultValue>] val mutable Target : Vector2
        [<DefaultValue>] val mutable Speed : float32
        [<DefaultValue>] val mutable Tint : Color
        [<DefaultValue>] val mutable Size : float32
        [<DefaultValue>] val mutable Active : bool
        let mutable timeSinceLastShot = 0.0f
        let mutable hitbox = Rectangle(0,0,1,1)
        let shape = Array2D.init 8 8 (fun x y -> 0.0f)                 
            
        member this.Update (addParticle:Particle->unit) (gameTime:GameTime, waveSpeed: float32, waveNumber : int, bounds: Rectangle, hero: Hero) =
            if not this.IsBoss then
                // Standard enemies move left/right according to the wave direction
                this.Target.X <- this.Target.X + waveSpeed
            else
                // Boss enemies move to random locations
                if Helper.Rand.Next(100) = 1 then
                    this.Target <- Vector2(float32 bounds.X + (float32(Helper.Rand.NextDouble()) * float32 bounds.Width), float32 bounds.Y + (float32(Helper.Rand.NextDouble()) * (float32 bounds.Height - 200.0f)))

            this.Position <- Vector2.Lerp(this.Position, this.Target, this.Speed)

            // Keep track of this enemy's hitbox
            hitbox <- Rectangle(int((this.Position + ((Vector2(-3.0f,-3.0f) * this.Size) + (Vector2.One * -(this.Size)))).X),
                                 int((this.Position + ((Vector2(-3.0f,-3.0f) * this.Size) + (Vector2.One * -(this.Size)))).Y),
                                 int((Vector2(8.0f, 8.0f) * this.Size).X),
                                 int((Vector2(8.0f, 8.0f) * this.Size).Y))

            // Check for entire enemy colliding with hero ship (hero dies in this case)
            hero.CheckEnemyCollision addParticle (hitbox)
            
            // Generate the enemy's "heart" particle
            addParticle {
                Source = Rectangle(1,1,1,1)
                Position = this.Position
                Speed = Vector2(0.0f,0.0f)
                SpeedDelta = Vector2.Zero
                Life = 0.0f
                FadeSpeed = 0.01f
                Tint = this.Tint
                Scale = ((this.Size * 4.0f) * 0.5f) + (float32(Helper.Rand.NextDouble()) * ((this.Size * 4.0f) * 0.8f))
                Rotation = 0.0f
                RotationSpeed = 0.0f
                Alpha = 0.3f
                Active = true
                }

            // We don't *draw* an enemy, per se, we spawn a new particle for each active "pixel" in the 8x8 grid
            // We can alter the life and fade speed to produce trails, but need to balance for speed
            this.Active <- false
            for y in 0 .. 7 do
                for x in 0 .. 7 do
                    if shape.[x,y] > 0.0f then
                        // As long as one "pixel" of the enemy is still intact, the enemy is active and we can generate a particle this frame
                        this.Active <- true
                        addParticle {
                            Source = Rectangle(1,1,1,1)
                            Position = this.Position + ((Vector2(-3.0f,-3.0f) * this.Size) + (Vector2.One * -(this.Size/2.0f))) + (Vector2(float32 x, float32 y) * this.Size)
                            Speed = Vector2(0.0f,1.0f)
                            SpeedDelta = Vector2.Zero
                            Life = 0.0f
                            FadeSpeed = 0.1f
                            Tint = this.Tint
                            Scale = (this.Size * 0.5f) + (float32(Helper.Rand.NextDouble()) * (this.Size * 0.8f))
                            Rotation = 0.0f 
                            RotationSpeed = 0.0f
                            Alpha = shape.[x,y]
                            Active = true
                            }


            if not this.Active then
                // Enemy died, produce a death particle
                addParticle {
                    Source = Rectangle(1,1,1,1)
                    Position = this.Position
                    Speed = Vector2(0.0f,1.0f)
                    SpeedDelta = Vector2(0.0f,0.1f)
                    Life = 500.0f
                    FadeSpeed = 0.01f
                    Tint = this.Tint
                    Scale = this.Size * 4.0f
                    Rotation = 0.0f 
                    RotationSpeed = -0.2f + (float32(Helper.Rand.NextDouble()) * 0.4f)
                    Alpha = 0.3f
                    Active = true
                    }

                if Helper.Rand.Next(5) = 1 || this.IsBoss || PowerupManager.Instance.KillsSinceLastPowerup >=4 then 
                    PowerupManager.Instance.Spawn(this.Position, Vector2(0.0f, 4.0f), 3000.0f)
                    PowerupManager.Instance.KillsSinceLastPowerup <- 0
                else
                    PowerupManager.Instance.KillsSinceLastPowerup <- PowerupManager.Instance.KillsSinceLastPowerup + 1

            // Let's let the enemy shoot!
            // We do this randomly based on the wave number, so the higher the wave the more chance the enemy has to shoot
            // Set the difficulty curve to determine the wave that is *most difficult*, the game will gradually increase in difficulty until that wave then will not increase further
            let difficultyCurve = 60
            timeSinceLastShot <- timeSinceLastShot + float32 gameTime.ElapsedGameTime.TotalMilliseconds
            let mutable chance = waveNumber + (hero.PowerupLevel)
            if this.IsBoss then chance <- chance * 2
            if timeSinceLastShot > (float32 difficultyCurve * 100.0f) - (float32 waveNumber * 100.0f) then chance <- difficultyCurve
            if chance > difficultyCurve then chance <- difficultyCurve
            if Helper.Rand.Next(10 + (1000 - (chance * (1000 / difficultyCurve)))) = 1 then
                timeSinceLastShot <- 0.0f
                if this.IsBoss then
                    // Boss has a random "spread" for the projectile
                    ProjectileManager.Instance.Spawn(ProjectileOwner.Enemy, this.Position + Vector2(-20.0f + (float32(Helper.Rand.NextDouble()) * 40.0f), 20.0f), Vector2(0.0f, 4.0f + (float32 waveNumber / 10.0f)), 5000.0f, Color.Purple, 4.0f)
                else
                    // Standard enemy always shoots from the centre
                    ProjectileManager.Instance.Spawn(ProjectileOwner.Enemy, this.Position + Vector2(0.0f, 20.0f), Vector2(0.0f, 4.0f + (float32 waveNumber / 10.0f)), 5000.0f, Color.Purple, 4.0f)
                        

        member this.CheckCollision (addParticle:Particle->unit) (p:Projectile, waveNumber:int, hero:Hero) =
            // If the projectile is inside the enemy's hitbox
            if hitbox.Contains(int p.Position.X, int p.Position.Y) && this.Position.Y > -20.0f && p.Owner = ProjectileOwner.Hero then
                // Now check each of the enemy's "pixels" in turn
                for y in 0 .. 7 do
                    for x in 0 .. 7 do
                        if shape.[x,y] > 0.0f then
                            // This "pixel" is active, so create a new hitbox around the "pixel"
                            let phb = Rectangle(int((this.Position + ((Vector2(-3.0f,-3.0f) * this.Size) + (Vector2.One * -(this.Size*1.4f)))).X) + int((Vector2(float32 x, float32 y) * this.Size).X),
                                                int((this.Position + ((Vector2(-3.0f,-3.0f) * this.Size) + (Vector2.One * -(this.Size*1.4f)))).Y) + int((Vector2(float32 x, float32 y) * this.Size).Y),
                                                int(this.Size * 1.8f),
                                                int(this.Size * 1.8f))
                            if phb.Contains(int p.Position.X, int p.Position.Y) then
                                // The projectile has collided with a "pixel"
                                if this.IsBoss then
                                    // Boss has a per-"Pixel" life of 7
                                    shape.[x,y] <- shape.[x,y] - 0.1f
                                else
                                    // Standard enemies' life depends on wave
                                    shape.[x,y] <- shape.[x,y] - (1.0f / (1.0f + float32(waveNumber/5)))
                                if shape.[x,y] <= 0.3f then shape.[x,y] <- 0.0f
                                p.Life <- 0.0f
                                if Helper.Rand.Next(20 + (hero.PowerupLevel * 20)) = 1 then 
                                        PowerupManager.Instance.Spawn(this.Position, Vector2(0.0f, 4.0f), 3000.0f)
                                        PowerupManager.Instance.KillsSinceLastPowerup <- 0
                                if shape.[x,y] <= 0.0f then
                                    // Destroyed pixel particle
                                    addParticle {
                                        Source = Rectangle(1,1,1,1)
                                        Position = this.Position + ((Vector2(-3.0f,-3.0f) * this.Size) + (Vector2.One * -(this.Size/2.0f))) + (Vector2(float32 x, float32 y) * this.Size)
                                        Speed = Vector2(-0.5f + (float32(Helper.Rand.NextDouble())), -2.0f)
                                        SpeedDelta = Vector2(0.0f,0.1f)
                                        Life = 1000.0f
                                        FadeSpeed = 0.01f
                                        Tint = this.Tint
                                        Scale = this.Size
                                        Rotation = 0.0f 
                                        RotationSpeed = -0.5f + (float32(Helper.Rand.NextDouble()))
                                        Alpha = 1.0f
                                        Active = true
                                        }

                                else
                                    // Boss "shield" particle
                                    addParticle {
                                        Source = Rectangle(1,1,1,1)
                                        Position = this.Position + ((Vector2(-3.0f,-3.0f) * this.Size) + (Vector2.One * -(this.Size/2.0f))) + (Vector2(float32 x, float32 y) * this.Size)
                                        Speed = Vector2.Zero
                                        SpeedDelta = Vector2.Zero
                                        Life = 0.0f
                                        FadeSpeed = 0.05f
                                        Tint = this.Tint
                                        Scale = this.Size * 2.0f
                                        Rotation = 0.0f 
                                        RotationSpeed = 0.0f
                                        Alpha = 1.0f
                                        Active = true
                                        }
        
        member this.GenerateShape() =
            // Generate a random shape for the ship
            // It's an 8x8 grid of "pixels" mirrored vertically
            for y in 0 .. 7 do
                for x in 0 .. 3 do
                    shape.[x,y] <- if Helper.Rand.Next(2) = 1 then 1.0f else 0.0f
                    shape.[7-x,y] <- shape.[x,y]



    type EnemyManager() =
        let MAX_ENEMIES = 100
        let Enemies = [| for i in 0 .. MAX_ENEMIES -> new Enemy() |]
        let mutable waveNumber = 0
        let mutable waveEnemySize = 8.0f
        let mutable waveSpacing = 100.0f
        let mutable waveColumns = 2
        let mutable waveRows = 1
        let mutable waveSpeed = 2.0f
        static let instance = new EnemyManager()
        static member internal Instance = instance 

        // Generate a new wave of enemies
        // Again, this is random and mirrored vertically
        // We can alter the number of columns, rows and spacing of the wave grid
        // as well as the size of the spawned enemies
        member this.NewWave(bounds: Rectangle) =
            if waveNumber % 5 = 0 then
                // Boss wave every five waves
                for i in 1 .. waveNumber / 5 do
                    this.Spawn(true, Vector2(float32 bounds.Center.X, float32 bounds.Top - 300.0f), 0.02f, Vector2(float32 bounds.X + (float32(Helper.Rand.NextDouble()) * float32 bounds.Width), float32 bounds.Y + (float32(Helper.Rand.NextDouble()) * (float32 bounds.Height - 200.0f))), 100.0f, Color(Vector3(float32(Helper.Rand.NextDouble()) * 0.5f, float32(Helper.Rand.NextDouble()) * 0.5f, float32(Helper.Rand.NextDouble()) * 0.5f) + Vector3(0.3f,0.3f,0.3f)), 15.0f) 
            else
                // Standard wave
                let mutable enemyCount = 0
                let mutable pos = Vector2(float32 bounds.Center.X, float32 bounds.Top - 400.0f)
                for y in 0 .. waveRows-1 do
                    for x in 0 .. (waveColumns/2)-1 do
                        if Helper.Rand.Next(2) = 0 then
                            // Enemies are spawned at a single point off the top of the screen, with their targets set to their position in the wave grid
                            enemyCount <- enemyCount + 1
                            this.Spawn(false, Vector2(float32 bounds.Center.X, float32 bounds.Top - 400.0f), 0.02f, Vector2((float32 bounds.Center.X + (waveSpacing / 2.0f)) - (waveSpacing * (float32(waveColumns /2) - float32 x)), pos.Y + 450.0f), 100.0f, Color(Vector3(float32(Helper.Rand.NextDouble()) * 0.5f, float32(Helper.Rand.NextDouble()) * 0.5f, float32(Helper.Rand.NextDouble()) * 0.5f) + Vector3(0.3f,0.3f,0.3f)), waveEnemySize) 
                            this.Spawn(false, Vector2(float32 bounds.Center.X, float32 bounds.Top - 400.0f), 0.02f, Vector2((float32 bounds.Center.X - (waveSpacing / 2.0f)) + (waveSpacing * (float32(waveColumns /2) - float32 x)), pos.Y + 450.0f), 100.0f, Color(Vector3(float32(Helper.Rand.NextDouble()) * 0.5f, float32(Helper.Rand.NextDouble()) * 0.5f, float32(Helper.Rand.NextDouble()) * 0.5f) + Vector3(0.3f,0.3f,0.3f)), waveEnemySize) 
                    pos.Y <- pos.Y + waveSpacing
                if enemyCount = 0 then this.NewWave(bounds)

        member this.Update (addParticle:Particle->unit) (gameTime : GameTime, bounds : Rectangle, hero : Hero) =
            let mutable activeCount = 0
            for e in Enemies do
                if e.Active then 
                    activeCount <- activeCount + 1
                    e.Update addParticle (gameTime, waveSpeed, waveNumber, bounds, hero)

                    // Move the wave in the opposite direction if one of the enemies hits the edge of our boundaries
                    if (waveSpeed > 0.0f && e.Position.X > float32 bounds.Right) || (waveSpeed < 0.0f && e.Position.X < float32 bounds.Left) then 
                        waveSpeed <- -waveSpeed
                        for e in Enemies do e.Target.Y <- e.Target.Y + 10.0f

                    // Check for projectile collisions against each enemy and each projectile
                    ProjectileManager.Instance.Projectiles.ForEach(fun p -> e.CheckCollision addParticle (p, waveNumber, hero))

            // If there are no enemies left, start a new wave
            if activeCount = 0 then
                waveNumber <- waveNumber + 1
                hero.RegenHealth addParticle
                // This is where we introduce some "progression" into the game
                if waveNumber % 6 = 0 && waveRows < 4 then
                    // Every 6 waves, we add a new row and reset the columns (up to a max of 4 rows)
                    waveRows <- waveRows + 1
                    waveColumns <- waveRows * 2
                if waveNumber % 2 = 0 && waveColumns < 10 then
                    // Every 2 waves, we add two new columns (up to a max of 12)
                    waveColumns <- waveColumns + 2
                this.NewWave(bounds)

        member this.Spawn(boss, pos, speed, target, health, tint, size) =
            let en = Array.find<Enemy>(fun en -> en.Active = false) Enemies
            en.IsBoss <- boss
            en.Position <- pos
            en.Target <- target
            en.Speed <- speed
            en.Tint <- tint
            en.Size <- size
            en.Active <- true
            en.GenerateShape()




