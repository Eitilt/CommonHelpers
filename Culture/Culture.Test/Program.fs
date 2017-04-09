﻿(* Any copyright is dedicated to the Public Domain.
 * http://creativecommons.org/publicdomain/zero/1.0/
 *)

open FsCheck

open AgEitilt.Common.Stream.Test

[<EntryPoint>]
let main argv =
    let culture = System.Globalization.CultureInfo "en"
    System.Console.WriteLine culture.DisplayName
    
    printfn "Press any key to close..."
    System.Console.ReadKey () |> ignore

    0 // return an integer exit code
