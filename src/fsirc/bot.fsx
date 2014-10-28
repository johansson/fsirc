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

#I @"..\..\packages\FSharpx.Core\lib\40\"
#I @"..\..\packages\FParsec\lib\net40-client\"
#r "FSharpx.Core.dll"
#r "FParsec.dll"
#r "FParsecCS.dll"
#load "Extensions.fs"
#load "parser.fs"
#load "client.fs"

open System
open System.IO
open Fs.Irc
open Fs.Irc.Parser
open Microsoft.FSharp.Control

let args = fsi.CommandLineArgs

if args.Length < 5 then
    printfn "need nick, network, port, channel(s)"
    Environment.Exit(1)

let nick = fsi.CommandLineArgs.[1]
let server = fsi.CommandLineArgs.[2]
let port = Int32.Parse(fsi.CommandLineArgs.[3])

let wr = new StreamWriter("fsirc.log", true, Text.Encoding.UTF8)

wr.AutoFlush <- true

let write (s: string) = wr.WriteLine(s)

let channels = fsi.CommandLineArgs.[4..] |> Array.toList

let client = new Client.Client(nick, server, port)

client.Connected
    |> Observable.subscribe (fun _ -> channels |> List.map client.Join |> Async.Parallel |> Async.Ignore |> Async.Start)

client.Messaged
    |> Observable.subscribe (fun m -> printfn "%A" m)

client.Messaged
    |> Observable.filter(fun m -> match m.prefix with
                                  | Some (User (_,_,_)) -> m.command = "PRIVMSG"
                                  | _                   -> false)
    |> Observable.subscribe(fun m -> match m.prefix with
                                     | Some (User (n,_,_)) -> printfn "%s <%s> %s" m.parameters.Head n m.parameters.Tail.Head
                                     | _                   -> ())

client.Messaged
    |> Observable.filter(fun m -> match m.prefix with
                                  | Some (User (_,_,_)) -> m.command = "PRIVMSG" && m.parameters.Tail.Head.StartsWith("!uptime")
                                  | _                   -> false)
    |> Observable.subscribe(fun m -> match m.prefix with
                                     | Some (User (n,_,_)) -> client.Uptime (m.parameters.Head) |> Async.Start
                                     | _                   -> ())

client.Connect() |> Async.StartImmediate

let ``^c`` _ = client.Quit("Ctrl-C!") |> Async.RunSynchronously
Console.CancelKeyPress |> Observable.subscribe ``^c``

Console.ReadKey()
client.Quit("Bye, master!")

Async.Sleep(1000) |> Async.RunSynchronously