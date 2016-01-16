namespace SpirographBot

open NLog

module Processor =

    open SpiroParser
    open System
    open Messages
    open LinqToTwitter
    open Twitter
    open System.Text.RegularExpressions
    open Output
    open Storage

    let logger = LogManager.GetLogger "Processor"
    
    let removeBotHandle text = 
        Regex.Replace(text, "@SpirographBot", "", RegexOptions.IgnoreCase)

    let probablyQuery (text:string) =
        (text.Contains "turn" 
        || text.Contains "move"
        || text.Contains "polygon")

    let (|Query|Mention|) text =
        if probablyQuery text 
        then Query
        else Mention
        
    let processArguments recipient commands statusID =
        let repeat, cmds = commands
        async {
            logger.Info "Processing arguments"

            let mediaID = 
                createSpirograph recipient repeat cmds
                |> Twitter.uploadImage

            { RecipientName = recipient
              StatusID = statusID
              Message = "Is this the picture you were looking for?"
              MediaID = Some mediaID }
            |> Twitter.responsesAgent.Post }

    let respondTo (status:Status) =
        
        let recipient = status.User.ScreenNameResponse
        let statusID = status.StatusID
        let text = status.Text

        sprintf "respondTo %s %i %s" recipient statusID text
        |> logger.Info

        if (text.Contains "thanks for the attention!" || text.Contains "Is this the picture you were looking for?") then
            () // crude mechanism to stop self replying
        else
            match text with
            | Mention -> 
                { RecipientName = recipient
                  StatusID = statusID
                  Message = "thanks for the attention!"
                  MediaID = None }
                |> Twitter.responsesAgent.Post
            | Query ->
                let commands =
                    text
                    |> removeBotHandle
                    |> extractArguments

                match commands with
                | Fail(msg) ->
                    { RecipientName = recipient
                      StatusID = statusID
                      Message = "failed to parse your request: " + msg
                      MediaID = None }
                    |> Twitter.responsesAgent.Post
                | OK(rpt,cmds) -> 
                    let r,c = rpt
                    processArguments recipient (int c, cmds) statusID
                    |> Async.Start
                    |> ignore

    let rec loop (sinceID:uint64 Option) = async {
        
        logger.Info "Checking for new mentions"

        let mentions, nextID, delay = pullMentions sinceID

        nextID 
        |> Option.iter (Storage.updateLastMentionID)
        
        mentions 
        |> List.iter respondTo

        do! Async.Sleep (delay.TotalMilliseconds |> int)

        return! loop (nextID) }

type Bot () =

    let logger = LogManager.GetLogger "Bot"

    member this.Start () =
        
        logger.Info "Service starting"

        Twitter.setDescription ()

        Storage.readLastMentionID ()
        |> Processor.loop 
        |> Async.Start

    member this.Stop () =
        ()
        logger.Info "Service stopped"

