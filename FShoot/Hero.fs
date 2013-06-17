namespace FShoot

open System
open Microsoft.Xna.Framework
open Microsoft.Xna.Framework.Graphics
open Microsoft.Xna.Framework.Content
open FShoot.Particles
open FShoot.Projectiles
open FShoot.Powerups
open FShoot.Audio
open FShoot.Text

type Hero(pos, tint) as this = 
        [<DefaultValue>] val mutable Position : Vector2
        [<DefaultValue>] val mutable Speed : Vector2
        [<DefaultValue>] val mutable Tint : Color
        [<DefaultValue>] val mutable Size : float32
        [<DefaultValue>] val mutable Active : bool
        [<DefaultValue>] val mutable PowerupLevel : int
        [<DefaultValue>] val mutable Score : int
        let mutable gunCooldownTime = 250.0f
        let mutable gunCooldown = 0.0f
        let mutable hitbox = Rectangle(0,0,1,1)
        let mutable shape = array2D [| 
                                        [| 0.0f; 0.0f; 0.0f; 0.0f; 0.0f; 0.0f; 0.0f; 1.0f |];  
                                        [| 0.0f; 0.0f; 0.0f; 0.0f; 1.0f; 1.0f; 1.0f; 1.0f |];  
                                        [| 0.0f; 0.0f; 1.0f; 1.0f; 1.0f; 1.0f; 1.0f; 0.0f |];  
                                        [| 1.0f; 1.0f; 1.0f; 1.0f; 1.0f; 1.0f; 0.0f; 0.0f |];  
                                        [| 1.0f; 1.0f; 1.0f; 1.0f; 1.0f; 1.0f; 0.0f; 0.0f |];  
                                        [| 0.0f; 0.0f; 1.0f; 1.0f; 1.0f; 1.0f; 1.0f; 0.0f |];  
                                        [| 0.0f; 0.0f; 0.0f; 0.0f; 1.0f; 1.0f; 1.0f; 1.0f |];  
                                        [| 0.0f; 0.0f; 0.0f; 0.0f; 0.0f; 0.0f; 0.0f; 1.0f |];  
                                    |] 
        do
            this.Position <- pos
            this.Tint <- tint
            this.Size <- 8.0f
            this.Active <- true
            this.PowerupLevel <- 0
            this.Score <- 0

        member this.Update(gameTime:GameTime, bounds: Rectangle) =

            if this.Active = true then
                this.Position <- this.Position + this.Speed
                this.Position <- Vector2.Clamp(this.Position, Vector2(float32 bounds.X, float32 bounds.Y), Vector2(float32 bounds.Right, float32 bounds.Bottom))

                gunCooldownTime <- 250.0f - ((float32 this.PowerupLevel) * 20.0f)
                if gunCooldownTime < 50.0f then gunCooldownTime <- 50.0f

                this.Speed.X <- MathHelper.Clamp(this.Speed.X, -(3.0f + (0.5f * float32 this.PowerupLevel)), (3.0f + (0.5f * float32 this.PowerupLevel)))
                this.Speed.X <- MathHelper.Clamp(this.Speed.X, -10.0f, 10.0f)

                if gunCooldown > 0.0f then
                    gunCooldown <- gunCooldown - float32 gameTime.ElapsedGameTime.TotalMilliseconds

                let mutable found = false
                for y in 0 .. 7 do
                    for x in 0 .. 7 do
                        if shape.[x,y] > 0.0f then
                            if shape.[x,y] > 0.5f then found <- true
                            ParticleManager.Instance.Spawn(Rectangle(1,1,1,1), 
                                                            this.Position + ((Vector2(-3.0f,-3.0f) * this.Size) + (Vector2.One * -(this.Size/2.0f))) + (Vector2(float32 x, float32 y) * this.Size),
                                                            Vector2(0.0f,1.0f), Vector2.Zero,
                                                            0.0f,
                                                            0.1f,
                                                            this.Tint,
                                                            (this.Size * 0.5f) + (float32(Helper.Rand.NextDouble()) * (this.Size * 0.8f)),
                                                            0.0f, 0.0f,
                                                            shape.[x,y])
                if found = false then this.Die()

                hitbox <- Rectangle(int((this.Position + ((Vector2(-3.0f,-3.0f) * this.Size) + (Vector2.One * -(this.Size)))).X),
                                     int((this.Position + ((Vector2(-3.0f,-3.0f) * this.Size) + (Vector2.One * -(this.Size)))).Y),
                                     int((Vector2(8.0f, 8.0f) * this.Size).X),
                                     int((Vector2(8.0f, 8.0f) * this.Size).Y))

                ProjectileManager.Instance.Projectiles.ForEach(fun p -> this.CheckProjectileCollision(p))
                PowerupManager.Instance.Powerups.ForEach(fun p -> this.CheckPowerupCollision(p))

        member this.CheckProjectileCollision(p:Projectile) =
            if hitbox.Contains(int p.Position.X, int p.Position.Y) && p.Owner = ProjectileOwner.Enemy then
                for y in 0 .. 7 do
                    for x in 0 .. 7 do
                        if shape.[x,y] >= 1.0f then
                            let phb = Rectangle(int((this.Position + ((Vector2(-3.0f,-3.0f) * this.Size) + (Vector2.One * -(this.Size*1.4f)))).X) + int((Vector2(float32 x, float32 y) * this.Size).X),
                                                int((this.Position + ((Vector2(-3.0f,-3.0f) * this.Size) + (Vector2.One * -(this.Size*1.4f)))).Y) + int((Vector2(float32 x, float32 y) * this.Size).Y),
                                                int(this.Size * 1.8f),
                                                int(this.Size * 1.8f))
                            if phb.Contains(int p.Position.X, int p.Position.Y) then
                                shape.[x,y] <- 0.3f
                                p.Life <- 0.0f
                                if this.PowerupLevel > 0 then 
                                    this.PowerupLevel <- this.PowerupLevel - 1
                                    TextManager.Instance.DrawText(this.Position + Vector2(0.0f, -(50.0f + (float32(Helper.Rand.NextDouble()) * 25.0f))),
                                          sprintf "POWER DOWN",
                                          4.0f,
                                          2.0f,
                                          0.0f,
                                          Color(1.0f,0.3f,0.3f),
                                          1000.0f,
                                          false)
                                ParticleManager.Instance.Spawn(Rectangle(1,1,1,1), 
                                                        this.Position + ((Vector2(-3.0f,-3.0f) * this.Size) + (Vector2.One * -(this.Size/2.0f))) + (Vector2(float32 x, float32 y) * this.Size),
                                                        Vector2(-0.5f + (float32(Helper.Rand.NextDouble())), -2.0f), Vector2(0.0f,0.1f),
                                                        1000.0f,
                                                        0.01f,
                                                        this.Tint,
                                                        this.Size * 3.0f,
                                                        0.0f, -0.5f + (float32(Helper.Rand.NextDouble())),
                                                        1.0f)

        member this.CheckPowerupCollision(p:Powerup) =
            if this.Active then
                hitbox <- Rectangle(int((this.Position + ((Vector2(-3.0f,-3.0f) * this.Size) + (Vector2.One * -(this.Size)))).X),
                                         int((this.Position + ((Vector2(-3.0f,-3.0f) * this.Size) + (Vector2.One * -(this.Size)))).Y),
                                         int((Vector2(8.0f, 8.0f) * this.Size).X),
                                         int((Vector2(8.0f, 8.0f) * this.Size).Y))
                hitbox.Inflate(20,20)
                if hitbox.Contains(int p.Position.X, int p.Position.Y) then
                    p.Active <- false
                    if this.PowerupLevel< 9 then 
                        this.PowerupLevel <- this.PowerupLevel + 1
                        TextManager.Instance.DrawText(this.Position + Vector2(0.0f, -(50.0f + (float32(Helper.Rand.NextDouble()) * 25.0f))),
                                          sprintf "POWER UP",
                                          4.0f,
                                          2.0f,
                                          0.0f,
                                          Color(1.0f,0.3f,0.3f),
                                          1000.0f,
                                          false)
                    else
                        this.Score <- this.Score + 500
                        TextManager.Instance.DrawText(this.Position + Vector2(0.0f, -(50.0f + (float32(Helper.Rand.NextDouble()) * 25.0f))),
                                          sprintf "500",
                                          5.0f,
                                          2.0f,
                                          0.0f,
                                          Color(1.0f,0.3f,0.3f),
                                          1000.0f,
                                          false)

        member this.CheckEnemyCollision(hb:Rectangle) =
            if this.Active then
                hitbox <- Rectangle(int((this.Position + ((Vector2(-3.0f,-3.0f) * this.Size) + (Vector2.One * -(this.Size)))).X),
                                         int((this.Position + ((Vector2(-3.0f,-3.0f) * this.Size) + (Vector2.One * -(this.Size)))).Y),
                                         int((Vector2(8.0f, 8.0f) * this.Size).X),
                                         int((Vector2(8.0f, 8.0f) * this.Size).Y))
                hitbox.Inflate(-20,-20)
                if hitbox.Intersects(hb) then this.Die()

        member this.Move(dir) =
            if this.Active then
                this.Speed.X <- this.Speed.X + ((0.3f + (0.1f * (float32 this.PowerupLevel))) * dir)

        member this.Die() = 
            for y in 0 .. 7 do
                    for x in 0 .. 7 do
                        ParticleManager.Instance.Spawn(Rectangle(1,1,1,1), 
                                                        this.Position + ((Vector2(-3.0f,-3.0f) * this.Size) + (Vector2.One * -(this.Size/2.0f))) + (Vector2(float32 x, float32 y) * this.Size),
                                                        Vector2(-2.5f + (float32(Helper.Rand.NextDouble()) * 5.0f), -5.0f), Vector2(0.0f,0.1f),
                                                        1000.0f,
                                                        0.01f,
                                                        this.Tint,
                                                        this.Size * 3.0f,
                                                        0.0f, -0.5f + (float32(Helper.Rand.NextDouble())),
                                                        shape.[x,y])
            this.Active <- false

        member this.Fire() =
            if this.Active then
                if gunCooldown <= 0.0f then
                    gunCooldown <- gunCooldownTime
                    AudioManager.Instance.Play("shoot", 0.1f) |> ignore
                    match this.PowerupLevel with
                    |          i when i  < 3 -> ProjectileManager.Instance.Spawn(ProjectileOwner.Hero, this.Position + Vector2(0.0f, -40.0f), Vector2(0.0f, -10.0f), 2000.0f, Color.Red, 4.0f)
                    | i when i >= 3 && i < 6 -> ProjectileManager.Instance.Spawn(ProjectileOwner.Hero, this.Position + Vector2(-10.0f, -40.0f), Vector2(0.0f, -10.0f), 2000.0f, Color.Red, 4.0f)
                                                ProjectileManager.Instance.Spawn(ProjectileOwner.Hero, this.Position + Vector2(10.0f, -40.0f), Vector2(0.0f, -10.0f), 2000.0f, Color.Red, 4.0f)
                    | i when i >= 6 && i < 9 -> ProjectileManager.Instance.Spawn(ProjectileOwner.Hero, this.Position + Vector2(0.0f, -40.0f), Vector2(0.0f, -10.0f), 2000.0f, Color.Red, 4.0f)
                                                ProjectileManager.Instance.Spawn(ProjectileOwner.Hero, this.Position + Vector2(-10.0f, -40.0f), Vector2(-1.0f, -10.0f), 2000.0f, Color.Red, 4.0f)
                                                ProjectileManager.Instance.Spawn(ProjectileOwner.Hero, this.Position + Vector2(10.0f, -40.0f), Vector2(1.0f, -10.0f), 2000.0f, Color.Red, 4.0f)
                    |          i when i >= 9 -> ProjectileManager.Instance.Spawn(ProjectileOwner.Hero, this.Position + Vector2(0.0f, -40.0f), Vector2(0.0f, -10.0f), 2000.0f, Color.Red, 4.0f)
                                                ProjectileManager.Instance.Spawn(ProjectileOwner.Hero, this.Position + Vector2(-10.0f, -40.0f), Vector2(-1.0f, -10.0f), 2000.0f, Color.Red, 4.0f)
                                                ProjectileManager.Instance.Spawn(ProjectileOwner.Hero, this.Position + Vector2(10.0f, -40.0f), Vector2(1.0f, -10.0f), 2000.0f, Color.Red, 4.0f)
                                                ProjectileManager.Instance.Spawn(ProjectileOwner.Hero, this.Position + Vector2(-30.0f, 20.0f), Vector2(-2.0f, -10.0f), 2000.0f, Color.Red, 4.0f)
                                                ProjectileManager.Instance.Spawn(ProjectileOwner.Hero, this.Position + Vector2(30.0f, 20.0f), Vector2(2.0f, -10.0f), 2000.0f, Color.Red, 4.0f)


        member this.RegenHealth() =
            // Give the hero 1 block of life back
            let mutable found = false
            for y in 0 .. 7 do
                    for x in 0 .. 7 do
                        if (shape.[x,y] >= 0.3f && shape.[x,y] < 1.0f) && found = false then
                            found <- true
                            shape.[x,y] <- 1.0f
                            ParticleManager.Instance.Spawn(Rectangle(1,1,1,1), 
                                                        this.Position + ((Vector2(-3.0f,-3.0f) * this.Size) + (Vector2.One * -(this.Size/2.0f))) + (Vector2(float32 x, float32 y) * this.Size),
                                                        Vector2.Zero, Vector2.Zero,
                                                        0.0f,
                                                        0.1f,
                                                        this.Tint,
                                                        this.Size * 2.0f,
                                                        0.0f, 0.0f,
                                                        1.0f)
                            TextManager.Instance.DrawText(this.Position + Vector2(0.0f, -(50.0f + (float32(Helper.Rand.NextDouble()) * 25.0f))),
                                          sprintf "REPAIRED",
                                          5.0f,
                                          2.0f,
                                          0.0f,
                                          Color(0.3f,0.3f,1.0f),
                                          1000.0f,
                                          false)