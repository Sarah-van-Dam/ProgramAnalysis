module MicroCTypes

type arithmeticExpression = 
    | Number of int
    | Plus of arithmeticExpression * arithmeticExpression
    | Minus of arithmeticExpression * arithmeticExpression
    | Multiply of arithmeticExpression * arithmeticExpression
    | Divide of arithmeticExpression * arithmeticExpression
    | Modulo of arithmeticExpression * arithmeticExpression
    | Power of arithmeticExpression * arithmeticExpression
    | Variable of string
    | ArrayMember of string * arithmeticExpression
    | RecordMember of string * int


type booleanExpression =
    | True
    | False
    | And of booleanExpression * booleanExpression
    | Or of booleanExpression * booleanExpression
    | Not of booleanExpression
    | Equal of arithmeticExpression * arithmeticExpression
    | NotEqual of arithmeticExpression * arithmeticExpression
    | Great of arithmeticExpression * arithmeticExpression
    | GreatEqual of arithmeticExpression * arithmeticExpression
    | Less of arithmeticExpression * arithmeticExpression
    | LessEqual of arithmeticExpression * arithmeticExpression
    
type statement = 
    | AssignVariable of string * arithmeticExpression
    | AssignRecord of string * arithmeticExpression * arithmeticExpression
    | AssignArray of string * arithmeticExpression * arithmeticExpression
    | AssignRecordMember of string * arithmeticExpression * int
    | Read of string
    | Write of string
    | While of booleanExpression * statement
    | If of booleanExpression * statement 
    | IfElse of booleanExpression * statement * statement
    | ContinuedStatement of statement * statement

type declaration = 
    | IntegerDeclaration of string
    | ArrayDeclaration of string * int
    | RecordDeclaration of string
    | ContinuedDeclaration of declaration * declaration
    

type expressionTree =
    | DS of declaration * statement
    | S of statement
    







