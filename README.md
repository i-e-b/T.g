# T·g
A simple C# generator for HTML/XML

## Why?
Sometimes you have a project where a lot of code is needed to output
a little markup. For those projects, markup templating engines make things more difficult,
and a lot of the time you end up doing string concatenation.
T·g is a step up from string concatenation.

## Usage

See the `TagTests.cs` file for examples of every feature. Here are some overviews to get you started.

### Basic usage
```csharp
var tag = T.g("div", "class","glass")[
             T.g("a", "href", "#")["Fish"],
             T.g("br/")
          ];

Console.WriteLine(tag.ToString());
```
Outputs `<div class="glass"><a href="#">Fish</a><br/></div>`

### Writing to a stream
```csharp
var tag = T.g( . . . );
tag.StreamTo(myWritableStream, Encoding.UTF8); // if called multiple times, this will write another copy of the tag
```

### Templating and injection pattern
```csharp
var doc = HtmlDoc("Hello World", out var head, out var body);

body.Add(T.g("h1")["Hello, world"]);
body.Add(T.g("p")["This is a simple HTML page example"]);

return doc.ToString();

.
.
.

TagContent HtmlDoc(string title, out TagContent head, out TagContent body)
{
    var html = T.g("html");

    head = T.g("head")[
                    T.g("title")[title],
                    T.g("style").LoadFile("Styles/PageStyle.css")
                ]
            ];
    
    body = T.g("body")[T.g("script").LoadFile("Scripts/PageScript.js")];

    html.Add(head);
    html.Add(body);

    return html;
}
```
