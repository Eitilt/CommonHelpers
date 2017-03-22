(* Any copyright is dedicated to the Public Domain.
 * http://creativecommons.org/publicdomain/zero/1.0/
 *)

open FsCheck

open AgEitilt.Common.Stream.Test.Generators

[<EntryPoint>]
let main argv =
    initGenerators ()

    Gen.sample 5 5 Arb.generate<ReadableStream>
    |> printfn "%A"
    
    printfn "Press any key to close..."
    System.Console.ReadKey () |> ignore

    0 // return an integer exit code
