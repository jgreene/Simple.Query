module Simple.Query.Tests.TestHelper

open Simple.Query
open Simple.Query.Tests.Models

open Microsoft.FSharp.Quotations
open Microsoft.FSharp.Linq.QuotationEvaluation



type TestSession() =
    interface ISession with
        //member this.Save(x) = x

        member this.Execute(query:Expr<SelectBuilder<'a>>) : seq<'a> = 
            let result = query.Compile()() 
            let r = result :> ITestSequence<'a>
            r.TestSequence()

//        member this.Execute(query:Expr<UpdateBuilder<'a>>) : int = 
//            let result = query.Compile()()
//            result.Query |> Seq.length
//        member this.Execute(query:Expr<DeleteBuilder<'a>>) : int = 
//            let result = query.Compile()()
//            result.Query |> Seq.length

//    member this.Execute(query:Expr<FutureBuilder<'a>>) : 'a =
//        let resultTyp = typeof<'a>
//        let properties = resultTyp.GetProperties()
//        
//        let futureQuery = query.Compile()()
//        let statements = futureQuery.Statements
//        
//        
//        let result = statements
//                    |> List.mapi(fun i statement -> this.Execute()
////                        let typ = statement.GetType()
////                        let genericType = typ.MakeGenericType(properties.[i])
////                        match statement with
////                        | :? SelectBuilder<_> -> 
//                    )
//        Microsoft.FSharp.Reflection.FSharpValue.MakeTuple (result |> Array.ofList, resultTyp) :?> 'a


    interface System.IDisposable with
        member this.Dispose() = ()

let print x = 
        printf "%A" x
        printf "%A" (System.Environment.NewLine + "------------------------------------------------------------------")
        printf "%A" System.Environment.NewLine

let test (expr:Expr<SelectBuilder<'a>>) =
    print expr
    use testSess = new TestSession() :> ISession
    
    let testResult = testSess.Execute expr
    print testResult

    use sess = getSession "Server=127.0.0.1;Port=5432;Database=SimpleQuery;User Id=postgres;Password=password1;" ()

    let sessResult = testSess.Execute expr
    print sessResult
    let result = (Seq.exists2 (fun f s -> f <> s) testResult sessResult ) = false
    NUnit.Framework.Assert.IsTrue(result)
