module Entry

open Browser


[<EntryPoint>] 
let main argv =
    document.addEventListener("readystatechange", fun _ ->
        if document.readyState = "complete" then
            TodoMVC.run()
    )
    0
