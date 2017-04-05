(* Any copyright is dedicated to the Public Domain.
 * http://creativecommons.org/publicdomain/zero/1.0/
 *)

open FsCheck

open AgEitilt.Common.Dictionary.Test

[<EntryPoint>]
let main argv =
    Generators.initGenerators ()

    Check.QuickAll<ArrayKeys.Equals> ()
    Check.QuickAll<ArrayKeys.HashCode> ()
    
    Check.QuickAll<DictionaryExtension.GetOrCreate> ()

    Check.QuickAll<ObservableDictionaryBase.Item> ()
    Check.QuickAll<ObservableDictionaryBase.Keys> ()

    printfn "Press any key to close..."
    System.Console.ReadKey () |> ignore

    0 // return an integer exit code
