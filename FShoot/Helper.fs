namespace FShoot

open System

type Helper() =
    static let rnd = new Random()
    static member internal Rand = rnd
