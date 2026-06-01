module TermProject.Characters

type CharacterMarker =
  { DefaultRight: string option
    DefaultLeft: string option
    ActionRight: string option
    ActionLeft: string option }

type Character =
  { Marker: CharacterMarker
    Price: int
    Unlocked: bool }

module Character =
  let create (info: Character) : Character =
    assert (info.Marker.DefaultRight <> None) // minimal condition

    { info with
        Marker =
          { DefaultRight = info.Marker.DefaultRight
            DefaultLeft = info.Marker.DefaultLeft |> Option.orElse info.Marker.DefaultRight
            ActionRight = info.Marker.ActionRight |> Option.orElse info.Marker.DefaultRight
            ActionLeft =
              info.Marker.ActionLeft
              |> Option.orElse info.Marker.DefaultLeft
              |> Option.orElse info.Marker.DefaultRight } }

let characterList: Character list =
  List.map Character.create
  <| [ { Marker =
           { DefaultRight = Some "🚶‍➡️"
             DefaultLeft = Some "🚶"
             ActionRight = Some "🏃‍➡️"
             ActionLeft = Some "🏃" }
         Price = 0
         Unlocked = true }
       { Marker =
           { DefaultRight = Some "🚶‍♂️‍➡️"
             DefaultLeft = Some "🚶‍♂️"
             ActionRight = Some "🏃‍♂️‍➡️"
             ActionLeft = Some "🏃‍♂️" }
         Price = 50
         Unlocked = false }
       { Marker =
           { DefaultRight = Some "👾"
             DefaultLeft = None
             ActionRight = None
             ActionLeft = None }
         Price = 100
         Unlocked = false }
       { Marker =
           { DefaultRight = Some "🤜"
             DefaultLeft = Some "🤛"
             ActionRight = Some "🤜"
             ActionLeft = Some "🤛" }
         Price = 50
         Unlocked = false }
       { Marker =
           { DefaultRight = Some "🌜"
             DefaultLeft = Some "🌛"
             ActionRight = Some "🌜"
             ActionLeft = Some "🌛" }
         Price = 100
         Unlocked = false }
       { Marker =
           { DefaultRight = Some "🦈"
             DefaultLeft = None
             ActionRight = None
             ActionLeft = None }
         Price = 100
         Unlocked = false }
       { Marker =
           { DefaultRight = Some "🤷‍♀️"
             DefaultLeft = None
             ActionRight = None
             ActionLeft = None }
         Price = 67
         Unlocked = false } ]
