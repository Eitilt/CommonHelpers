(* Any copyright is dedicated to the Public Domain.
 * http://creativecommons.org/publicdomain/zero/1.0/
 *)

module AgEitilt.Common.Dictionary.Test.ObservableDictionaryBase

open System.Collections.Generic
open System.Collections.Specialized
open System.ComponentModel

open Xunit
open FsCheck
open FsCheck.Xunit

open AgEitilt.Common.Dictionary

type Observer<'a, 'b> (dict : ObservableDictionaryBase<'a, 'b>) =
    // Types of NotifyCollectionChangedAction
    let mutable addChanges                    = 0
    let mutable removeChanges                 = 0
    let mutable replaceChanges                = 0
    let mutable moveChanges                   = 0
    let mutable resetChanges                  = 0
    let mutable unrecognizedCollectionChanges = 0

    // Known properties on ObservableDictionaryBase
    let mutable keyChanges                    = 0
    let mutable valuesChanges                 = 0
    let mutable countChanges                  = 0
    let mutable unrecognizedPropertyChanges   = 0

    //TODO: Implement assignment and then add to tests
    let mutable newValuesReversed = Seq.empty<KeyValuePair<'a, 'b>>
    let mutable oldValuesReversed = Seq.empty<KeyValuePair<'a, 'b> option>

    do
        fun (args : NotifyCollectionChangedEventArgs) ->
            match args.Action with
            | NotifyCollectionChangedAction.Add     -> addChanges     <- addChanges + 1
            | NotifyCollectionChangedAction.Remove  -> removeChanges  <- removeChanges + 1
            | NotifyCollectionChangedAction.Replace -> replaceChanges <- replaceChanges + 1
            | NotifyCollectionChangedAction.Move    -> moveChanges    <- moveChanges + 1
            | NotifyCollectionChangedAction.Reset   -> resetChanges   <- resetChanges + 1
            | _ -> unrecognizedCollectionChanges                      <- unrecognizedCollectionChanges + 1
        |> dict.CollectionChanged.Add

        fun (args : PropertyChangedEventArgs) ->
            match args.PropertyName with
            | "Keys"   -> keyChanges           <- keyChanges + 1
            | "Values" -> valuesChanges        <- valuesChanges + 1
            | "Count"  -> countChanges         <- countChanges + 1
            | _ -> unrecognizedPropertyChanges <- unrecognizedPropertyChanges + 1
        |> dict.PropertyChanged.Add

    member this.AddChanges                    with get () = addChanges
    member this.RemoveChanges                 with get () = removeChanges
    member this.ReplaceChanges                with get () = replaceChanges
    member this.MoveChanges                   with get () = moveChanges
    member this.ResetChanges                  with get () = resetChanges
    member this.UnrecognizedCollectionChanges with get () = unrecognizedCollectionChanges

    member this.KeyChanges                    with get () = keyChanges
    member this.ValuesChanges                 with get () = valuesChanges
    member this.CountChanges                  with get () = countChanges
    member this.UnrecognizedPropertyChanges   with get () = unrecognizedPropertyChanges

let isZeroProperty s v =
    (v = 0) |@ sprintf "%s: %d" s v
let equalProperty s v w =
    (v = w) |@ sprintf "%s: %d (want %d)" s v w

//BUG: Not being run
type ObservableDictionaryBaseFixture () =
    do
        Arb.register<Generators.DictionaryGenerator> () |> ignore
[<CollectionDefinition("ObservableDictionaryBase")>]
type ObservableDictionaryBaseCollection () =
    interface ICollectionFixture<ObservableDictionaryBaseFixture>

[<Collection("ObservableDictionaryBase")>]
type Item =
    [<Property>]
    static member ``Set and retrieval returns the same item``
            (dict : ObservableDictionaryBase<obj, obj>, key : NonNull<obj>, value : obj) =
        dict.[key.Get] <- value
        dict.[key.Get] = value
    
    (* BUG: Exceptions aren't properly lazy
    [<Property>]
    static member ``Nonexistent keys throw the proper exception``
            (dict : ObservableDictionaryBase<obj, obj>, key : NonNull<obj>) =
        lazy dict.[key]
        |> Prop.throws<KeyNotFoundException, _>
    *)

    [<Property>]
    static member ``The proper events are triggered on setting values``
            (dict : ObservableDictionaryBase<obj, obj>, keys : NonNull<obj> array) =
        // The generator doesn't generate empty dictionaries, which can throw
        // off the isAddition/isReplace logic
        //dict.Clear ()

        // Add event tracker *after* the preprocessing
        let counts = Observer dict
        
        // Process the keys as these new lists are used in multiple places
        let filtered = keys |> Seq.map (fun k -> k.Get)
        let grouped = filtered |> Seq.groupBy id

        // Target counts for various events
        let operations = filtered |> Seq.length
        let groups = grouped |> Seq.length
        let replacements = grouped |> Seq.map (fun g -> Seq.length (snd g) - 1) |> Seq.sum

        // Generate a random value for each key, then add each pair
        Gen.sample 4 operations Arb.generate<obj>
        |> Seq.iter2 (fun k v -> dict.[k] <- v) filtered

        // Compare the actual counts to the expected numbers
        operations > 0 ==>
        "CollectionChanged events" @| (
                equalProperty  "correct number of additions"         counts.AddChanges     groups
            .&. isZeroProperty "no removals"                         counts.RemoveChanges
            .&. equalProperty  "correct number of replacements"      counts.ReplaceChanges replacements
            .&. isZeroProperty "no moves"                            counts.MoveChanges
            .&. isZeroProperty "no resets"                           counts.ResetChanges
            .&. isZeroProperty "no unrecognized collection events"   counts.UnrecognizedCollectionChanges
        ) .&.
        "PropertyChanged events" @| (
                equalProperty  "correct number of changes to Keys"   counts.KeyChanges     groups
            .&. equalProperty  "correct number of changes to Values" counts.ValuesChanges  operations
            .&. equalProperty  "correct number of changes to Count"  counts.CountChanges   groups
            .&. isZeroProperty "no unrecognized property events"     counts.UnrecognizedPropertyChanges
        )

[<Collection("ObservableDictionaryBase")>]
type Keys =
    //TODO: This doesn't check replacing values
    [<Property>]
    static member ``Keys contains all added keys (and nothing else if initially empty)``
            (dict : ObservableDictionaryBase<obj, obj>, keys : NonNull<obj> array, values : obj array) =
        // The generator doesn't generate empty dictionaries, which can throw
        // off the checking logic
        //dict.Clear ()

        // Remove duplicate keys as .Add can't handle replacement
        let grouped =
            keys
            |> Seq.map (fun k -> k.Get)
            |> Seq.groupBy id
            |> Seq.map fst

        // Number of unique keys in the list
        let groups = grouped |> Seq.length

        // Be sure there are at least as many values as there are keys
        match Seq.length grouped, Array.length values with
        | k, 0            -> Array.zeroCreate k
        | k, v when k < v -> values
        | k, v            -> Array.create (k / v + 1) values
                             |> Array.collect id
        // Additionally, randomly switch between methods of adding values
        |> Seq.zip <| Gen.sample 2 groups Arb.generate<bool>
        |> Seq.iter2 (fun k (v, b) -> if b then dict.[k] <- v else dict.Add (k, v)) grouped

        groups > 0 ==> lazy
          // Compare the returned keys to what was put in
        ( grouped
          |> Seq.map (fun k -> dict.Keys |> Seq.exists ((=) k))
          |> Seq.reduce (&&)
          // If the length isn't greater than the number of groups, there
          // can't be extra keys beyond what we put in
          .&. (groups = Seq.length dict.Keys)
        )
    
    [<Property>]
    static member ``Replacing items doesn't change the set of keys``
            (dict : ObservableDictionaryBase<obj, obj>, keys : NonNull<obj> array, values : obj array) =
        // The generator doesn't generate empty dictionaries, which can throw
        // off the checking logic
        //dict.Clear ()

        // Prep the dictionary to have all elements at all unique keys 
        let grouped =
            keys
            |> Array.map (fun k -> k.Get)
            |> Seq.groupBy id
            |> Seq.map fst
        let len = Seq.length grouped

        // Be sure there are at least as many values as there are keys
        match len, Array.length values with
        | k, 0            -> Array.zeroCreate k
        | k, v when k < v -> values
        | k, v            -> Array.create (k / v + 1) values
                             |> Array.collect id
        |> Seq.iter2 (fun k v -> dict.[k] <- v) grouped

        len > 0 ==> lazy (
            // Assign to some keys multiple times and others not at all
            Gen.elements keys
            |> Gen.sample len len
            |> Seq.iter2 (fun v k -> dict.[k.Get] <- v) values

            // Compare the returned keys to what was put in
            grouped
            |> Seq.map (fun k -> dict.Keys |> Seq.exists ((=) k))
            |> Seq.reduce (&&)
        )
        // If the length isn't greater than the number of groups, there
        // can't be extra keys beyond what we started with
        .&. (Seq.length grouped = Seq.length dict.Keys)

    [<Property>]
    static member ``Keys aren't included in the list after removal``
            (dict : ObservableDictionaryBase<obj, obj>, keys : NonNull<obj> array, values : obj array) =
        // Prep the dictionary to have all elements at all unique keys 
        let grouped =
            keys
            |> Array.map (fun k -> k.Get)
            |> Seq.groupBy id
            |> Seq.map fst
        let len = Seq.length grouped

        // Be sure there are at least as many values as there are keys
        match len, Array.length values with
        | k, 0            -> Array.zeroCreate k
        | k, v when k < v -> values
        | k, v            -> Array.create (k / v + 1) values
                             |> Array.collect id
        |> Seq.iter2 (fun k v -> dict.[k] <- v) grouped

        len > 0 ==> lazy (
            // Take a random subset of keys to remove
            let removed =
                grouped
                |> Gen.elements
                |> Gen.sample len len
                |> Seq.groupBy id
                |> Seq.map fst

            removed
            |> Seq.iter (fun k -> dict.Remove k |> ignore)

            // Check for any of the removed keys still being in the list
            removed
            |> Seq.map (fun k -> dict.Keys |> Seq.exists ((=) k))
            |> Seq.reduce (&&)
            |> not
        )
