﻿namespace FShoot

open System
open System.Collections
open System.Collections.Generic
open System.Linq
open Microsoft.Xna.Framework
open Microsoft.Xna.Framework.Graphics
open Microsoft.Xna.Framework.Content
open Microsoft.Xna.Framework.Audio
open Particles

module Text = 

    type TextManager() =
        let SFX = new List<SoundEffect>()
        let textData = array2D [|
                                    [|0;0;0;0;0;1;1;1;1;1;1;1;1;1;1;1;1;1;1;1;1;1;1;1;0;1;1;1;1;1;1;1;1;1;1;1;1;1;1;1;1;0;0;0;1;1;1;1;1;1;0;0;0;0;1;1;0;0;0;1;1;0;0;0;0;1;1;1;1;1;1;1;1;1;1;1;1;1;1;1;1;1;1;1;1;1;1;1;1;1;1;1;1;1;1;1;1;1;1;1;1;1;1;1;1;1;0;0;0;1;1;0;0;0;1;1;0;1;0;1;1;0;0;0;1;1;0;0;0;1;1;1;1;1;1;0;1;1;0;0;1;1;1;1;1;1;1;1;1;1;1;0;0;0;1;1;1;1;1;1;1;1;1;1;1;1;1;1;1;1;1;1;1;1;1;1;1;1;1;1;1;1;1;1;1;0;0;0;0;0;0;0;0;0;0;|];
                                    [|0;0;0;0;0;1;0;0;0;1;1;0;0;0;1;1;0;0;0;0;1;0;0;0;1;1;0;0;0;0;1;0;0;0;0;1;0;0;0;0;1;0;0;0;1;0;0;1;0;0;0;0;0;0;1;1;0;0;1;0;1;0;0;0;0;1;0;1;0;1;1;0;0;0;1;1;0;0;0;1;1;0;0;0;1;1;0;0;0;1;1;0;0;0;1;1;0;0;0;0;0;0;1;0;0;1;0;0;0;1;1;0;0;0;1;1;0;1;0;1;0;1;0;1;0;0;1;0;1;0;0;0;0;1;0;1;0;1;0;0;0;0;0;0;1;0;0;0;0;1;1;0;0;0;1;1;0;0;0;0;1;0;0;0;0;0;0;0;0;1;1;0;0;0;1;1;0;0;0;1;1;0;0;0;1;0;0;0;0;0;0;0;0;0;0;|];
                                    [|0;0;0;0;0;1;1;1;1;1;1;1;1;1;0;1;0;0;0;0;1;0;0;0;1;1;1;1;0;0;1;1;1;0;0;1;0;1;1;1;1;1;1;1;1;0;0;1;0;0;0;0;0;0;1;1;1;1;0;0;1;0;0;0;0;1;0;1;0;1;1;0;0;0;1;1;0;0;0;1;1;1;1;1;1;1;0;1;0;1;1;1;1;1;1;1;1;1;1;1;0;0;1;0;0;1;0;0;0;1;1;0;0;0;1;1;0;1;0;1;0;0;1;0;0;0;0;1;0;0;0;0;1;0;0;0;0;1;0;0;1;1;1;1;1;0;0;1;1;1;1;1;1;1;1;1;1;1;1;1;1;1;1;1;1;0;0;0;0;1;1;1;1;1;1;1;1;1;1;1;1;0;0;0;1;1;1;1;1;1;0;0;0;0;0;|];
                                    [|0;0;0;0;0;1;0;0;0;1;1;0;0;0;1;1;0;0;0;0;1;0;0;0;1;1;0;0;0;0;1;0;0;0;0;1;0;0;0;1;1;0;0;0;1;0;0;1;0;0;1;0;0;0;1;1;0;0;1;0;1;0;0;0;0;1;0;1;0;1;1;0;0;0;1;1;0;0;0;1;1;0;0;0;0;1;0;0;1;1;1;0;0;1;0;0;0;0;0;1;0;0;1;0;0;1;0;0;0;1;0;1;0;1;0;1;0;1;0;1;0;1;0;1;0;0;0;1;0;0;0;1;0;0;0;0;0;1;0;0;1;0;0;0;0;0;0;0;0;1;0;0;0;0;1;0;0;0;0;1;1;0;0;0;1;0;0;0;0;1;1;0;0;0;1;0;0;0;0;1;1;0;0;0;1;0;0;0;0;0;0;0;0;0;0;|];
                                    [|0;0;0;0;0;1;0;0;0;1;1;1;1;1;1;1;1;1;1;1;1;1;1;1;0;1;1;1;1;1;1;0;0;0;0;1;1;1;1;1;1;0;0;0;1;1;1;1;1;1;1;1;1;1;1;1;0;0;0;1;1;1;1;1;1;1;0;1;0;1;1;0;0;0;1;1;1;1;1;1;1;0;0;0;0;1;1;1;1;1;1;0;0;0;1;1;1;1;1;1;0;0;1;0;0;1;1;1;1;1;0;0;1;0;0;1;1;1;1;1;1;0;0;0;1;0;0;1;0;0;1;1;1;1;1;1;1;1;1;1;1;1;1;1;1;1;1;1;1;1;0;0;0;0;1;1;1;1;1;1;1;1;1;1;1;0;0;0;0;1;1;1;1;1;1;0;0;0;0;1;1;1;1;1;1;0;0;0;0;0;0;0;1;0;0;|];
                                |]
        let charMap = " ABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890-."
        static let instance = new TextManager()
        static member internal Instance = instance 


        member this.DrawText(pos:Vector2, text:string, size:float32, spacing:float32, padding:float32, tint:Color, life:float32, jitter:bool) =
            let totalsize = new Vector2(((float32 text.Length * 5.0f) * size) + ((float32 text.Length * spacing) - spacing) + ((float32 text.Length * padding) * 10.0f), (size * 5.0f) + (padding * 10.0f))
            let mutable position = (pos - (totalsize/2.0f)) + Vector2(size/2.0f,size/2.0f)
            for c in 0 .. text.Length-1 do
                let datax = charMap.IndexOf(text.[c]) * 5
                for y in 0..4 do
                    position.Y <- position.Y + padding
                    for x in 0..4 do
                        position.X <- position.X + padding
                        if textData.[y , x + datax] = 1 then
                            ParticleManager.Instance.Spawn(Rectangle(1,1,1,1), 
                                position,
                                Vector2.Zero, Vector2.Zero,
                                life,
                                0.1f,
                                tint,
                                (if jitter then (size * 0.5f) + (float32(Helper.Rand.NextDouble()) * (size * 0.8f)) else size),
                                0.0f, 0.0f,
                                1.0f)
                        position.X <- position.X + size + padding
                    position.X <- position.X - (5.0f * size) - (10.0f * padding)
                    position.Y <- position.Y + size + padding
                position.X <- position.X + (5.0f * size) + (10.0f * padding) + spacing
                position.Y <- position.Y - (5.0f * size) - (10.0f * padding)