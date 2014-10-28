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

namespace Fs.Irc

open Microsoft.FSharp.Control

open System
open System.IO
open System.Net.Sockets
open Extensions

open Fs.Irc
open Fs.Irc.Parser

module Client =
    let getBytes (x: string) = x |> System.Text.Encoding.UTF8.GetBytes

    type Client(n: string, h: string, p) =
        let mutable nick = n
        let mutable stream : Option<NetworkStream> = None
        let mutable reader : Option<AsyncStreamReader> = None
        let mutable succeeded = true
        let bootTime = DateTime.UtcNow

        let connected = new Event<string>()
        let messaged = new Event<Message>()
        let pinged = new Event<string>()

        let ping (n: string) = String.Format("PONG :{0}\r\n", n)

        let write (str: string) = async {
            match stream with
                | Some s -> try
                                do! str |> getBytes |> s.AsyncWrite
                                do! s.AsyncFlush()
                            with
                            | _ -> printfn "exception writing :("
                | None   -> printfn "No stream"
        }

        let uptime (d: string) = async {
            let uptime = DateTime.UtcNow - bootTime
            let days = if uptime.Days > 0 then String.Format("{0}d ", uptime.Days) else String.Empty
            let u = String.Format("PRIVMSG {0} :{1}{2}h {3}m {4}s\r\n", d, days, uptime.Hours, uptime.Minutes, uptime.Seconds)
            do! write u
        }

        let login = async {
            let u = String.Format("USER {0} 0 * :FsIrc\r\n", nick)
            let n = String.Format("NICK {0}\r\n", nick)
            do! write u
            do! write n
        }

        let join (ch: string) = async {
            let join = String.Format("JOIN {0}\r\n", ch)
            do! write join
        }

        let die (s: string) = async {
            let die = String.Format("QUIT :{0}\r\n", if String.IsNullOrEmpty(s) then "Goodbye, cruel world." else s)
            do! write die
            Environment.Exit(0)
        }

        let procLine line = async {
            printfn "%s" line
            let message = Fs.Irc.Parser.ParseMessage(line)
            
            match message with
            | None   -> printfn "parse error"
            | Some m -> messaged.Trigger m
        }

        let rec read (rdr: Option<AsyncStreamReader>) = async {
            match rdr with
                | None   -> printfn "AsyncStreamReader doesn't exist yet, or something overwrote it with None."
                | Some r ->
                    let! fin = r.EndOfStream
                    if not fin then
                        let! line = r.ReadLine()
                        do! procLine(line)

                    do! read rdr
        }

        member this.Connect () = async {
            this.Messaged
                |> Observable.filter(fun m -> match m.prefix with
                                              | None -> m.command = "PING"
                                              | _    -> false)
                |> Observable.subscribe(fun m -> pinged.Trigger m.parameters.Head) |> ignore

            this.Messaged
                |> Observable.filter(fun m -> match m.prefix with
                                              | Some (Server s) -> m.command = "001"
                                              | _               -> false)
                |> Observable.subscribe(fun m -> match m.prefix with
                                                 | Some (Server s) -> connected.Trigger s
                                                 | _               -> ()) |> ignore

            this.Pinged |> Observable.subscribe(fun pong -> ping pong |> write |> Async.Ignore |> Async.Start) |> ignore
            let c = new TcpClient()
            c.NoDelay <- true
            c.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.KeepAlive, 1)
            do! c.AsyncConnect(h, p)
            stream <- Some(c.GetStream())
            match stream with
                | Some s -> reader <- Some(new AsyncStreamReader(s))
                | None   -> printfn "Something happened. Can't make AsyncStreamReader."
                            Environment.Exit -1
            read reader |> Async.Start
            do! login
        }

        member this.Join ch = async {
            do! join ch
        }

        member this.Write s = async {
            let! succeeded = write s
            return succeeded
        }

        member this.Quit s = async {
            do! die s
        }

        member this.Uptime d = async {
            do! uptime d
        }

        member this.Connected = connected.Publish
        member this.Messaged = messaged.Publish
        member private this.Pinged = pinged.Publish