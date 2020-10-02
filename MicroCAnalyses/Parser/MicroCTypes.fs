module MicroCTypes

type aExpr = 
    | N of int
    | Plus of aExpr * aExpr
    | Minus of aExpr * aExpr
    | Multiply of aExpr * aExpr
    | Divide of aExpr * aExpr
    | Modulo of aExpr * aExpr
    | Pow of aExpr * aExpr
    | Var of string
    | Arr of string * aExpr
    | RecordEntry of string * int


type bExpr =
    | True
    | False
    | And of bExpr * bExpr
    | Or of bExpr * bExpr
    | Not of bExpr
    | Eq of aExpr * aExpr
    | Neq of aExpr * aExpr
    | Great of aExpr * aExpr
    | Ge of aExpr * aExpr
    | Less of aExpr * aExpr
    | Le of aExpr * aExpr
    
type statement = 
    | AssignVariable of string * aExpr
    | AssignRecord of string * aExpr * aExpr
    | AssignArray of string * aExpr * aExpr
    | AssignRecordMember of string * aExpr * int
    | Read of string
    | Write of string
    | While of bExpr * statement
    | If of bExpr * statement 
    | IfE of bExpr * statement * statement
    | ContinuedS of statement * statement

type declaration = 
    | IntegerD of string
    | ArrD of string * int
    | RecordD of string
    | ContinuedD of declaration * declaration
    

type expr =
    | DS of declaration * statement
    | S of statement
    







