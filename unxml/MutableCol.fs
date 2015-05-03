namespace MutableCol

open System.Collections.Generic

[<AutoOpen>]
module Dict = 
    // Converts seq of key - value pairs to mutable Dictionary
    let ofSeq (src:seq<'a * 'b>) = 
        let d = new Dictionary<'a, 'b>()
        for (k,v) in src do
            d.Add(k,v)
        d

    // get a seq of key-value pairs for easy iteration with for (k,v) in d do...
    let pairs (d:Dictionary<'a, 'b>) =
        seq {
            for kv in d do
                yield (kv.Key, kv.Value)
        } |> Seq.toArray

    let sortedPairs (d:Dictionary<'a, 'b>) =
        let keys = Seq.toArray d.Keys
                   |> Seq.sort
        seq {
            for k in keys do 
                yield (k, d.[k])
         }
    let keys (d:Dictionary<'a, 'b>) = 
        let kc = d.Keys
        [for k in kc -> k]

[<AutoOpen>]            
module MList =
    let push (l: List<'t>) v = l.Add v
    
    let peek (l: List<'t>) n = l.[l.Count - n] 

    let pop (l: List<'t>) =
        let last = l.[l.Count-1] 
        l.RemoveAt (l.Count-1)
        last

     
            
