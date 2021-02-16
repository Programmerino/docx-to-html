module tester.Client.Bindings

open Bolero.Html
open MudBlazor
open Bolero

module Bindings =
    let mudRequired =
        div [] [
            comp<MudThemeProvider> [] []
            (*comp<MudThemeProvider> [] []
            comp<MudDialogProvider> [] []
            comp<MudSnackbarProvider> [] []*)
        ]

    let bBuild typ def x = typ (def @ x)

    let mudCard = bBuild comp<MudText> [] []

    let mudCardMedia (image: string) =
        bBuild comp<MudCardMedia> [ "Image" => image ]

    let mudCardContent = bBuild comp<MudCardContent> [] []

    let mudLayout = bBuild comp<MudLayout> [] []

    let mudAppBar = bBuild comp<MudAppBar> []

    let mudIconButton (icon: string) =
        bBuild comp<MudIconButton> [ "Icon" => icon ]

    let mudAppBarSpacer = bBuild comp<MudAppBarSpacer> [] [] []

    let mudText (typo: Typo) = bBuild comp<MudText> [ "Typo" => typo ]

    let mudDrawer = bBuild comp<MudDrawer> []

    let mudDrawerHeader = bBuild comp<MudDrawerHeader> [] []

    let mudMainContent = bBuild comp<MudMainContent> [] []

    let mudButton = bBuild comp<MudButton> []

    let mudTextField = bBuild comp<MudTextField<_>> []


module Wireframe =
    open Bindings

    type Model =
        { DrawerOpen: bool
          ShowDrawer: bool
          ShowMore: bool
          NavMenu: Node list
          Body: Node list
          GithubLink: string }

    let initModel =
        { DrawerOpen = false
          ShowDrawer = true
          ShowMore = true
          NavMenu = [ text "Not provided..." ]
          Body = [ text "Not provided..." ]
          GithubLink = "https://github.com/Programmerino" }

    let init () = initModel

    type Message = | ToggleDrawer

    let update message model =
        match message with
        | ToggleDrawer ->
            { model with
                  DrawerOpen = not model.DrawerOpen }

    let strictView model dispatch =
        div [] [
            mudRequired
            mudLayout [ mudAppBar [ "Elevation" => 1 ] [
                            mudIconButton
                                Icons.Custom.Brands.GitHub
                                [ "Color" => Color.Inherit
                                  "Edge" => Edge.End
                                  "Link" => model.GithubLink
                                  "Target" => "_blank" ]
                                []
                        ]

                        mudMainContent model.Body ]
        ]

    let view model dispatch github showdrawer showmore navMenu body =
        lazyComp2
            strictView
            { model with
                  NavMenu = navMenu
                  Body = body
                  GithubLink = github
                  ShowDrawer = showdrawer
                  ShowMore = showmore }
            dispatch
