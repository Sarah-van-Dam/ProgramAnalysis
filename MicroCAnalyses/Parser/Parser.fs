module Parser
open FSharp.Text.Lexing
let parse program = 
        let lexbuf = LexBuffer<char>.FromString program
        let res = MicroCParser.start MicroCLexer.tokenize lexbuf
        res