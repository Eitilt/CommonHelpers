(* Any copyright is dedicated to the Public Domain.
 * http://creativecommons.org/publicdomain/zero/1.0/
 *)

module AgEitilt.Common.Stream.Test.Generators

open System
open System.IO

open FsCheck

//TODO: Implement streams randomly closing (!CanRead && !CanWrite) or
// returning/accepting less bytes than expected, alongside CanTimeout
//TODO: Implement streams changing their contents unexpectedly?
type TestStream (readable, seekable, writable, bytes) =
    // Need to use this form to get an expandable MemoryStream, otherwise the
    // test behaviour becomes too unpredictable
    inherit MemoryStream (Array.length bytes)

    let buffer = bytes
    // Allow initializing the array even if writable = false
    let mutable inInit = true
    do
        base.Write(bytes, 0, Array.length bytes)
        base.Position <- 0L
        inInit <- false

    override s.CanRead = readable
    override s.CanSeek = seekable
    override s.CanWrite = writable || inInit

    override s.ToString () =
        sprintf "%s, %s, %s : %A"
        <| if s.CanRead then "read" else "____"
        <| if s.CanSeek then "seek" else "____"
        <| if s.CanWrite then "write" else "_____"
        <| buffer

    override s.Length =
        if s.CanSeek then
            base.Length
        else
            raise <| new NotSupportedException "Seeking has been disabled"

    override s.Position =
        if s.CanSeek then
            base.Position
        else
            raise <| new NotSupportedException "Seeking has been disabled"

    override s.Read (buffer, offset, count) =
        if s.CanRead then
            base.Read (buffer, offset, count)
        else
            raise <| new NotSupportedException "Reading has been disabled"

    override s.ReadAsync (buffer, offset, count, token) =
        if s.CanRead then
            base.ReadAsync (buffer, offset, count, token)
        else
            raise <| new NotSupportedException "Reading has been disabled"

    override s.ReadByte () =
        if s.CanRead then
            base.ReadByte ()
        else
            raise <| new NotSupportedException "Reading has been disabled"

    override s.Seek (offset, origin) =
        if s.CanSeek then
            base.Seek (offset, origin)
        else
            raise <| new NotSupportedException "Seeking has been disabled"

    override s.SetLength len =
        if not s.CanSeek then
            raise <| new NotSupportedException "Seeking has been disabled"
        else if not s.CanWrite then
            raise <| new NotSupportedException "Writing has been disabled"
        else
            base.SetLength len

    override s.Write (buffer, offset, count) =
        if s.CanWrite then
            base.Write (buffer, offset, count)
        else
            raise <| new NotSupportedException "Writing has been disabled"

    override s.WriteAsync (buffer, offset, count, token) =
        if s.CanWrite then
            base.WriteAsync (buffer, offset, count, token)
        else
            raise <| new NotSupportedException "Writing has been disabled"

    override s.WriteByte byte =
        if s.CanWrite then
            base.WriteByte byte
        else
            raise <| new NotSupportedException "Writing has been disabled"



    static member Generate r s w =
        Gen.apply
        <| gen { return fun bs ->
                new TestStream (r, s, w, bs) :> Stream
            }
        <| Gen.sized (fun len ->
                Arb.generate<byte>
                |> Gen.arrayOfLength len
            )

type ReadableStream = ReadableStream of Stream with
    static member ToStream (ReadableStream s) = s

type SeekableStream = SeekableStream of Stream with
    static member ToStream (SeekableStream s) = s

type WritableStream = WritableStream of Stream with
    static member ToStream (WritableStream s) = s

type StreamGenerators =
    static member Stream () =
        [ TestStream.Generate true  true  true
        ; TestStream.Generate true  true  false
        ; TestStream.Generate true  false true
        ; TestStream.Generate false true  true
        ; TestStream.Generate false false true
        ; TestStream.Generate false true  false
        ; TestStream.Generate true  false false
        ; TestStream.Generate false false false
        ]
        |> Gen.oneof
        |> Arb.fromGen

    static member ReadableStream () =
        [ TestStream.Generate true  true  true
        ; TestStream.Generate true  true  false
        ; TestStream.Generate true  false true
        ; TestStream.Generate true  false false
        ]
        |> Gen.oneof
        |> Arb.fromGen
        |> Arb.convert ReadableStream ReadableStream.ToStream

    static member SeekableStream () =
        [ TestStream.Generate true  true  true
        ; TestStream.Generate true  true  false
        ; TestStream.Generate false true  true
        ; TestStream.Generate false true  false
        ]
        |> Gen.oneof
        |> Arb.fromGen
        |> Arb.convert SeekableStream SeekableStream.ToStream

    static member WritableStream () =
        [ TestStream.Generate true  true  true
        ; TestStream.Generate true  false true
        ; TestStream.Generate false true  true
        ; TestStream.Generate false false true
        ]
        |> Gen.oneof
        |> Arb.fromGen
        |> Arb.convert WritableStream WritableStream.ToStream

let initGenerators () =
    Arb.register<StreamGenerators> () |> ignore
