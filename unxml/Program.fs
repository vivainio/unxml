open System.Xml
open System.IO

type StreamEnt =
    | Path of string
    | Val of string * string
    | Attr of string * string
    | End of string


let readXml (fname:string) : StreamEnt list =
    let st = new StreamReader(fname)
    use r = XmlReader.Create(st)
    r.MoveToElement() |> ignore
    let elems = seq {
        let mutable curKey = ""

        while r.Read() do
            match r.NodeType with
                | XmlNodeType.Element ->
                    curKey <- r.Name

                    yield Path(curKey)
                    while r.MoveToNextAttribute() do
                        yield Attr(r.Name, r.Value)
                    r.MoveToElement() |> ignore
                    if r.IsEmptyElement then
                        curKey <- ""
                        yield End(r.Name)


                | XmlNodeType.Text ->
                    yield Val(curKey, r.Value)

                | XmlNodeType.EndElement ->
                    curKey <- ""
                    yield End(r.Name)

                | _ -> ()
    }
    elems |> Seq.toList

let readLines (filePath:string) = seq {
    use sr = new StreamReader (filePath)
    while not sr.EndOfStream do
        yield sr.ReadLine ()
}

let treeView fname =
    let stream = List.toArray(readXml fname)
    let shallows =
        seq {
            for i in [0..stream.Length-3] do
                let segment = stream.[i], stream.[i+1], stream.[i+2]
                match segment with
                    | (Path(_), (Val(_) | Attr(_)), End(_)) ->
                        yield i
                        yield i+1
                    | _ -> ()
        } |> Set.ofSeq

    let mutable depth = 0
    let mutable currentPath = ""

    //for (idx,ent) in Array.mapi (fun i x -> i,x) stream do
    stream |> Array.iteri (fun idx ent ->
        let indent = (String.replicate (depth*2) " ")
        let shallow = Set.contains idx shallows
        match ent with
            | Val(k,v) ->
                if k = currentPath then
                    printfn "%s = %s" (if shallow then "" else indent) v
                else
                    printfn "%s%s: %s" indent k v
            | Attr(k,v) ->
                printfn "%s[%s]: %s" (if shallow then " " else indent)  k v
            | Path name ->
                currentPath <- name
                depth <- depth + 1
                printf "%s%s%s" indent name (if shallow then "" else "\n")
            | End(_)->
                depth <- depth - 1
    )

[<EntryPoint>]
let main argv =
    match argv.Length with
        | 1 ->
            let fname = argv.[0]
            if File.Exists fname then
                try
                    treeView(fname)
                    0
                with
                | :? XmlException as ex ->
                    printfn "Malformed xml (or no xml at all?). Error: %s" ex.Message
                    3
            else
                printfn "unxml: File not found: '%s'" fname
                2
        | _ ->
            printfn "Please specify XML file to read."
            1


