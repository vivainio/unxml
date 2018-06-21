open System.Xml
open System.IO
open System.Collections.Generic
open Fake.FileSystemHelper
open MutableCol


type StreamEnt =
    | Path of string
    | Val of string * string
    | Attr of string * string
    | End of string

type XmlRec = {
    Parents: string[]
    Path : string
    Vals : Dictionary<string,string>
    //Attrs: Dictionary<string,string>
}

let p_rec (r:XmlRec) =
    printfn "%s\n" r.Path
    for (k,v) in Dict.sortedPairs r.Vals do
        printfn "   %s: %s" k v


let readXml (fname:string) =
    let st = new StreamReader(fname)
    use r = XmlReader.Create(st)
    r.MoveToElement() |> ignore
    let elems = seq {
        let pars = new Stack<string>()
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

//type KvList = List<string*string>
type KvDict = Dictionary<string,string>
type StackPair = KvDict*KvDict

let rec safeAdd (dict:KvDict) (k:string) (v:string) =
     if dict.ContainsKey(k) then
        (safeAdd dict (k+"_") v)
     else
        dict.Add(k,v)



let newPair() =
    new Dictionary<string,string>()


let parseStream stream =
    let mutable vals = new KvDict()
    let mutable oldPath = ""

    let attrStack = new List<KvDict>(50)
    let parentPaths = new List<string>(50)
    seq {
        for ent in stream do
            match ent with
                | Val(k,v) ->
                    let grandp = MList.peek attrStack 2

                    safeAdd (grandp) k v
                | Attr(k,v) ->

                    let parent = MList.peek attrStack 1
                    safeAdd (parent) (sprintf "[%s]" k) v

                | Path (name) ->
                    MList.push attrStack (newPair())
                    MList.push parentPaths name
                | End(name)->
                    let fvals = MList.pop attrStack
                    let this = MList.pop parentPaths
                    if (fvals.Count > 0) then
                        yield {Path = name; Vals = fvals;
                        Parents = Array.ofSeq parentPaths}
    }

type Rule =
    | SortKey of string*string
    | Delete of string*string

let readLines (filePath:string) = seq {
    use sr = new StreamReader (filePath)
    while not sr.EndOfStream do
        yield sr.ReadLine ()
}

let readConfig fname : Rule[] =
    let lines = readLines fname
    seq {
        let mutable curRecord = ""
        for line in lines do
            let parts = line.Split()
            let rule =
                match parts.[0] with
                | "del" -> Some (Delete(curRecord, parts.[1]))
                | "sort" -> Some (SortKey(curRecord, parts.[1]))
                | "rec" -> curRecord <- parts.[1]; None
                | _ -> failwithf "Invalid rule '%s'" parts.[0]
            if rule.IsSome then
                yield rule.Value
    } |> Seq.toArray


let dumpGroups recGroups =
    for (g, vlist) in Dict.sortedPairs recGroups do
        printfn "\n%s:" g

        for v in vlist do
            printfn "  - %s:" g
            for (k,v) in Dict.sortedPairs v.Vals do
                printfn "     %s: %s" k v

let sortRecs (recs: seq<XmlRec>) key =
    recs
    |> Seq.sortBy (fun r ->
        //printfn "look for %s" key
        //p_rec r

        let (ok, v) = r.Vals.TryGetValue(key)
        if ok then v else "")
let treeView fname =
    let stream = List.toArray(readXml fname)
    let shallows =
        seq {
            for i in [0..stream.Length-3] do
                let segment = stream.[i], stream.[i+1], stream.[i+2]

                match segment with
                    | (Path(name), (Val(_,_) | Attr(_, _)), End(_)) ->
                        yield i
                        yield i+1
                    | _ -> ()
        } |> Set.ofSeq

    let mutable depth = 0
    let mutable currentPath = ""

    for (idx,ent) in Seq.mapi (fun i x -> i,x) stream do
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

            | End(name)->
                depth <- depth - 1

    ()

type RecordDb(fname) =
    let recs = readXml fname |> parseStream |> Seq.toList
    let groups = recs |> Seq.groupBy (fun s -> s.Path) |> Dict.ofSeq

    member x.ParseRules(fname) =
        if fileExists(fname) then readConfig fname |> Seq.toArray else [||]

    member x.Dump() =
        dumpGroups groups
        ()
    member x.DumpAsTree() =
        for r in recs do
            let indent = String.replicate r.Parents.Length " "
            //r.Parents.Length " "
            printfn "%s%s" indent r.Path
            for (k,v) in Dict.sortedPairs r.Vals do
                printfn "%s  %s: %s" indent k v
    member x.ApplyRules (rules:Rule[]) =

        for (group, recs) in (Dict.sortedPairs groups) do
            let mutable sorted = false
            for rule in rules do
                match rule with
                    | SortKey(g, key) when g=group ->
                        sorted <- true
                        groups.[g] <- sortRecs recs key
                    | _ -> ()
                //if not sorted then sortBlind recs

[<EntryPoint>]

let main argv =
    // todo arg parsing to change mode
    match argv.Length with
        | 1 ->
            let fname = argv.[0]

            treeView(fname)
            //let db = RecordDb(argv.[0])
            //let rules = db.ParseRules "rules.txt"
            //db.ApplyRules rules
            //db.Dump()
            0
        | _ ->
            printfn "Please specify XML file to read."
            1


