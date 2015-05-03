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
                | End(name)-> 
                    let fvals = MList.pop attrStack  
                    if (fvals.Count > 0) then
                        yield {Path = name; Vals = fvals}
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
    
let sortBlind (recs: seq<XmlRec>) = 
    ()
    
    
type RecordDb(fname) =
    let recs = readXml fname |> parseStream |> Seq.toList 
    let groups = recs |> Seq.groupBy (fun s -> s.Path) |> Dict.ofSeq
    
    member x.ParseRules(fname) =
        if fileExists(fname) then readConfig fname |> Seq.toArray else [||] 

    member x.Dump() = 
        dumpGroups groups
        ()
   
    member x.ApplyRules (rules:Rule[]) =

        for (group, recs) in (Dict.sortedPairs groups) do
            let mutable sorted = false
            for rule in rules do 
                match rule with
                    | SortKey(g, key) when g=group ->
                        sorted <- true 
                        groups.[g] <- sortRecs recs key
                    | _ -> ()
                if not sorted then sortBlind recs

[<EntryPoint>]

let main argv =
    match argv.Length with 
        | 1 ->
            let db = RecordDb(argv.[0])
            let rules = db.ParseRules "rules.txt"
            db.ApplyRules rules
            db.Dump()
            0
        | _ ->
            printfn "Please specif XML file to read."
            1       
               
        
  