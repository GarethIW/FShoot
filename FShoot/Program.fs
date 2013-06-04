// Learn more about F# at http://fsharp.net
// See the 'F# Tutorial' project for more help.
namespace FShoot

module main = 
    [<EntryPoint>]
    let main args = 
        let game = new FShootGame()
        game.Run()
        0



