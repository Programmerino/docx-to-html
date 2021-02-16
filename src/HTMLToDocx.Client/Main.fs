module tester.Client.Main


open Elmish
open Bolero
open tester.Client.Bindings
open Bolero.Html
open Bindings.Bindings
open MudBlazor
open Microsoft.AspNetCore.Components.Forms
open Elmish
open System.IO
open System
open Microsoft.JSInterop
open Bolero.Remoting.Client
open BlazorInputFile
open System.Text

type Model =
    { Wireframe: Wireframe.Model
      HTMLFile: string }

type Message =
    | Ping
    | Wireframe of Wireframe.Message
    | Upload of IFileListEntry []
    | Finished of string
    | Error of Exception

let update js message (model: Model) =
    printfn "Update"

    match message with
    | Ping -> model, Cmd.none
    | Wireframe msg ->
        { model with
              Wireframe = Wireframe.update msg model.Wireframe },
        Cmd.none
    | Upload ev ->
        //{ model with HTMLFile = (ev.File.OpenReadStream() |> Clippit.convertToHtml ev.File.Name) } |> tap (printfn "Finished: %A"), Cmd.none
        model, Cmd.ofAsync (Clippit.convertToHtml "test") (ev.[0].Data) (Finished) (Error)
    | Finished html -> { model with HTMLFile = html }, Cmd.none
    | Error exn -> raise exn

let mudTextField = bBuild comp<MudTextField<_>> []

let mudLink = bBuild comp<MudLink> []

// Avoids UriFormatException for strings being too long
let unboundedEscape str =
    let limit = 2000

    let loops = String.length str / limit

    let newBuilder =
        [ 0 .. loops ]
        |> List.fold
            (fun (sb: StringBuilder) x ->
                if x < loops then
                    sb.Append(Uri.EscapeDataString(str.Substring(limit * x, limit)))
                else
                    sb.Append(Uri.EscapeDataString(str.Substring(limit * x))))
            (StringBuilder())

    newBuilder.ToString()

let toUri str =
    "data:text/html;charset=utf-8,"
    + unboundedEscape str

let view model dispatch =
    Wireframe.view
        model.Wireframe
        (Wireframe >> dispatch)
        "https://github.com/Programmerino/docx-to-html"
        false
        false
        []
        [ mudText Typo.h2 [ "Align" => Align.Center ] [ text "DOCX to HTML" ]
          comp<InputFile>
              [ attr.id "fileInput"
                attr.callback "OnChange" (Upload >> dispatch)
                attr.hidden true ]
              []
          mudText
              Typo.body1
              [ "Align" => Align.Center ]
              [ text "Powered by .NET using "
                mudLink [ attr.href "https://github.com/fsbolero/Bolero" ] [
                    text "Bolero"
                ]
                text " and "
                mudLink [ attr.href "https://github.com/sergey-tihon/Clippit/" ] [
                    text "Clippit"
                ]
                text
                    ". This is the best method I found for converting docx to HTML thus far that's viable for client-side execution." ]
          div [ Classes [ "d-flex"
                          "flex-row"
                          "justify-center"
                          "mt-14" ] ] [
              mudButton [ "HtmlTag" => "label"
                          "Variant" => Variant.Filled
                          "Color" => Color.Primary
                          "StartIcon" => Icons.Filled.CloudUpload
                          Classes [ "ma-2" ]
                          attr.``for`` "fileInput" ] [
                  text "Upload DOCX here"
              ]
              cond (model.HTMLFile = "")
              <| function
              | false ->
                  mudButton [ "Link" => (toUri model.HTMLFile)
                              attr.target "_blank"
                              attr.download "Out.html"
                              "Variant" => Variant.Filled
                              "Color" => Color.Primary
                              "StartIcon" => Icons.Filled.CloudDownload
                              Classes [ "ma-2" ] ] [
                      text "Download result"
                  ]
              | true -> empty
          ]
          iframe [ attr.src (toUri model.HTMLFile) ] [] ]

let init _ =
    { Wireframe = Wireframe.init ()
      HTMLFile = "" },
    Cmd.none

type MyApp() =
    inherit ProgramComponent<Model, Message>()


    override this.Program =
        Program.mkProgram init (update this.JSRuntime) view
        |> Program.withErrorHandler (fun (_, exn) -> printfn "%A" exn)
