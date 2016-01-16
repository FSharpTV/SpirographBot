namespace SpirographBot

module SpiroParser =
    open FParsec

    let reservedChars = set [';';'[';']';'*']

    let basicString =
        let normalChar = satisfy (reservedChars.Contains >> not)
        spaces >>. manyChars (normalChar)

    let pRepeat =
        pstring "repeat" .>> spaces .>>. basicString

    let pTurn =
         pstring "turn" .>> spaces .>>. basicString

    let pMove =
        pstring "move" .>> spaces .>>. basicString

    let pPolygon =
        pstring "polygon" .>> spaces .>>. basicString

    let pCmd = (pTurn <|> pMove <|> pPolygon)

    let pCmdList =
        pstring "[" >>. sepBy pCmd (pstring ";") .>> pstring "]" .>> spaces
    
    let trim (s:string) = s.Trim ()

    let pArgs = 
        tuple2
            (spaces >>. pRepeat .>> (pchar '*' >>. spaces)) 
            (pCmdList) 

    type Res<'a> =
        | OK of 'a 
        | Fail of string

    let extractArguments (exp:string) =         
        match (run pArgs exp) with
        | Success(arg,_,_) -> OK(arg)
        | Failure(msg,_,_) -> Fail(msg)
