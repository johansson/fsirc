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

#I @"E:\projects\fsirc\packages\FParsec.1.0.1\lib\net40-client\"

#r "FParsec.dll"
#r "FParsecCS.dll"

#load "parser.fs"

open Fs.Irc
open Fs.Irc.Parser
open FParsec

let parse = Parser.parse

let nick = Parser.nick
let user = Parser.user

let userPrefix = Parser.userPrefix

let serverPrefix = Parser.serverPrefix

let prefix = Parser.prefix

let command = Parser.command <?> "command"

let parameters = Parser.parameters <?> "parameters"

let message = Parser.message

let NickStandalone  = "nick" |> parse nick
let UserStandalone  = "user" |> parse user

let UserPrefix1     = "nick "                                   |> parse userPrefix
let UserPrefix2     = "nick!user "                              |> parse userPrefix
let UserPrefix3     = "nick!user@host "                         |> parse userPrefix
let UserPrefix4     = "nick@host"                               |> parse userPrefix
let UserPrefix5     = "nick@fe80::6924:2274:ecbb:a5e4%15"       |> parse userPrefix
let UserPrefix6     = "nick!user@fe80::6924:2274:ecbb:a5e4%15"  |> parse userPrefix
let UserPrefix7     = "nick "                                   |> parse userPrefix
let UserPrefix8     = "nick!user "                              |> parse userPrefix
let UserPrefix9     = "nick!user@host "                         |> parse userPrefix
let UserPrefix10    = "nick\r\n"                                |> parse userPrefix
let UserPrefix11    = "nick!user\r\n"                           |> parse userPrefix
let UserPrefix12    = "nick!user@host\r\n"                      |> parse userPrefix

let ServerPrefix1   = "server"                                  |> parse serverPrefix
let ServerPrefix2   = "server "                                 |> parse serverPrefix

let Prefix1         = ":some.server.net "                       |> parse prefix
let Prefix2         = ":nick "                                  |> parse prefix
let Prefix3         = ":nick!user"                              |> parse prefix
let Prefix4         = ":nick!user@host"                         |> parse prefix
let Prefix5         = ":nick@host"                              |> parse prefix
let Prefix6         = ":fe80::6924:2274:ecbb:a5e4"              |> parse prefix

let Command1        = "JOIN"                                    |> parse command
let Command2        = "001"                                     |> parse command
let Command3        = "fail"                                    |> parse command

let Parameters1     = " #foo\r\n"                               |> parse parameters
let Parameters2     = " #foo :I am who I am.\r\n"               |> parse parameters

let Message1        = ":some.server.net NOTICE * :*** Looking up your hostname..."                              |> parse message
let Message2        = ":some.server.net 001 nick :Welcome to the Internet Relay Chat Network nick"              |> parse message
let Message3        = ":nick MODE nick :+i"                                                                     |> parse message
let Message4        = ":nick!~user@host JOIN #foo"                                                              |> parse message
let Message5        = ":nick!~user@host PRIVMSG #foo :yeah definitely"                                          |> parse message
let Message6        = "PING :some.server.net"                                                                   |> parse message
let Message7        = ":fe80::6924:2274:ecbb:a5e4 NOTICE * :*** Looking up your hostname..."                    |> parse message
let Message8        = ":fe80::6924:2274:ecbb:a5e4 001 nick :Welcome to the Internet Relay Chat Network nick"    |> parse message

let NickStandalone_expected = Some "nick"
let UserStandalone_expected = Some "user"
let UserPrefix1_expected    = Some (User ("nick",None,None))
let UserPrefix2_expected    = Some (User ("nick",Some "user",None))
let UserPrefix3_expected    = Some (User ("nick",Some "user",Some "host"))
let UserPrefix4_expected    = Some (User ("nick",None,Some "host"))
let UserPrefix5_expected    = Some (User ("nick",None,Some "fe80::6924:2274:ecbb:a5e4%15"))
let UserPrefix6_expected    = Some (User ("nick",Some "user",Some "fe80::6924:2274:ecbb:a5e4%15"))
let UserPrefix7_expected    = Some (User ("nick",None,None))
let UserPrefix8_expected    = Some (User ("nick",Some "user",None))
let UserPrefix9_expected    = Some (User ("nick",Some "user",Some "host"))
let UserPrefix10_expected   = Some (User ("nick",None,None))
let UserPrefix11_expected   = Some (User ("nick",Some "user",None))
let UserPrefix12_expected   = Some (User ("nick",Some "user",Some "host"))
let ServerPrefix1_expected  = Some (Server "server")
let ServerPrefix2_expected  = Some (Server "server")
let Prefix1_expected        = Some (Server "some.server.net")
let Prefix2_expected        = Some (User ("nick",None,None))
let Prefix3_expected        = Some (User ("nick",Some "user",None))
let Prefix4_expected        = Some (User ("nick",Some "user",Some "host"))
let Prefix5_expected        = Some (User ("nick",None,Some "host"))
let Prefix6_expected        = Some (Server "fe80::6924:2274:ecbb:a5e4")
let Command1_expected       = Some "JOIN"
let Command2_expected       = Some "001"
let Command3_expected       = None
let Parameters1_expected    = Some ["#foo"]
let Parameters2_expected    = Some ["#foo"; "I am who I am."]
let Message1_expected       = Some {prefix = Some (Server "some.server.net"); command = "NOTICE"; parameters = ["*"; "*** Looking up your hostname..."];}
let Message2_expected       = Some {prefix = Some (Server "some.server.net"); command = "001"; parameters = ["nick"; "Welcome to the Internet Relay Chat Network nick"];}
let Message3_expected       = Some {prefix = Some (User ("nick",None,None)); command = "MODE"; parameters = ["nick"; "+i"];}
let Message4_expected       = Some {prefix = Some (User ("nick",Some "~user",Some "host")); command = "JOIN"; parameters = ["#foo"];}
let Message5_expected       = Some {prefix = Some (User ("nick",Some "~user",Some "host")); command = "PRIVMSG"; parameters = ["#foo"; "yeah definitely"];}
let Message6_expected       = Some {prefix = None; command = "PING"; parameters = ["some.server.net"];}
let Message7_expected       = Some {prefix = Some (Server "fe80::6924:2274:ecbb:a5e4"); command = "NOTICE"; parameters = ["*"; "*** Looking up your hostname..."];}
let Message8_expected       = Some {prefix = Some (Server "fe80::6924:2274:ecbb:a5e4"); command = "001"; parameters = ["nick"; "Welcome to the Internet Relay Chat Network nick"];}

let passOrFail a e = if a = e then "pass:" else "fail:"

printfn "---Nick and User---"
printfn "%s %A" (passOrFail NickStandalone NickStandalone_expected) NickStandalone
printfn "%s %A" (passOrFail UserStandalone UserStandalone_expected) UserStandalone
printfn "---Just User---"
printfn "%s %A" (passOrFail UserPrefix1 UserPrefix1_expected) UserPrefix1
printfn "%s %A" (passOrFail UserPrefix2 UserPrefix2_expected) UserPrefix2
printfn "%s %A" (passOrFail UserPrefix3 UserPrefix3_expected) UserPrefix3
printfn "%s %A" (passOrFail UserPrefix4 UserPrefix4_expected) UserPrefix4
printfn "%s %A" (passOrFail UserPrefix5 UserPrefix5_expected) UserPrefix5
printfn "%s %A" (passOrFail UserPrefix6 UserPrefix6_expected) UserPrefix6
printfn "%s %A" (passOrFail UserPrefix7 UserPrefix7_expected) UserPrefix7
printfn "%s %A" (passOrFail UserPrefix8 UserPrefix8_expected) UserPrefix8
printfn "%s %A" (passOrFail UserPrefix9 UserPrefix9_expected) UserPrefix9
printfn "%s %A" (passOrFail UserPrefix10 UserPrefix10_expected) UserPrefix10
printfn "%s %A" (passOrFail UserPrefix11 UserPrefix11_expected) UserPrefix11
printfn "%s %A" (passOrFail UserPrefix12 UserPrefix12_expected) UserPrefix12
printfn "---Just Server---"
printfn "%s %A" (passOrFail ServerPrefix1 ServerPrefix1_expected) ServerPrefix1
printfn "%s %A" (passOrFail ServerPrefix2 ServerPrefix2_expected) ServerPrefix2
printfn "---Prefixes---"
printfn "%s %A" (passOrFail Prefix1 Prefix1_expected) Prefix1
printfn "%s %A" (passOrFail Prefix2 Prefix2_expected) Prefix2
printfn "%s %A" (passOrFail Prefix3 Prefix3_expected) Prefix3
printfn "%s %A" (passOrFail Prefix4 Prefix4_expected) Prefix4
printfn "%s %A" (passOrFail Prefix5 Prefix5_expected) Prefix5
printfn "%s %A" (passOrFail Prefix6 Prefix6_expected) Prefix6
printfn "---Commands---"
printfn "%s %A" (passOrFail Command1 Command1_expected) Command1
printfn "%s %A" (passOrFail Command2 Command2_expected) Command2
printfn "%s %A" (passOrFail Command3 Command3_expected) Command3
printfn "---Parameters---"
printfn "%s %A" (passOrFail Parameters1 Parameters1_expected) Parameters1
printfn "%s %A" (passOrFail Parameters2 Parameters2_expected) Parameters2
printfn "---Messages---"
printfn "%s %A" (passOrFail Message1 Message1_expected) Message1
printfn "%s %A" (passOrFail Message2 Message2_expected) Message2
printfn "%s %A" (passOrFail Message3 Message3_expected) Message3
printfn "%s %A" (passOrFail Message4 Message4_expected) Message4
printfn "%s %A" (passOrFail Message5 Message5_expected) Message5
printfn "%s %A" (passOrFail Message6 Message6_expected) Message6
printfn "%s %A" (passOrFail Message7 Message7_expected) Message7
printfn "%s %A" (passOrFail Message8 Message8_expected) Message8