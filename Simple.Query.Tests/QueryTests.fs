namespace Simple.Query.Tests

open NUnit.Framework
open Simple.Query
open Simple.Query.Tests.TestHelper
open Simple.Query.Tests.Models

[<TestFixture>]
type QueryTests() =

    let print x = 
        
        printf "%A" x
        printf "%A" (System.Environment.NewLine + "------------------------------------------------------------------")
        printf "%A" System.Environment.NewLine

    let sess = new TestSession() :> ISession
    
    
    [<Test>]
    member this.``Can Convert Simple Select Into Sequence``() =
        
        test <@ select { for a in sess.People do yield (a.FirstName, a.LastName) } @>

        
        
        

