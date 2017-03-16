(* Any copyright is dedicated to the Public Domain.
 * http://creativecommons.org/publicdomain/zero/1.0/
 *)

module ConcatStream.Test.ConcatStream

open System.IO

open ConcatStream

type Constructors () =
    let ``The empty stream has length 0`` () =
        let c = new ConcatStream ()
        c.Length = 0