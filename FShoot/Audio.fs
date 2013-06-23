namespace FShoot

open System
open System.Collections
open System.Collections.Generic
open System.Linq
open Microsoft.Xna.Framework
open Microsoft.Xna.Framework.Graphics
open Microsoft.Xna.Framework.Content
open Microsoft.Xna.Framework.Audio

module Audio = 

    type AudioManager() =
        let SFX = new Dictionary<string, SoundEffect>()
        static let instance = new AudioManager()
        static member internal Instance = instance 

        member this.LoadContent(content:ContentManager) = 
            SFX.Add("shoot", content.Load<SoundEffect>("sfx/shoot"))
            SFX.Add("enemyshoot", content.Load<SoundEffect>("sfx/enemyshoot"))
            SFX.Add("powerup", content.Load<SoundEffect>("sfx/powerup"))
            SFX.Add("enemyhit", content.Load<SoundEffect>("sfx/enemyhit"))
            SFX.Add("enemydie", content.Load<SoundEffect>("sfx/enemydie"))
            SFX.Add("newwave", content.Load<SoundEffect>("sfx/newwave"))
            SFX.Add("playerhit", content.Load<SoundEffect>("sfx/playerhit"))
            SFX.Add("playerdie", content.Load<SoundEffect>("sfx/playerdie"))
            SFX.Add("shieldhit", content.Load<SoundEffect>("sfx/shieldhit"))

        member this.Play(name) =
            this.Play(name, 1.0f)
        member this.Play(name, vol) =
            this.Play(name, vol, 0.0f)
        member this.Play(name, vol, pitch) =
            this.Play(name, vol, pitch, 0.0f)
        member this.Play(name, vol, pitch, pan) =
            SFX.[name].Play(vol, pitch, pan)

        

   




