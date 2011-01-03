module Simple.QueryInternals.QueryAnalyzer

type queryType =
| Select
| Update
| Delete

type projection = {
    tableType : System.Type
    
}

type query = {
    queryType : queryType
    tableType : System.Type
    projectionType : System.Type
}
