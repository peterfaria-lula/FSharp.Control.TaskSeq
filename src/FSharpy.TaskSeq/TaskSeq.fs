﻿namespace FSharpy.TaskSeq

open System.Collections.Generic
open System.Threading
open System.Threading.Tasks

module TaskSeq =
    open TaskSeq

    /// Returns taskSeq as an array. This function is blocking until the sequence is exhausted.
    let toList (t: taskSeq<'T>) = [
        let e = t.GetAsyncEnumerator(CancellationToken())

        try
            while (let vt = e.MoveNextAsync() in if vt.IsCompleted then vt.Result else vt.AsTask().Result) do
                yield e.Current
        finally
            e.DisposeAsync().AsTask().Wait()
    ]


    /// Returns taskSeq as an array. This function is blocking until the sequence is exhausted.
    let toArray (taskSeq: taskSeq<'T>) = [|
        let e = taskSeq.GetAsyncEnumerator(CancellationToken())

        try
            while (let vt = e.MoveNextAsync() in if vt.IsCompleted then vt.Result else vt.AsTask().Result) do
                yield e.Current
        finally
            e.DisposeAsync().AsTask().Wait()
    |]

    let empty<'T> = taskSeq {
        for c: 'T in [] do
            yield c
    }

    let ofArray (array: 'T[]) = taskSeq {
        for c in array do
            yield c
    }

    let ofList (list: 'T list) = taskSeq {
        for c in list do
            yield c
    }

    let ofSeq (sequence: 'T seq) = taskSeq {
        for c in sequence do
            yield c
    }

    let ofResizeArray (data: 'T ResizeArray) = taskSeq {
        for c in data do
            yield c
    }

    let ofTaskSeq (sequence: #Task<'T> seq) = taskSeq {
        for c in sequence do
            let! c = c
            yield c
    }

    let ofTaskList (list: #Task<'T> list) = taskSeq {
        for c in list do
            let! c = c
            yield c
    }

    let ofTaskArray (array: #Task<'T> array) = taskSeq {
        for c in array do
            let! c = c
            yield c
    }

    let ofAsyncSeq (sequence: Async<'T> seq) = taskSeq {
        for c in sequence do
            let! c = task { return! c }
            yield c
    }

    let ofAsyncList (list: Async<'T> list) = taskSeq {
        for c in list do
            let! c = Task.ofAsync c
            yield c
    }

    let ofAsyncArray (array: Async<'T> array) = taskSeq {
        for c in array do
            let! c = Async.toTask c
            yield c
    }

    /// Unwraps the taskSeq as a Task<array<_>>. This function is non-blocking.
    let toArrayAsync taskSeq =
        Internal.toResizeArrayAsync taskSeq
        |> Task.map (fun a -> a.ToArray())

    /// Unwraps the taskSeq as a Task<list<_>>. This function is non-blocking.
    let toListAsync taskSeq = (Internal.toResizeArrayAsync >> Task.map List.ofSeq) taskSeq

    /// Unwraps the taskSeq as a Task<ResizeArray<_>>. This function is non-blocking.
    let toResizeArrayAsync taskSeq =
        Internal.toResizeArrayAsync taskSeq
        |> Task.map (fun a -> a.ToArray())

    /// Unwraps the taskSeq as a Task<IList<_>>. This function is non-blocking.
    let toIListAsync taskSeq =
        (Internal.toResizeArrayAsync
         >> Task.map (fun x -> x :> IList<_>))
            taskSeq

    /// Unwraps the taskSeq as a Task<seq<_>>. This function is non-blocking,
    /// exhausts the sequence and caches the results of the tasks in the sequence.
    let toSeqCachedAsync taskSeq =
        (Internal.toResizeArrayAsync
         >> Task.map (fun x -> x :> seq<_>))
            taskSeq

    /// Iterates over the taskSeq. This function is non-blocking
    /// exhausts the sequence as soon as the task is evaluated.
    let iterAsync action taskSeq = Internal.iteriAsync (fun _ -> action) taskSeq

    /// Iterates over the taskSeq. This function is non-blocking,
    /// exhausts the sequence as soon as the task is evaluated.
    let iteriAsync action taskSeq = Internal.iteriAsync action taskSeq

    /// Maps over the taskSeq. This function is non-blocking.
    let map (mapper: 'T -> 'U) taskSeq = Internal.mapi (fun _ -> mapper) taskSeq

    /// Maps over the taskSeq with an index. This function is non-blocking.
    let mapi (mapper: int -> 'T -> 'U) taskSeq = Internal.mapi mapper taskSeq

    /// Maps over the taskSeq. This function is non-blocking.
    let mapAsync (mapper: 'T -> Task<'U>) taskSeq = Internal.mapiAsync (fun _ -> mapper) taskSeq

    /// Maps over the taskSeq with an index. This function is non-blocking.
    let mapiAsync (mapper: int -> 'T -> Task<'U>) taskSeq = Internal.mapiAsync mapper taskSeq

    /// Applies the given function to the items in the taskSeq and concatenates all the results in order.
    let collect (binder: 'T -> #IAsyncEnumerable<'U>) taskSeq = Internal.collect binder taskSeq

    /// Applies the given function to the items in the taskSeq and concatenates all the results in order.
    let collectSeq (binder: 'T -> #seq<'U>) taskSeq = Internal.collectSeq binder taskSeq

    /// Zips two task sequences, returning a taskSeq of the tuples of each sequence, in order. May raise ArgumentException
    /// if the sequences are or unequal length.
    let zip taskSeq1 taskSeq2 = Internal.zip taskSeq1 taskSeq2