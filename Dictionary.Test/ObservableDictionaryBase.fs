(* Any copyright is dedicated to the Public Domain.
 * http://creativecommons.org/publicdomain/zero/1.0/
 *)

module AgEitilt.Common.Dictionary.Test.ObservableDictionaryBase

open FsCheck
open FsCheck.Xunit

open AgEitilt.Common.Dictionary

type Item =
    [<Property>]
    static member ``Set and retrieval returns the same item`` (dict : ObservableDictionary<obj, obj>, key : obj, value : obj) =
        dict.[key] <- value
        dict.[key] = value