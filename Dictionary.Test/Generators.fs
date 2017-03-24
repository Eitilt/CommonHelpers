(* Any copyright is dedicated to the Public Domain.
 * http://creativecommons.org/publicdomain/zero/1.0/
 *)

module AgEitilt.Common.Dictionary.Test.Generators

open FsCheck

open AgEitilt.Common.Dictionary

type ComparerGenerator =
    static member ArrayComparer () =
        gen {
            return! Gen.elements <| 
            [ ArrayEqualityComparer<'a> ()
            ]
        } |> Arb.fromGen


let ObservableDictionaryGen () =
    gen {
        return! Gen.elements <|
        (*BUG: This doesn't actually generate empty dictionaries. That's often
         * not what's desired, but should fix to not be unexpected (and then
         * add deliberately-non-empty constructors)
         *)
        [ ObservableDictionary<'a, 'b> ()
        ]
    }
    
let UpcastDictionaryGen () =
    gen {
        return fun d -> d :> ObservableDictionaryBase<'a, 'b>
    }

type DictionaryGenerator =
    static member ObservableDictionary () =
        Arb.fromGen <| ObservableDictionaryGen ()

    static member ObservableDictionaryBase () =
        [ ObservableDictionaryGen () |> Gen.apply (UpcastDictionaryGen ())
        ] |> Gen.oneof
        |> Arb.fromGen

let initGenerators () =
    Arb.register<ComparerGenerator> () |> ignore
    Arb.register<DictionaryGenerator> () |> ignore
