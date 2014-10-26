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

open System
open FParsec

type ServerName = string
type NickName   = string
type RealName   = string
type UserName   = string
type Command    = string
type Parameter  = string

type Prefix =
    | Server of ServerName
    | User   of NickName * Option<UserName> * Option<ServerName>

type Message = {
    prefix     : Option<Prefix>;
    command    : Command;
    parameters : List<Parameter>
}

module Parser =
    let internal colon   = ':'
    let internal excl    = '!'
    let internal at      = '@'
    let internal space   = ' '
    let internal dot     = '.'
    let internal spaces = (anyOf " \b\t")

    let internal nick = many1Chars (noneOf " .:!@\r\n")
    let internal user = many1Chars (noneOf " @\r\n")        

    let internal userPrefix = parse {
        let! nick   = nick <?> "nick"
        do! nextCharSatisfies (fun c -> c <> dot && c <> colon)
        let! user   = opt (skipChar excl >>? user)
        let! server = opt (skipChar at >>? many1Satisfy (isNoneOf " \r\n"))
        return User (nick, user, server)
    }

    let internal serverPrefix = many1Satisfy (fun x -> x <> space) |>> fun s -> Server s
 
    let internal prefix = skipChar colon >>. (attempt userPrefix <|> serverPrefix)

    let internal command = many1Satisfy isUpper <|> manyMinMaxSatisfy 3 3 isDigit

    let internal parameter = skipChar colon >>. many1Chars (noneOf "\r\n") <|> many1Chars (noneOf " \r\n")
    let internal parameters = many (spaces >>. parameter)

    let internal message : Parser<Message, unit> = parse {
        let! p  = opt prefix <?> "prefix"
        let! s1 = opt (many1Chars spaces)
        let! c  = command <?> "command"
        let! ps  = parameters <?> "parameters"
        let! _  = opt newline
        let! eoi = eof
        return { prefix = p; command = c; parameters = ps }
    }

    let internal parse g line =
        match run g line with
        | Success(r, _, _) -> Some r
        | Failure(e, _, _) -> printfn "%s" e
                              None

    let ParseMessage line = parse message line