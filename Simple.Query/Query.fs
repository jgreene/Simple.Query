module Simple.Query

open System
open Microsoft.FSharp.Quotations
open Microsoft.FSharp.Linq.QuotationEvaluation

type ISqlCommand = interface end
type ISqlCommand<'a> = 
    interface 
        inherit ISqlCommand
    end

type ITestSequence<'a> = 
    interface
        abstract TestSequence : unit -> seq<'a>
    end

type SelectBuilder<'a>(seq:seq<'a>) =

    new() = SelectBuilder<'a>(Seq.empty)
    
    member this.Delay(f:unit -> SelectBuilder<'b>) = 
        let newF = fun () -> 
                        let r = f() :> ITestSequence<'b>
                        r.TestSequence()
        SelectBuilder<'b>(Seq.delay newF)
    member this.Combine(r:SelectBuilder<'b>,s:SelectBuilder<'b>) = 
        let rs = r :> ITestSequence<'b>
        let ss = s :> ITestSequence<'b>
        SelectBuilder<'b>(Seq.append (rs.TestSequence()) (ss.TestSequence()))
    member this.For<'b,'c>(s:SelectBuilder<'b>,f:'b -> SelectBuilder<'c>) = 
        let ss = s :> ITestSequence<'b>
        let ff : 'b -> seq<'c> = fun x -> 
                                    let result = (f x) :> ITestSequence<'c>
                                    result.TestSequence()
        SelectBuilder<'c>(Seq.collect ff (ss.TestSequence()))
    member this.Yield<'b>(v:'b) = SelectBuilder<'b>(Seq.singleton v) //:> System.Collections.Generic.IEnumerable<'a>
    member this.YieldFrom<'b>(s) = SelectBuilder<'b>(s)
    member this.Zero() = SelectBuilder(Seq.empty)

    interface ITestSequence<'a> with
        member this.TestSequence () = seq

    interface ISqlCommand<'a>
           
type UpdateBuilder<'a>(query:SelectBuilder<'a>) =
    member this.For<'b,'c>(s:SelectBuilder<'b>,f:'b -> SelectBuilder<'c>) = 
        let SelectBuilder = SelectBuilder<'c>()
        UpdateBuilder(SelectBuilder)
    member this.Yield(v) = query.Yield(v)
    member this.Zero() = SelectBuilder()

    member this.Query with get() = query

    interface ISqlCommand<'a>
//
//type DeleteBuilder<'a>(query:SelectBuilder<'a>) =
//    member this.For<'b,'c>(s:SelectBuilder<'b>,f:'b -> SelectBuilder<'c>) = 
//        let SelectBuilder = SelectBuilder<'c>()
//        UpdateBuilder(SelectBuilder)
//    member this.Yield(v) = query.Yield(v)
//    member this.Zero() = SelectBuilder()
//
//    member this.Query with get() = query
//
//    interface ISqlCommand<'a>

type BatchBuilder<'a>(value:'a option, statements:ResizeArray<Expr>) =
    member this.Bind(x:Expr<SelectBuilder<'b>>, f) =
        statements.Add(x :> Expr) |> ignore
        f Seq.empty<'b>

//    member this.Bind(x:Expr<UpdateBuilder<'b>>, f) =
//        statements.Add(x :> Expr) |> ignore
//        f 0
//    member this.Bind(x:Expr<DeleteBuilder<'b>>, f) =
//        statements.Add(x :> Expr) |> ignore
//        f 0
    member this.Return<'b>(x:'b) = BatchBuilder(Some x, statements)

    member this.Statements with get() = statements.ToArray()

type ISession =
    
    interface
        inherit IDisposable
//        abstract Save : 'a -> 'a
//        abstract Get : int -> 'a
//        abstract Get : System.Guid -> 'a
        abstract Execute : Expr<SelectBuilder<'a>> -> seq<'a>
//        abstract Execute : Expr<UpdateBuilder<'a>> -> int
//        abstract Execute : Expr<DeleteBuilder<'a>> -> int
    end

    



let select = SelectBuilder() : unit SelectBuilder

let update = UpdateBuilder(select)

let batch = BatchBuilder((None : unit option), new ResizeArray<Expr>())

module Session =

        open System
        open System.Data.Common

        type database =
        | Postgres

        type Session(connString:string, db:database) =
    
            let connection = new Npgsql.NpgsqlConnection(connString)
            do connection.Open() |> ignore
            let trans = connection.BeginTransaction()
    
    

            interface ISession with
                override this.Execute(select) =
                    Seq.empty

            interface IDisposable with
                override this.Dispose() =
                    trans.Commit()
                    connection.Dispose()

open Session
let getSession (connString:string) =
    (fun () -> new Session(connString, Postgres))

