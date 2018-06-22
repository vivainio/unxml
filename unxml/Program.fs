open System.Xml
open System.IO

type StreamEnt =
    | Path of string
    | Val of string * string
    | Attr of string * string
    | End of string


let readXml (fname:string) : StreamEnt list =
    use st = new StreamReader(fname)
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

let treeView fname =
    let stream = List.toArray(readXml fname)

    let mutable depth = 0
    let mutable currentPath = ""
    let mutable idx = 0
    while idx < stream.Length do
        let ent = stream.[idx]
        let indent = (String.replicate (depth*2) " ")
        match ent with
            // this is only entered when there is tag with attribute and text content
            | Val(_,v)  ->
                printfn "%s= %s" indent v
            // non-shallow attribute
            | Attr(k,v) ->
                printfn "%s[%s]: %s" indent k v
            | Path name ->
                currentPath <- name
                depth <- depth + 1
                // peek ahead for shallow or hero; if so nuke the attribute
                let extra =
                    match stream.[idx+1], stream.[idx+2] with
                    | Val(_,v), End(_) ->
                        idx <- idx + 1
                        sprintf " = %s" v
                    | (Val(k,v) | Attr(k,v)), End(_) ->
                        idx <- idx + 1
                        sprintf " [%s]: %s" k v
                    | _ -> ""
                printfn "%s%s%s" indent name extra

            | End(_)->
                depth <- depth - 1
        idx <- idx+1

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


