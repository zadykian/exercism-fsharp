module OcrNumbers

open System

type DigitCell = string list

type DigitRow = DigitCell list

let private intersperse (item: 'T) (sequence: seq<'T>): seq<'T> =
    sequence
    |> Seq.collect (fun currentItem -> [item; currentItem])
    |> Seq.mapi (fun index item -> index, item)
    |> Seq.choose (fun (index, item) -> if index = 0 then None else Some item)

let private zipseq<'T> (input: seq<seq<'T>>): seq<seq<'T>> =
    let enumerators =
        input
        |> Seq.map (fun seq -> seq.GetEnumerator())
        |> Seq.toList

    let moveNextForAll() =
        enumerators
        |> Seq.forall (fun enumerator -> enumerator.MoveNext())

    seq {
        while moveNextForAll() do
            yield enumerators |> Seq.map (fun enumerator -> enumerator.Current)
    }

let private parseSingleRow (inputRow: string list): DigitRow =
    let toSingleDigit (chunk: seq<seq<char>>): DigitCell = 
        chunk
            |> Seq.map String.Concat
            |> Seq.toList

    inputRow
        |> Seq.map (fun line -> line |> Seq.chunkBySize 3 |> Seq.map Array.toSeq)
        |> zipseq
        |> Seq.map toSingleDigit
        |> Seq.toList

let private parseCell (digitCell: DigitCell): char =
    let cellsToDigits : Map<DigitCell, byte> =
        [
            ([ " _ ";
               "| |";
               "|_|";
               "   " ], 0uy)

            ([ "   ";
               "  |";
               "  |";
               "   " ], 1uy)

            ([ " _ ";
               " _|";
               "|_ ";
               "   " ], 2uy)

            ([ " _ ";
               " _|";
               " _|";
               "   " ], 3uy)

            ([ "   ";
               "|_|";
               "  |";
               "   " ], 4uy)

            ([ " _ ";
               "|_ ";
               " _|";
               "   " ], 5uy)

            ([ " _ ";
               "|_ ";
               "|_|";
               "   " ], 6uy)

            ([ " _ ";
               "  |";
               "  |";
               "   " ], 7uy)

            ([ " _ ";
               "|_|";
               "|_|";
               "   " ], 8uy)

            ([ " _ ";
               "|_|";
               " _|";
               "   " ], 9uy)
        ]
        |> Map.ofList

    match cellsToDigits.TryFind digitCell with
        | Some digit -> sprintf "%i" digit |> Seq.last
        | None       -> '?'

let convert (input: string list): string option =
    let rawDigitRows =
        input
        |> Seq.chunkBySize 4
        |> Seq.map Seq.toList
        |> Seq.toList

    let hasInvalidRows =
        rawDigitRows
        |> Seq.exists (fun row ->
            row |> Seq.length <> 4 ||
            row |> Seq.exists (fun line -> line.Length % 3 <> 0))

    let digitCharacters =
        rawDigitRows
            |> Seq.map parseSingleRow
            |> Seq.map (Seq.map parseCell)
            |> intersperse (List.toSeq [','])
            |> Seq.collect id

    if hasInvalidRows then None
    else Some (digitCharacters |> String.Concat)