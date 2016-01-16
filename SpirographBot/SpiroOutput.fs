namespace SpirographBot

module Output =

    open System.Drawing
    open System.IO
    open Messages
    open Plotting

    let createSpirograph (recipient:string) (repeat:int) commands =
        let initialPlotter = 
            { Position  = 750,750
              Color     = Color.Red
              Direction = 0.0
              Bitmap    = new Bitmap(2000,2000) 
            }
        
        let parseCommands (cmds:List<string*string>) = 
            let cmdAndValsTuples = 
                cmds
                |> Seq.map (fun (f,s) -> f.ToUpper(),s)
            
            let parsePolygon (values:string) =
                let vals = values.Split [|','|] |> Array.map int
                polygon vals.[0] vals.[1]

            let matcher cmd =
                match (cmd) with
                        | (c,v) when c = "TURN" -> turn (float v)
                        | (c,v) when c = "MOVE" -> move (int v)
                        | (c,v) when c = "POLYGON" -> parsePolygon v
                        | _ -> failwith "implement me"

            Seq.map matcher cmdAndValsTuples
            |> Seq.toList

        let path = Path.Combine(Directory.GetCurrentDirectory(), recipient+".png")
        let fileName = recipient+".png"
        let stripe = parseCommands commands
    
        generate stripe repeat initialPlotter
        |> saveAs fileName

