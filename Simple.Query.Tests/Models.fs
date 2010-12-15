module Simple.Query.Tests.Models

open Simple.Query

type Person = {
    Id : int
    FirstName : string
    LastName : string
}

let people = {1..10} |> Seq.map (fun x -> { Id = x; FirstName = "FirstName" + x.ToString(); LastName = "LastName" + x.ToString(); })

type ISession with
    member this.People = SelectBuilder<Person>(people)
