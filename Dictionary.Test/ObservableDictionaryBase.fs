(* Any copyright is dedicated to the Public Domain.
 * http://creativecommons.org/publicdomain/zero/1.0/
 *)

module AgEitilt.Common.Dictionary.Test.ObservableDictionaryBase

open System.Collections.Generic
open System.Collections.Specialized
open System.ComponentModel

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
    let mutable newValuesReversed = Seq.empty<'b>
    let mutable oldValuesReversed = Seq.empty<'b option>

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

type Item =
    [<Property>]
    static member ``Set and retrieval returns the same item`` (dict : ObservableDictionaryBase<obj, obj>, key : NonNull<obj>, value : obj) =
        dict.[key] <- value
        dict.[key] = value
    
    (* BUG: Exceptions aren't properly lazy
    [<Property>]
    static member ``Nonexistent keys throw the proper exception`` (dict : ObservableDictionaryBase<obj, obj>, key : NonNull<obj>) =
        lazy dict.[key]
        |> Prop.throws<KeyNotFoundException, _>
    *)

    [<Property>]
    static member ``The proper events are triggered on setting values`` (dict : ObservableDictionaryBase<obj, obj>, keys : obj array, values : obj array) =
        // The generator doesn't generate empty dictionaries, which can throw
        // off the isAddition/isReplace logic
        dict.Clear ()

        // Add event tracker *after* the preprocessing
        let counts = Observer dict
        
        // Process the keys as these new lists are used in multiple places
        let filtered = keys |> Seq.filter (fun k -> not <| obj.ReferenceEquals (k, null))
        let grouped = filtered |> Seq.groupBy id

        // Target counts for various events
        let operations = filtered |> Seq.length
        let groups = grouped |> Seq.length
        let replacements = grouped |> Seq.map (fun (_, vs) -> Seq.length vs - 1) |> Seq.sum

        // Loop the values until there's enough for all keys in the list, then
        // add them to the dictionary
        match Array.length values with
        | 0   ->
                Seq.iter (fun k -> dict.[k] <- null) filtered
        | len when len < operations ->
                Seq.iter2 (fun k v -> dict.[k] <- v) filtered values
        | len ->
                System.Linq.Enumerable.Repeat (values, (operations / len) + 1) |>
                Seq.collect id |>
                Seq.iter2 (fun k v -> dict.[k] <- v) filtered

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
