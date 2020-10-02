module Parser
open FSharp.Text.Lexing
let parse program : MicroCTypes.expressionTree = 
        let lexbuf = LexBuffer<char>.FromString program
        let res = MicroCParser.start MicroCLexer.tokenize lexbuf
        res