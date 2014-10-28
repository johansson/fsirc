namespace System
open System.Reflection

[<assembly: AssemblyTitleAttribute("fsirc")>]
[<assembly: AssemblyProductAttribute("fsirc")>]
[<assembly: AssemblyDescriptionAttribute("F# IRC parser and network library")>]
[<assembly: AssemblyVersionAttribute("0.0.1")>]
[<assembly: AssemblyFileVersionAttribute("0.0.1")>]
do ()

module internal AssemblyVersionInformation =
    let [<Literal>] Version = "0.0.1"
