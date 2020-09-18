module Program
open FSharp.Text.Lexing

[<EntryPoint>]
let main argv =
    let parse program = 
        let lexbuf = LexBuffer<char>.FromString program
        let res = MicroCParser.start MicroCLexer.tokenize lexbuf
        res

    //a few parsing tests with simple and complex json
    //let program1 = "int f1; int f2; int input; int current; f1 := 0;f2 := 0; read input;"
    
    let program2 = "int f1; int f2; int input; int current; f1 := 0; f2 := 0; read input;
                       if(input == 0 | input == 1)
                       {
                           current := 1;
                       }
                       while(input > 1) {
                           current := f1+f2;
                           f2 := f1;
                           f1 := current;
                       }
                       write current;"
    //let res1 = parse program1;
    let res2 = parse program2
    printfn "%A" res2
    0