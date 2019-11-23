namespace Model

open FSharp.Data.Adaptive
open Adaptify

// MODEL
[<ModelType>]
type Entry =
   { description : string
     completed : bool
     editing : bool }

// The full application state of our todo app.
[<ModelType>]
type Model = 
   { entries : IndexList<Entry>
     field : string
     visibility : string }