module Clippit

open System.IO
open System.Linq
open System.Xml.Linq
open Clippit
open DocumentFormat.OpenXml.Packaging
open System
open System.Collections.Generic
open System.Reflection

[<AutoOpen>]
module Async =
    type AsyncWithStackTraceBuilder() =
        member __.Zero() = async.Zero()

        member __.Return t = async.Return t

        member inline __.ReturnFrom a =
            async {
                try
                    return! async.ReturnFrom a
                with e ->
                    Runtime
                        .ExceptionServices
                        .ExceptionDispatchInfo
                        .Capture(e)
                        .Throw()

                    return raise <| Exception() // this line is unreachable as the prior line throws the exception
            }

        member inline __.Bind a =
            async {
                try
                    return! async.Bind a
                with e ->
                    System
                        .Runtime
                        .ExceptionServices
                        .ExceptionDispatchInfo
                        .Capture(e)
                        .Throw()

                    return raise <| Exception() // this line is unreachable as the prior line throws the exception
            }

        member __.Combine(u, t) = async.Combine(u, t)
        member __.Delay f = async.Delay f

        member __.For(s, body) = async.For(s, body)
        member __.While(guard, computation) = async.While(guard, computation)

        member __.Using(resource, binder) = async.Using(resource, binder)

        member __.TryWith(a, handler) = async.TryWith(a, handler)
        member __.TryFinally(a, compensation) = async.TryFinally(a, compensation)

    let async = AsyncWithStackTraceBuilder()

// A scam to convince Clippit to not try to find compatible font families which references the forbidden System.Drawing (not compatible with WASM)
let warmup () =
    let ez =
        typeof<WmlToHtmlConverter>.GetField ("_knownFamilies", BindingFlags.Static ||| BindingFlags.NonPublic)

    ez.SetValue(null, HashSet<string>())

let convertToHtml name (stream: IO.Stream) =
    warmup ()

    async {

        let memoryStream = new MemoryStream()

        do!
            stream.CopyToAsync(memoryStream)
            |> Async.AwaitTask


        let wDoc =
            WordprocessingDocument.Open(memoryStream, true)

        let part = wDoc.CoreFilePropertiesPart

        let pageTitle =
            if not (isNull part) then
                string (
                    ((part.GetXDocument()).Descendants(DC.title))
                        .FirstOrDefault()
                )
            else
                name

        let settings = WmlToHtmlConverterSettings()
        settings.PageTitle <- pageTitle
        settings.FabricateCssClasses <- true
        settings.CssClassPrefix <- "docx-"
        settings.RestrictToSupportedLanguages <- false
        settings.RestrictToSupportedNumberingFormats <- false

        (*let imageHandler (imageInfo: ImageInfo) =
            let extension =
                imageInfo.ContentType
                |> String.split ["/"]
                |> Seq.tryHead
                |> map String.toLower
            let imageFormat =
                match extension with
                | Some x ->
                    match x with
                    | "png" -> Some ImageFormat.Png
                    | "gif" -> Some ImageFormat.Gif
                    | "bmp" -> Some ImageFormat.Bmp
                    | "jpeg" -> Some ImageFormat.Jpeg
                    | "tiff" -> Some ImageFormat.Gif // Need to set extension as well
                    | "x-wmf" -> Some ImageFormat.Wmf // Need to set extension as well
                    | _ -> None
                | None -> None
            printfn "testerino"
            match imageFormat with
            | Some x ->
                let base64 =
                    try
                        use (ms: MemoryStream) = new MemoryStream()
                        imageInfo.Bitmap.Save(ms, x)
                        let ba = ms.ToArray()
                        Some(System.Convert.ToBase64String(ba))
                    with :? ExternalException -> None

                let format = imageInfo.Bitmap.RawFormat

                let codec =
                    ImageCodecInfo
                        .GetImageDecoders()
                        .First(fun x -> x.FormatID = format.Guid)

                let mimeType = codec.MimeType

                let imageSource =
                    String.Format("data:{0};base64,{1}", mimeType, base64)

                XElement(
                    Xhtml.img,
                    XAttribute(NoNamespace.src, imageSource),
                    imageInfo.ImgStyleAttribute,
                    imageInfo.ImgStyleAttribute,
                    XAttribute(
                        NoNamespace.alt,
                        if isNull imageInfo.AltText then
                            ""
                        else
                            imageInfo.AltText
                    )
                )
            | None -> Unchecked.defaultof<_>*)

        settings.ImageHandler <- null

        let (htmlElement: XElement) =
            WmlToHtmlConverter.ConvertToHtml(wDoc, settings)


        let html =
            XDocument(
                XDocumentType("html", Unchecked.defaultof<_>, Unchecked.defaultof<_>, Unchecked.defaultof<_>),
                htmlElement
            )

        let htmlString =
            html.ToString(SaveOptions.DisableFormatting)

        return htmlString
    }
