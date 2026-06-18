# Technical Concepts Guide: RcLocalizer Application

This guide breaks down every key C# and WPF programming concept used in the **RcLocalizer** codebase. It is organized from the **most basic concepts** (useful for junior developers) to **most advanced concepts** (useful to explain algorithms, web networking, and parsing to project reviewers).

---

## Table of Contents
1. [Auto-Implemented Properties](#1-auto-implemented-properties)
2. [Properties with Backing Fields](#2-properties-with-backing-fields)
3. [WPF Event Routing & Handlers](#3-wpf-event-routing--handlers)
4. [File I/O (Input/Output)](#4-file-io-inputoutput)
5. [Case-Insensitive Comparisons & Dictionaries](#5-case-insensitive-comparisons--dictionaries)
6. [XML File Parsing (XDocument)](#6-xml-file-parsing-xdocument)
7. [JSON Serialization & Deserialization](#7-json-serialization--deserialization)
8. [LINQ (Language Integrated Query) Querying](#8-linq-language-integrated-query-querying)
9. [INotifyPropertyChanged & Property Bindings](#9-inotifypropertychanged--property-bindings)
10. [ObservableCollection<T>](#10-observablecollectiont)
11. [Pass-By-Reference (ref Parameter Modifier)](#11-pass-by-reference-ref-parameter-modifier)
12. [Lexical Analysis (Tokenization & Parsing)](#12-lexical-analysis-tokenization--parsing)
13. [Asynchronous Concurrency (async / await / Task)](#13-asynchronous-concurrency-async--await--task)
14. [HTTP REST Communication (HttpClient)](#14-http-rest-communication-httpclient)
15. [Reverse-Index Traversal (Back-to-Front Manipulation)](#15-reverse-index-traversal-back-to-front-manipulation)

---

## 1. Auto-Implemented Properties

### Simple Definition
An auto-implemented property is a shorthand way to create variables that other classes can read or write, without having to write detailed code to set up a private container.

### Where Used
- **File**: [ResourceString.cs](file:///d:/AutoDesk_POC/RcLocalizer/Models/ResourceString.cs)
- **Class**: `ResourceString`

### Code Example
```csharp
public string Key { get; set; } = string.Empty;
public string Text { get; set; } = string.Empty;
public string Translated { get; set; } = string.Empty;
```

### How It Works (Advanced)
Behind the scenes, the C# compiler automatically generates a hidden, private variable (called a *backing field*) and writes standard `get` and `set` methods for you. This saves code space while keeping encapsulation intact.

### Why Needed
It allows the application to cleanly define a data model to store the keys, English texts, and translations in memory.

---

## 2. Properties with Backing Fields

### Simple Definition
Unlike auto-properties, these properties have a visible private variable (a backing field) behind them. We write custom code inside the `set` action to trigger other behaviors (like notifying the UI to redraw) whenever the variable changes.

### Where Used
- **File**: [MainWindow.xaml.cs](file:///d:/AutoDesk_POC/RcLocalizer/Views/MainWindow.xaml.cs)
- **Property**: `StatusMessage`

### Code Example
```csharp
private string _statusMessage = "Ready.";
public string StatusMessage
{
    get => _statusMessage;
    set => SetProperty(ref _statusMessage, value, nameof(StatusMessage));
}
```

### How It Works (Advanced)
When `StatusMessage = "Translating..."` is called, the C# compiler executes the custom code block in the `set` assessor. It verifies if the value has changed, assigns it to the private backing field `_statusMessage`, and fires the WPF property change events.

### Why Needed
In WPF, when variables change in C# code, the UI does not know unless we specifically raise notifications. Using properties with backing fields lets us trigger notifications to refresh text blocks on the screen.

---

## 3. WPF Event Routing & Handlers

### Simple Definition
An event handler is a C# method that is linked to a visual control (like a button) in XAML. When a user interacts with the control (like clicking it), the method runs.

### Where Used
- **File**: [MainWindow.xaml](file:///d:/AutoDesk_POC/RcLocalizer/Views/MainWindow.xaml) & [MainWindow.xaml.cs](file:///d:/AutoDesk_POC/RcLocalizer/Views/MainWindow.xaml.cs)
- **Method**: `OnSelectFileClick()`

### Code Example
*XAML Setup:*
```xml
<Button Content="Select RC File..." Click="OnSelectFileClick"/>
```
*C# Handler:*
```csharp
private void OnSelectFileClick(object sender, RoutedEventArgs e)
{
    OpenFileDialog openFileDialog = new OpenFileDialog { Filter = "Resource Files (*.rc)|*.rc" };
    if (openFileDialog.ShowDialog() == true)
    {
        SelectedFilePath = openFileDialog.FileName;
    }
}
```

### How It Works (Advanced)
WPF uses a system called *Routed Events*. When a button is clicked, it raises an event that bubbles up through the visual tree. The framework listens for this event and calls the delegate signature `RoutedEventHandler` which matches the method in the code-behind class.

### Why Needed
This is the primary way the user triggers application actions (selecting files, starting translations, running validations, and saving outputs).

---

## 4. File I/O (Input/Output)

### Simple Definition
File I/O refers to standard tools that allow an application to read data from local files or save data back to files on the hard drive.

### Where Used
- **File**: [MainWindow.xaml.cs](file:///d:/AutoDesk_POC/RcLocalizer/Views/MainWindow.xaml.cs)
- **Method**: `OnExtractStringsClick()` and `OnSaveRcClick()`

### Code Example
```csharp
// Reading file
string content = File.ReadAllText(SelectedFilePath);

// Writing file
File.WriteAllText(dialog.FileName, GeneratedRcContent, Encoding.UTF8);
```

### How It Works (Advanced)
`File.ReadAllText` opens a read stream to the target file path, converts the raw bytes to a C# string using target decoding (or defaulting to UTF-8), and closes the file handle. `File.WriteAllText` opens a write stream, overwrites existing contents, flushes buffers, and locks file parameters safely.

### Why Needed
The app must load raw C++ `.rc` file structures, read local caches, and save final translated resource files.

---

## 5. Case-Insensitive Comparisons & Dictionaries

### Simple Definition
A dictionary is a fast lookup list of key-value pairs. Standard lookups are picky about letter cases (e.g. "File" is not the same as "file"). Case-insensitive settings instruct C# to ignore upper/lower case differences.

### Where Used
- **File**: [TranslationMemoryService.cs](file:///d:/AutoDesk_POC/RcLocalizer/Services/TranslationMemoryService.cs)
- **Method**: `ConvertToDict()`

### Code Example
```csharp
private Dictionary<string, string> ConvertToDict(Dictionary<string, string>? dict)
{
    if (dict == null) return new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
    return new Dictionary<string, string>(dict, StringComparer.OrdinalIgnoreCase);
}
```

### How It Works (Advanced)
When configured with `StringComparer.OrdinalIgnoreCase`, the dictionary calls character comparison mappings that treat uppercase and lowercase codes similarly during hash index calculations and retrieval equality checks.

### Why Needed
Resource keys (like `IDS_FILE` or `ids_file`) or cached English terms may vary in capitalization depending on developer styles. Ignoring case issues prevents lookup misses.

---

## 6. XML File Parsing (XDocument)

### Simple Definition
XML is a structured format that uses tags (like `<appSettings>`) to store parameters. `XDocument` is a built-in C# tool to read and edit XML elements like a tree of nodes.

### Where Used
- **File**: [MainWindow.xaml.cs](file:///d:/AutoDesk_POC/RcLocalizer/Views/MainWindow.xaml.cs)
- **Method**: `LoadApiKeyFromConfig()` and `SaveApiKeyToConfig()`

### Code Example
```csharp
string configPath = AppDomain.CurrentDomain.BaseDirectory + "App.config";
var document = System.Xml.Linq.XDocument.Load(configPath);
var element = document.Descendants("add").FirstOrDefault(e => (string?)e.Attribute("key") == "GeminiApiKey");
string key = element != null ? (string?)element.Attribute("value") : "";
```

### How It Works (Advanced)
`XDocument` loads the XML document into an in-memory tree layout. LINQ methods search descendants matching target criteria. Values can be read or modified, and changes are committed back to disk by writing the modified DOM structure.

### Why Needed
It allows loading and saving the API key to `App.config` securely without depending on bulky external packages, keeping code light.

---

## 7. JSON Serialization & Deserialization

### Simple Definition
- **Serialization**: Converting a C# object (like a list of translations) into a text string (JSON format) to send over the web or save to disk.
- **Deserialization**: Taking a JSON text string and converting it back into a C# object that the code can read.

### Where Used
- **File**: [TranslationService.cs](file:///d:/AutoDesk_POC/RcLocalizer/Services/TranslationService.cs)
- **Method**: `GetPrompt()` and `UpdateTranslations()`

### Code Example
```csharp
// Serialization (Converting list of objects to text)
string serialized = JsonSerializer.Serialize(inputs);

// Deserialization (Converting text back to objects)
var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
var outputs = JsonSerializer.Deserialize<List<TranslationOutput>>(jsonResponse, options);
```

### How It Works (Advanced)
`JsonSerializer` walks property reflection trees of target data objects and outputs structured string hierarchies. Deserialization reverses this, matching JSON keys to matching C# properties.

### Why Needed
Web APIs (like Gemini) communicate using JSON payloads. We also use JSON to save translation caches.

---

## 8. LINQ (Language Integrated Query) Querying

### Simple Definition
LINQ is a set of query tools in C# that lets you search, filter, and sort lists of data using simple, readable commands (similar to how databases query data).

### Where Used
- **File**: [RcGeneratorService.cs](file:///d:/AutoDesk_POC/RcLocalizer/Services/RcGeneratorService.cs)
- **Method**: `SortReplacements()`

### Code Example
```csharp
private List<ResourceString> SortReplacements(List<ResourceString> resourceStrings)
{
    return resourceStrings
        .Where(r => r.StartIndex >= 0 && r.EndIndex >= 0)
        .OrderByDescending(r => r.StartIndex)
        .ToList();
}
```

### How It Works (Advanced)
LINQ methods operate on `IEnumerable<T>` collections. `.Where()` applies filter functions, `.OrderByDescending()` evaluates key selectors to perform sorting, and `.ToList()` executes queries to instantiate new lists.

### Why Needed
The app needs to filter strings that have valid index spans and sort them descending to do correct text replacements.

---

## 9. INotifyPropertyChanged & Property Bindings

### Simple Definition
WPF uses **Data Binding** to link visual properties in XAML to variables in C#. The `INotifyPropertyChanged` interface is a standard contract that allows C# to send "Hey, I changed!" signals so the UI redraws the value immediately.

### Where Used
- **File**: [MainWindow.xaml.cs](file:///d:/AutoDesk_POC/RcLocalizer/Views/MainWindow.xaml.cs)
- **Method**: `OnPropertyChanged()` and `SetProperty()`

### Code Example
*XAML Binding Setup:*
```xml
<TextBlock Text="{Binding StatusMessage}"/>
```
*C# Binding Setup:*
```csharp
public event PropertyChangedEventHandler? PropertyChanged;

protected void OnPropertyChanged(string name)
{
    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}

protected void SetProperty<T>(ref T storage, T value, string name)
{
    if (EqualityComparer<T>.Default.Equals(storage, value)) return;
    storage = value;
    OnPropertyChanged(name);
}
```

### How It Works (Advanced)
When DataContext is set (`DataContext = this`), WPF builds binding listeners for target properties. When `SetProperty` is called, it raises the `PropertyChanged` event passing the property name. The WPF dispatcher intercepts the event, reads the new value from the property getter, and updates the UI control.

### Why Needed
Keeps status logs, statistics, and validation panels in sync with background operations.

---

## 10. ObservableCollection<T>

### Simple Definition
While a standard `List<T>` is great for holding objects, WPF does not know when items are added or removed from it. An `ObservableCollection<T>` is a special list that automatically alerts the UI whenever items are added, cleared, or deleted.

### Where Used
- **File**: [MainWindow.xaml.cs](file:///d:/AutoDesk_POC/RcLocalizer/Views/MainWindow.xaml.cs)
- **Field**: `ResourceStrings`

### Code Example
```csharp
public ObservableCollection<ResourceString> ResourceStrings { get; } = new();

// UI automatically adds a row when this runs:
ResourceStrings.Add(new ResourceString { Key = "IDS_OK", Text = "OK" });
```

### How It Works (Advanced)
`ObservableCollection` inherits from `Collection<T>` and implements `INotifyCollectionChanged` and `INotifyPropertyChanged`. Whenever modification events run, it raises collection change events detailing added/removed indexes and items.

### Why Needed
When strings are extracted or translated, they are loaded into this collection, which automatically populates the DataGrid rows on screen.

---

## 11. Pass-By-Reference (ref Parameter Modifier)

### Simple Definition
Normally, passing a variable (like a count) to a method sends a copy of the value. Changes inside the method do not affect the original variable. The `ref` keyword tells C# to pass the *actual pointer* to the variable so changes inside update the original value.

### Where Used
- **File**: [ValidationService.cs](file:///d:/AutoDesk_POC/RcLocalizer/Services/ValidationService.cs)
- **Method**: `CheckTranslations()` and `CheckItem()`

### Code Example
```csharp
private void CheckTranslations(List<ResourceString> original, List<ResourceString> translated, ValidationResult result)
{
    bool allNotEmpty = true;
    bool allPlaceholders = true;
    // ...
    CheckList(original, map, result, ref allNotEmpty, ref allPlaceholders);
    // allNotEmpty has been updated inside CheckList
}
```

### How It Works (Advanced)
By default, C# passes value types (like `bool`, `int`) by value. The `ref` keyword instructs the compiler to pass the memory address of the variable instead. Writing to the parameter updates the reference value directly on the stack.

### Why Needed
It allows nested checker methods to flag and update overall pass/fail boolean outcomes across lists without needing complex return structures.

---

## 12. Lexical Analysis (Tokenization & Parsing)

### Simple Definition
- **Tokenization**: Breaking down a long file of characters into distinct parts (called *Tokens*), like categorizing them as strings, keywords, numbers, or comments.
- **Parsing**: Analyzing the list of tokens to make sense of the structure (e.g. finding a `STRINGTABLE` block and pairing ID keys with their quoted text values).

### Where Used
- **File**: [RcTokenizer.cs](file:///d:/AutoDesk_POC/RcLocalizer/Services/RcTokenizer.cs) and [RcParserService.cs](file:///d:/AutoDesk_POC/RcLocalizer/Services/RcParserService.cs)

### Code Example
```csharp
// 1. Tokenizing characters
while (index < text.Length)
{
    index = ParseNext(text, index, tokens);
}

// 2. Parsing tokens
if (token.Type == TokenType.StringLiteral && state.LastKeyToken != null)
{
    AddResourceString(token, state.LastKeyToken.Value, resourceStrings);
}
```

### How It Works (Advanced)
Lexical tokenizers read character positions, skipping comments (`//` or `/*`) and preprocessor lines (`#`) using state-based conditions. The parser reads these tokens like a state machine, tracking blocks (nesting level) to ensure only valid string table elements are processed.

### Why Needed
Regular text search or Regex is too brittle for Autodesk `.rc` files. Comments, nested macros, and special quotes require a solid parsing engine to avoid corruption.

---

## 13. Asynchronous Concurrency (async / await / Task)

### Simple Definition
Asynchronous programming allows the application to start long tasks (like calling the Gemini API) in the background without freezing the desktop window. The UI remains responsive (you can click other buttons or move the window) while waiting.

### Where Used
- **File**: [MainWindow.xaml.cs](file:///d:/AutoDesk_POC/RcLocalizer/Views/MainWindow.xaml.cs)
- **Method**: `OnTranslateClick()` and `ExecuteTaskAsync()`

### Code Example
```csharp
private async void OnTranslateClick(object sender, RoutedEventArgs e)
{
    // Why: Execute translation workflow asynchronously.
    await ExecuteTaskAsync(RunTranslationFlow);
}
```

### How It Works (Advanced)
The compiler translates `async/await` into a complex state machine. When an `await` instruction is met, the method yields control, allowing the UI thread to continue rendering. Once the task finishes, execution resumes on the original thread context.

### Why Needed
Calling the Gemini translation API over the network takes time. Asynchronous tasks prevent the UI from freezing and appearing unresponsive to users.

---

## 14. HTTP REST Communication (HttpClient)

### Simple Definition
HttpClient is a built-in C# tool that acts like an invisible web browser. It makes network connections to send data (POST requests) to internet servers and read responses.

### Where Used
- **File**: [GeminiService.cs](file:///d:/AutoDesk_POC/RcLocalizer/Services/GeminiService.cs)
- **Method**: `CallApiAsync()`

### Code Example
```csharp
private static readonly HttpClient Client = new HttpClient();

public async Task<string> CallApiAsync(string systemInstruction, string prompt, string apiKey, string model)
{
    string url = "https://generativelanguage.googleapis.com/v1beta/models/" + model + ":generateContent?key=" + apiKey;
    string requestJson = BuildRequestJson(systemInstruction, prompt);
    StringContent content = new StringContent(requestJson, Encoding.UTF8, "application/json");
    HttpResponseMessage response = await Client.PostAsync(url, content);
    return await ParseResponseAsync(response);
}
```

### How It Works (Advanced)
`HttpClient` establishes connection pools, handles SSL handshakes, serializes headers, and sends request bodies over TCP/IP sockets to the API. It reads the response streams and returns standard HTTP statuses and content strings.

### Why Needed
It is the bridge that allows the application to connect to the Google Gemini AI models for translating and validating text.

---

## 15. Reverse-Index Traversal (Back-to-Front Manipulation)

### Simple Definition
When editing text, replacing `"A"` with `"LongText"` pushes the positions of all subsequent text forward. This invalidates all index offsets. Replacing strings starting from the **end of the file** avoids changing index positions for preceding text.

### Where Used
- **File**: [RcGeneratorService.cs](file:///d:/AutoDesk_POC/RcLocalizer/Services/RcGeneratorService.cs)
- **Method**: `SortReplacements()`

### Code Example
```csharp
private List<ResourceString> SortReplacements(List<ResourceString> resourceStrings)
{
    // Why: Filter valid string indices and sort descending by start offset.
    return resourceStrings
        .Where(r => r.StartIndex >= 0 && r.EndIndex >= 0)
        .OrderByDescending(r => r.StartIndex) // Sorts highest index first
        .ToList();
}
```

### How It Works (Advanced)
If a string replacement occurs at index `1000`, the file length changes. But because we sort descending, the next replacement occurs at a smaller index (e.g. `500`). The character shift at `1000` does not affect the character positions of preceding items.

### Why Needed
Ensures translated strings are merged in the exact correct slots in the target file without corrupting surrounding C++ macros or layout parameters.
