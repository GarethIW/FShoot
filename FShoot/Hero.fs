namespace FShoot

open System
open Microsoft.Xna.Framework
open Microsoft.Xna.Framework.Graphics
open Microsoft.Xna.Framework.Content
open FShoot.Particles
open FShoot.Projectiles

type Hero(pos, tint) as this = 
        [<DefaultValue>] val mutable Position : Vector2
        [<DefaultValue>] val mutable Speed : Vector2
        [<DefaultValue>] val mutable Health : float32
        [<DefaultValue>] val mutable Tint : Color
        [<DefaultValue>] val mutable Size : float32
        [<DefaultValue>] val mutable Active : bool
        let mutable gunCooldownTime = 50.0f
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

        member this.Update(gameTime:GameTime, bounds: Rectangle) =
            this.Speed.X <- MathHelper.Clamp(this.Speed.X, -5.0f, 5.0f)
            this.Position <- this.Position + this.Speed
            this.Position <- Vector2.Clamp(this.Position, Vector2(float32 bounds.X, float32 bounds.Y), Vector2(float32 bounds.Right, float32 bounds.Bottom))

            if this.Health <= 0.0f then
                this.Active <- false

            if gunCooldown > 0.0f then
                gunCooldown <- gunCooldown - float32 gameTime.ElapsedGameTime.TotalMilliseconds

            for y in 0 .. 7 do
                for x in 0 .. 7 do
                    if shape.[x,y] > 0.0f then
                        ParticleManager.Instance.Spawn(Rectangle(1,1,1,1), 
                                                        this.Position + ((Vector2(-3.0f,-3.0f) * this.Size) + (Vector2.One * -(this.Size/2.0f))) + (Vector2(float32 x, float32 y) * this.Size),
                                                        Vector2(0.0f,1.0f), Vector2.Zero,
                                                        0.0f,
                                                        0.1f,
                                                        this.Tint,
                                                        (this.Size * 0.5f) + (float32(Helper.Rand.NextDouble()) * (this.Size * 0.8f)),
                                                        0.0f, 0.0f,
                                                        shape.[x,y])

            hitbox <- Rectangle(int((this.Position + ((Vector2(-3.0f,-3.0f) * this.Size) + (Vector2.One * -(this.Size)))).X),
                                 int((this.Position + ((Vector2(-3.0f,-3.0f) * this.Size) + (Vector2.One * -(this.Size)))).Y),
                                 int((Vector2(8.0f, 8.0f) * this.Size).X),
                                 int((Vector2(8.0f, 8.0f) * this.Size).Y))

            ProjectileManager.Instance.Projectiles.ForEach(fun p -> this.CheckCollision(p))

        member this.CheckCollision(p:Projectile) =
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
                                ParticleManager.Instance.Spawn(Rectangle(1,1,1,1), 
                                                        this.Position + ((Vector2(-3.0f,-3.0f) * this.Size) + (Vector2.One * -(this.Size/2.0f))) + (Vector2(float32 x, float32 y) * this.Size),
                                                        Vector2(-0.5f + (float32(Helper.Rand.NextDouble())), -2.0f), Vector2(0.0f,0.1f),
                                                        1000.0f,
                                                        0.01f,
                                                        this.Tint,
                                                        this.Size * 3.0f,
                                                        0.0f, -0.5f + (float32(Helper.Rand.NextDouble())),
                                                        1.0f)

        member this.Fire() =
            if gunCooldown <= 0.0f then
                gunCooldown <- gunCooldownTime
                ProjectileManager.Instance.Spawn(ProjectileOwner.Hero, this.Position + Vector2(0.0f, -40.0f), Vector2(0.0f, -10.0f), 2000.0f, Color.Red, 4.0f)

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