(* Any copyright is dedicated to the Public Domain.
 * http://creativecommons.org/publicdomain/zero/1.0/
 *)

module AgEitilt.Common.Dictionary.Test.ArrayKeys

open FsCheck
open FsCheck.Xunit

open AgEitilt.Common.Dictionary

let checkMultiple (len : PositiveInt) f x =
    match List.init len.Get (fun _ -> f x) with
    | []     -> true
    | h :: t -> Seq.fold (fun (eq, last) this -> (eq && this = last, this)) (true, h) t
                |> fst
                

type Equals =
    [<Property>]
    static member ``Values equal themselves`` (c : ArrayEqualityComparer<obj>, xs : obj array) =
        c.Equals (xs, xs)

    [<Property>]
    static member ``Arrays with the same elements equal each other`` (c : ArrayEqualityComparer<obj>, xs : obj array) =
        c.Equals (xs, Array.copy xs)
    
    [<Property>]
    static member ``Values don't equal others`` (c : ArrayEqualityComparer<obj>, xs : obj array, ys : obj array) =
        ((    xs.Length <> ys.Length
           || Array.forall2 (<>) xs ys
         ) && xs.Length > 0  // forall2 apparently defaults to `true`
        ) ==> not (c.Equals (xs, ys))

    [<Property(MaxTest = 1)>]
    static member ``null values are equal to other nulls`` (c : ArrayEqualityComparer<obj>) =
        c.Equals(null, null)

    [<Property>]
    static member ``null values aren't equal to any array`` (c : ArrayEqualityComparer<obj>, xs : obj array) =
        // Checks that neither orientation is true
           c.Equals (xs, null)
        || c.Equals (null, xs)
        |> not

    [<Property>]
    static member ``Calling multiple times has the same result`` (len : PositiveInt, c : ArrayEqualityComparer<obj>, xs : obj array, ys : obj array) =
        checkMultiple len c.Equals (xs, ys)

type HashCode =
    [<Property>]
    static member ``Values always have the same hash`` (c1 : ArrayEqualityComparer<obj>, c2 : ArrayEqualityComparer<obj>, xs : obj array) =
        c1.GetHashCode xs = c2.GetHashCode xs
    
    [<Property>]
    static member ``Arrays with the same elements have the same hash`` (c : ArrayEqualityComparer<obj>, xs : obj array) =
        c.GetHashCode xs = c.GetHashCode (Array.copy xs)
    
    [<Property(MaxTest = 1)>]
    static member ``null objects always have the same hash`` (c1 : ArrayEqualityComparer<obj>, c2 : ArrayEqualityComparer<obj>) =
        c1.GetHashCode(null) = c2.GetHashCode(null)

    [<Property>]
    static member ``Calling multiple times has the same result`` (len : PositiveInt, c : ArrayEqualityComparer<obj>, xs : obj array) =
        checkMultiple len c.GetHashCode xs