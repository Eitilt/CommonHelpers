module ConcatStream.Test.ConcatStream

open System.IO

open ConcatStream

type Constructors () =
    let ``The empty stream has length 0`` () =
        let c = new ConcatStream ()
        c.Length = 0