(* Any copyright is dedicated to the Public Domain.
 * http://creativecommons.org/publicdomain/zero/1.0/
 *)

module AgEitilt.Common.Stream.Test.ConcatStream

open FsCheck
open FsCheck.Xunit

open AgEitilt.Common.Stream

type Constructors () =
    [<Property(MaxTest = 1)>]
    static member ``The empty stream has length 0`` () =
        let c = new ConcatStream ()
        c.Length = 0L