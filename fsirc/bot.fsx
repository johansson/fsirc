// Copyright 2014 Will Johansson
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

#I @"E:\projects\fsirc\packages\FSharpx.Core.1.8.41\lib\40\"
#I @"E:\projects\fsirc\packages\FParsec.1.0.1\lib\net40-client\"
#r "FSharpx.Core.dll"
#r "FParsec.dll"
#r "FParsecCS.dll"
#load "Extensions.fs"
#load "client.fs"
#load "parser.fs"

open System
open System.IO
open Fs.Irc
open Fs.Irc.Parser
open Microsoft.FSharp.Control

let wr = new StreamWriter("fsirc.log", true, Text.Encoding.UTF8)

wr.AutoFlush <- true

let write (s: string) = wr.WriteLine(s)

let nick = query {
    for arg in Environment.GetCommandLineArgs() do
    lastOrDefault
}

let client = new Client.Client(nick, "irc.someserver.net", 6667)

let channels = ["#foo"; "#bar"]

client.Connected
    |> Observable.subscribe (fun _ -> channels |> List.map client.Join |> Async.Parallel |> Async.Ignore |> Async.Start)

client.Messaged
    |> Observable.subscribe (fun s -> wr.WriteLine(s))

client.Messaged
    |> Observable.subscribe (fun s -> let message = Parser.ParseMessage s
                                      match message with
                                      | None   -> wr.WriteLine("parse error: %s", s)
                                                  printfn "parse error: %s" s
                                      | Some m -> wr.WriteLine("parsed: %A", m)
                                                  printfn "parsed: %A" m)

client.Messaged
    |> Observable.filter (fun s -> s.Contains("!uptime"))
    |> Observable.subscribe (fun s -> let c = if s.Contains("#foo") then "#foo" elif s.Contains("#bar") then "#bar" else "someuser"
                                      client.Uptime s |> Async.Start)

client.Connect() |> Async.StartImmediate

let ``^c`` _ = client.Quit("Ctrl-C!") |> Async.RunSynchronously
Console.CancelKeyPress |> Observable.subscribe ``^c``

Console.ReadKey()