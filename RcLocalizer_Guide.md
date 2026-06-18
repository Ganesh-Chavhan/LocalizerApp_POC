# RcLocalizer - Comprehensive Developer & Presentation Guide

Welcome to the **RcLocalizer** comprehensive guide! This document is designed for junior developers, interns, and presenters. It explains the entire project from basic concepts to low-level implementation details, helping you confidently present this Proof of Concept (PoC) to leadership at **CCTech** and **Autodesk**.

---

## Table of Contents
1. [Project Overview](#1-project-overview)
2. [What the Project Does (Step-by-Step)](#2-what-the-project-does-step-by-step)
3. [Project Architecture](#3-project-architecture)
4. [Execution Flow (Button Click Triggers)](#4-execution-flow-button-click-triggers)
5. [Code Flow (Under the Hood)](#5-code-flow-under-the-hood)
6. [Each Code File Explained](#6-each-code-file-explained)
7. [App.config & Key Security](#7-appconfig--key-security)
8. [Input & Output Examples](#8-input--output-examples)
9. [Key Dependencies](#9-key-dependencies)
10. [Common Issues & Solutions](#10-common-issues--solutions)

---

## 1. Project Overview

### What is RcLocalizer?
Imagine you have a desktop software program (like AutoCAD) that was originally written in English. To sell this software in Japan, Germany, or India, all the buttons, menus, and message boxes need to speak the local language. 

**RcLocalizer** is a desktop tool that automatically reads the English text files of a C++ application, translates the words into a target language (like Japanese, Hindi, French, or German) using Google's Gemini AI, verifies that the translations are safe to use, and creates new, translated language files.

### What Problem Does It Solve?
Traditional desktop applications (especially C++ programs used by Autodesk) store their user interface strings in special files called **Resource Scripts (`.rc` files)**. 
- Translating these files manually is slow and expensive.
- Simple translation tools often corrupt the code formatting, comments, or special formatting variables (like `%s` or `%d`), which causes the application to crash.
- Calling cloud translation APIs repeatedly for identical strings wastes money and time.

RcLocalizer solves this by safely parsing the file structure, caching translations locally to avoid duplicate API fees, and running local structural validations combined with AI language reviews to ensure translation quality.

### Who Uses It?
- **CCTech Development Teams**: To automate the localizing process of legacy desktop software.
- **Autodesk Product Managers & Engineers**: To easily translate and preview CAD tool design windows in international markets.

### Why Is It Important?
It brings **Automation**, **Safety**, and **Visual Verification** together in one tool. Instead of waiting weeks for manual translators, developers can localize and visually preview a complex engineering UI in seconds, knowing that the structural integrity of their code formatting placeholders is protected.

---

## 2. What the Project Does (Step-by-Step)

```
[Original English .rc File]
           │
           ▼
  [Step 1: Extract Strings]  ───► Reads and tokenizes the string tables
           │
           ▼
  [Step 2: Translate]        ───► Checks local cache, sends misses to Gemini
           │
           ▼
  [Step 3: Validate]         ───► Runs local check & AI QA review
           │
           ▼
  [Step 4: Generate & Save]  ───► Merges back-to-front into a new localized .rc
```

### Feature 1: Parse `.rc` Files
- **What is an `.rc` file?** It is a C++ resource script. It contains layout information for menus and dialogs, along with a `STRINGTABLE` block where text strings are stored alongside their unique ID keys.
- **How does it extract strings?** The parser reads the file character-by-character, identifying comments, macro directives, and brackets. When it finds a `STRINGTABLE` block, it extracts only the unique ID key (e.g., `IDS_FILE`) and the English text inside the double quotes.
- **Example Input/Output**:
  - *Input line in `.rc` file*: `IDS_OPEN "Open Project"`
  - *Extracted Data*: Key = `IDS_OPEN`, English Text = `Open Project`.

### Feature 2: Translate Strings
- **How it uses Gemini API**: The application packages extracted strings into a compact JSON array and sends them to the Gemini API using **JSON Mode** (`responseMimeType = "application/json"`). This forces the AI to reply in a clean JSON format without conversational remarks.
- **How it maintains a glossary (Cache)**: Before calling Gemini, the app checks a local folder (`TranslationMemory`). If a string has been translated before, it loads the translation instantly from disk. This is called a **Cache Hit**. If it's new, it is a **Cache Miss** and is sent to Gemini.
- **Example Translation Process**:
  - English: `"New Project"` ➜ Translation Memory Lookup (Not found) ➜ Gemini Request ➜ Japanese: `"新規プロジェクト"` ➜ Saved to cache.

### Feature 3: Validate Translations
- **What validations does it do?**
  1. *Resource Count Check*: Ensures no translation strings were dropped.
  2. *Empty Check*: Detects if any translation came back blank.
  3. *Placeholder Check*: Crucially verifies that formatting tokens (like `%s`, `%d`, or `{0}`) remain intact.
- **What errors does it catch?** If the English text has `"%s File"`, and the AI outputs `"ファイル"` (forgetting the `%s`), the validation catches the mismatch and flags it in bright red text.
- **Example Validation Results**:
  - `IDS_FORMAT "Open %s"` ➜ Translated to `"打开"` (Missing `%s`) ➜ Validation Result: `❌ [Placeholder Mismatch] count has changed.`

### Feature 4: Generate Localized `.rc` Files
- **How it creates new `.rc` files**: It copies the original English `.rc` file and replaces the text segments inside the quote marks with the translated values.
- **How it preserves structure**: By sorting the resource strings by their character positions in descending order (from the end of the file to the beginning), the generator can swap text without changing or shifting the positions of preceding strings. All code spacing, preprocessor macros (`#define`), and line comments are preserved exactly as they were.

---

## 3. Project Architecture

The application is structured into three clean layers to separate concerns, making it highly modular and easy to explain:

```
┌─────────────────────────────────────────────────────────────────┐
│                           UI LAYER                              │
│  [MainWindow.xaml]                    [PreviewWindow.xaml]      │
│  [MainWindow.xaml.cs] (DataContext)   [PreviewWindow.xaml.cs]   │
└────────────────────────────────┬────────────────────────────────┘
                                 │
                                 ▼ (Direct C# Method Calls)
┌─────────────────────────────────────────────────────────────────┐
│                         SERVICE LAYER                           │
│  [RcTokenizer]              [RcParserService]                   │
│  [RcGeneratorService]        [RcResourceLoaderService]           │
│  [TranslationService]        [TranslationMemoryService]         │
│  [ValidationService]         [AiValidationService]              │
│  [GeminiService] (REST Client Client)                           │
└────────────────────────────────┬────────────────────────────────┘
                                 │
                                 ▼ (Data Holders)
┌─────────────────────────────────────────────────────────────────┐
│                         MODEL LAYER                             │
│  [ResourceString.cs]  [TargetLanguage.cs]  [ValidationResult.cs]│
└────────────────────────────────┬────────────────────────────────┘
                                 │
                                 ▼ (REST API Call via HTTP)
┌─────────────────────────────────────────────────────────────────┐
│                       EXTERNAL SERVICES                         │
│                    [Google Gemini API]                          │
└─────────────────────────────────────────────────────────────────┘
```

### How Layers Communicate
1. **User Interaction**: The user clicks a button on the UI (e.g., "Extract Strings").
2. **UI Triggers Service**: The code-behind `MainWindow.xaml.cs` calls the corresponding method in a service (e.g., `RcParserService.Parse(text)`).
3. **Service Manipulates Models**: The service processes raw data and returns easy-to-use C# objects (e.g., `List<ResourceString>`).
4. **Data Binding Updates UI**: The list is stored in an `ObservableCollection` bound to the DataGrid. WPF automatically detects this change and renders the new rows on screen.

---

## 4. Execution Flow

Here is exactly what happens when the user clicks each button in the application:

### Flow 1: User clicks "Select RC File"
1. **Trigger**: User clicks `[Select RC File...]`.
2. **UI Action**: An `OpenFileDialog` opens, filtering for files ending in `.rc`.
3. **Execution**: The user selects a file (e.g. `sample.rc`).
4. **Result**: `SelectedFilePath` is updated, the previous grid strings are cleared, and the status bar displays: *"File selected: sample.rc. Press 'Extract Strings' to read entries."*

### Flow 2: User clicks "Extract Strings"
1. **Trigger**: User clicks `[Extract Strings]`.
2. **Execution**: 
   - Code-behind reads the entire file content into memory.
   - It calls `RcParserService.Parse(content)`.
   - The parser tokenizes the text and returns a list of `ResourceString` items containing keys, values, and character start/end positions.
3. **Result**: The `ResourceStrings` collection is populated, and the strings are displayed in the left DataGrid. Status updates to: *"Extracted X strings. Ready."*

### Flow 3: User clicks "Translate"
1. **Trigger**: User clicks `[Translate (Gemini)]`.
2. **Execution**:
   - The app reads the API Key from the input fields and loads the translation memory cache for the selected target language (e.g., `ja-JP.json`).
   - It iterates through the strings: matches are loaded from cache, and misses are grouped into batches of 50.
   - For misses, `TranslationService` calls `GeminiService` to send a prompt to the Gemini API.
   - Gemini returns a JSON array of translations.
   - The app updates the cache file with the new translations.
3. **Result**: The DataGrid updates to show the translated text in the "Translation" column. Cache hits/misses statistics are updated on the screen.

### Flow 4: User clicks "Validate"
1. **Trigger**: User clicks `[Validate (Gemini)]`.
2. **Execution**:
   - The app sends the translated strings to `AiValidationService`, which calls the Gemini API to score the translations (0-100) and provide quality reviews.
   - It passes the collection through the local `ValidationService` to check placeholder counts and empty values.
   - The code-behind sets `ValidationPanel.Visibility` to `Visibility.Visible`.
3. **Result**: The right-side **Validation Results Panel** slides into view. It displays validation badges (✔/❌), the detailed list of formatting errors, the overall AI QA score, and individual score ratings.

---

## 5. Code Flow (Under the Hood)

Let's look at how the code executes step-by-step inside the key services.

### When `.rc` File is Parsed:
1. `RcParserService.Parse(string content)` is called.
2. It passes the raw content to `RcTokenizer.Tokenize()`. The tokenizer runs a loop, using small helper methods like `SkipComment()` and `ReadStringLiteral()` to convert raw text characters into structured tokens (`Identifier`, `StringLiteral`, `Number`, etc.).
3. The parser processes these tokens in sequence. When it detects a `TokenType.StringTable`, it sets `ParserState.InStringTable = true`.
4. While inside the string table, if it reads an `Identifier` or `Number`, it stores it as the `LastKeyToken`.
5. When it reads a `StringLiteral` next, it matches it with the key, unescapes double-quotes using `UnescapeRcString()`, and adds a new `ResourceString` object containing the start and end positions of the string in the file.

### When Strings are Translated:
1. `TranslationService.TranslateAsync()` receives the list of cache-miss resource strings.
2. It loops through the items in batches of 50 using `.Skip(i).Take(50)`.
3. For each batch, it generates a JSON array string representing the key and original text.
4. It calls `GeminiService.CallApiAsync()`, passing a system prompt asking the model to behave as a translation system and return output strictly in JSON.
5. `GeminiService` uses `HttpClient` to send a POST request to the Google API endpoint.
6. The JSON string returned by Gemini is parsed, and the translation fields in the matching `ResourceString` items are updated.

### When Validation Runs:
1. `ValidationService.Validate()` is called.
2. It checks if the count of original strings equals the translated strings.
3. It maps the translated elements into a dictionary by Key.
4. For each string, it checks if the translation is null or empty. If so, it logs an error.
5. If not empty, it loops through the formatting tokens list (`%s`, `%d`, `{0}`, etc.). It counts occurrences in both the English string and the translation using `IndexOf()`. If the counts do not match, it adds a descriptive message to the validation error list.

### When `.rc` File is Generated:
1. `RcGeneratorService.Generate()` receives the original file text and the list of translated strings.
2. It filters out items with invalid coordinates and sorts them in descending order by `StartIndex` (back-to-front).
3. It loops through each string, gets the translated text (falling back to English if untranslated), and calls `EscapeRcString()` (which replaces internal double quotes with `""` and wraps the result in enclosing quotes).
4. It calls `StringBuilder.Remove()` and `StringBuilder.Insert()` on the raw text builder using the tracked start index and length.
5. The resulting in-memory string is saved to disk using `File.WriteAllText()`.

---

## 6. Each Code File Explained

### Views (User Interface)

#### [MainWindow.xaml](file:///d:/AutoDesk_POC/RcLocalizer/Views/MainWindow.xaml)
- **Purpose**: Defines the layout and styles for the main desktop application.
- **Controls**:
  - *Header*: Features the title, model selector, masked API key password box, and the toggle reveal button.
  - *Controls Panel*: Card containing target file selection, target language selector, translation memory hits/misses metrics, and the main action buttons.
  - *Split Panels*: Main grid containing the DataGrid on the left and the validation results panel on the right.
  - *Footer*: Action buttons to trigger design preview, generate, and save the translated script file.

#### [MainWindow.xaml.cs](file:///d:/AutoDesk_POC/RcLocalizer/Views/MainWindow.xaml.cs)
- **Purpose**: Handles all GUI interactions, local bindings, and orchestrates the translation and validation flows.
- **Key Methods**:
  - `OnSelectFileClick()`: Triggers the dialog to select the target resource file.
  - `OnExtractStringsClick()`: Calls parser to read the file and update the grid.
  - `OnTranslateClick()`: Checks local cache, sends misses to Gemini, and updates the display.
  - `OnValidateClick()`: Triggers local validation and AI quality reviews, displaying the validation results panel.
  - `OnGenerateRcClick()`: Merges translations back into the `.rc` file structure in memory.
  - `OnSaveRcClick()`: Writes the generated resource script content to a chosen file path.

#### [PreviewWindow.xaml](file:///d:/AutoDesk_POC/RcLocalizer/Views/PreviewWindow.xaml)
- **Purpose**: A mock CAD design studio dialog simulating buttons, menu bars, and properties panels to preview localized UI strings.

#### [PreviewWindow.xaml.cs](file:///d:/AutoDesk_POC/RcLocalizer/Views/PreviewWindow.xaml.cs)
- **Purpose**: Handles dropdown selection changes in the preview dialog, loads target localized `.rc` files using `RcResourceLoaderService`, and notifies bindings to refresh text.

---

### Models (Data Containers)

#### [ResourceString.cs](file:///d:/AutoDesk_POC/RcLocalizer/Models/ResourceString.cs)
- **Purpose**: Data model containing properties for a single extracted resource item.
- **Key Properties**:
  - `Key` (e.g. `IDS_TITLE`)
  - `Text` (original English string)
  - `Translated` (target language translation)
  - `StartIndex` / `EndIndex` (character index offsets in the file)
  - `ValidationScore` / `ValidationStatus` / `ValidationFeedback` (AI validation results)

#### [TargetLanguage.cs](file:///d:/AutoDesk_POC/RcLocalizer/Models/TargetLanguage.cs)
- **Purpose**: Lightweight model to store metadata for target languages (e.g. `Name = "Japanese"`, `CultureCode = "ja-JP"`).

#### [ValidationResult.cs](file:///d:/AutoDesk_POC/RcLocalizer/Models/ValidationResult.cs)
- **Purpose**: Holds summary metrics for local structural checks, including pass/fail booleans and a list of specific error messages.

#### [ValStats.cs](file:///d:/AutoDesk_POC/RcLocalizer/Models/ValStats.cs)
- **Purpose**: Holds temporary counter values (excellent count, good count, average score sum) when evaluating AI validation.

---

### Services (Business Logic)

#### [RcTokenizer.cs](file:///d:/AutoDesk_POC/RcLocalizer/Services/RcTokenizer.cs)
- **Purpose**: Lexical scanner that walks character-by-character through an RC file, stripping comments and directives while grouping text blocks into list of `Token` items.

#### [RcParserService.cs](file:///d:/AutoDesk_POC/RcLocalizer/Services/RcParserService.cs)
- **Purpose**: Processes the token list, identifies active `STRINGTABLE` blocks, matches identifiers with string values, and outputs resource lists.

#### [RcGeneratorService.cs](file:///d:/AutoDesk_POC/RcLocalizer/Services/RcGeneratorService.cs)
- **Purpose**: Reconstructs the localized file by replacing strings from back-to-front and formatting double quotes.

#### [TranslationService.cs](file:///d:/AutoDesk_POC/RcLocalizer/Services/TranslationService.cs)
- **Purpose**: Splits translation queues into batches of 50, requests JSON responses from `GeminiService`, and assigns translations.

#### [AiValidationService.cs](file:///d:/AutoDesk_POC/RcLocalizer/Services/AiValidationService.cs)
- **Purpose**: Uses `GeminiService` to call the Gemini API and analyze translation accuracy, natural grammar, and UI suitability.

#### [ValidationService.cs](file:///d:/AutoDesk_POC/RcLocalizer/Services/ValidationService.cs)
- **Purpose**: Locally counts formatting placeholders (`%s`, `%d`, `{0}`) to prevent runtime application crashes.

#### [TranslationMemoryService.cs](file:///d:/AutoDesk_POC/RcLocalizer/Services/TranslationMemoryService.cs)
- **Purpose**: Saves and loads translations locally to JSON files, serving as a cache memory layer.

#### [RcResourceLoaderService.cs](file:///d:/AutoDesk_POC/RcLocalizer/Services/RcResourceLoaderService.cs)
- **Purpose**: Loads and parses specific language files for previewing inside the simulated design studio window.

#### [GeminiService.cs](file:///d:/AutoDesk_POC/RcLocalizer/Services/GeminiService.cs)
- **Purpose**: Handles HTTP POST connections to the Gemini API, serializes request JSON payloads, and extracts candidate responses.

---

## 7. App.config & Key Security

### Purpose
To protect the user's secret Gemini API Key, it must not be hardcoded inside the C# files. The application stores configuration parameters in `App.config` inside the project root.

### Structure of [App.config](file:///d:/AutoDesk_POC/RcLocalizer/App.config)
```xml
<?xml version="1.0" encoding="utf-8" ?>
<configuration>
  <appSettings>
    <add key="GeminiApiKey" value="AIzaSyYourActualKeyHere..." />
    <add key="DefaultModel" value="gemini-2.5-flash" />
  </appSettings>
</configuration>
```

### Accessing App.config in Code
To avoid package dependencies and keep the code simple for presentation, the application uses built-in XML parsing to read and write key settings.

- **Reading Key**:
  ```csharp
  string configPath = AppDomain.CurrentDomain.BaseDirectory + "App.config";
  var document = XDocument.Load(configPath);
  var element = document.Descendants("add").FirstOrDefault(e => (string)e.Attribute("key") == "GeminiApiKey");
  string apiKey = element?.Attribute("value") ?? "";
  ```
- **Writing Key**:
  ```csharp
  var element = document.Descendants("add").FirstOrDefault(e => (string)e.Attribute("key") == "GeminiApiKey");
  element.SetAttributeValue("value", newKey);
  document.Save(configPath);
  ```

---

## 8. Input & Output Examples

### Input: English C++ Resource File (`sample.rc`)
```cpp
// English (United States) resources
LANGUAGE 9, 1

STRINGTABLE
BEGIN
    IDS_FILE                "File"
    IDS_OPEN                "Open %s Project"
    IDS_WELCOME             "Welcome to Autodesk CCTech Localizer!"
END
```

### Output: Japanese Localized Resource File (`ja-JP.rc`)
Notice how formatting spacing, header comments, and macro instructions are fully preserved:
```cpp
// English (United States) resources
LANGUAGE 9, 1

STRINGTABLE
BEGIN
    IDS_FILE                "ファイル"
    IDS_OPEN                "打开 %s 项目"
    IDS_WELCOME             "Autodesk CCTech Localizerへようこそ！"
END
```

---

## 9. Key Dependencies

1. **.NET Runtime (net10.0-windows / net8.0-windows)**: Provides the execution platform and base runtime libraries.
2. **WPF Framework (Windows Presentation Foundation)**: Provides the graphical framework for desktop layouts, UI styling, and data-binding controls.
3. **Google Gemini API Beta Endpoint**: The external AI service used for translation and QA validation.
4. **System Libraries Used**:
   - `System.Net.Http`: For sending HTTP requests to the Gemini API.
   - `System.Text.Json`: For serializing and deserializing JSON payloads.
   - `System.Xml.Linq`: For reading and saving settings in `App.config`.

---

## 10. Common Issues & Solutions

### Issue 1: Special Characters in Strings (Double Quotes)
- **Problem**: In C++ `.rc` files, quotes inside string literals are escaped by doubling them (e.g. `"Hello ""World"""`). Normal text files use standard escape characters (like `\"`). If written incorrectly, the resource compiler fails.
- **Solution**: 
  - When parsing, `UnescapeRcString()` converts double quotes (`""`) into standard single quotes (`"`).
  - When generating, `EscapeRcString()` replaces single quotes (`"`) back into double-quotes (`""`) and wraps the final output in enclosing quotes.

### Issue 2: API Rate Limiting (Too Many Requests)
- **Problem**: Sending each string to Gemini individually results in high network latency and triggers API rate limit errors (HTTP 429).
- **Solution**: The app groups translation items into batches of 50, and AI validation items into batches of 25. This reduces API request overhead by 98%.

### Issue 3: Missing `.rc` Localized File
- **Problem**: When launching the preview window, a localized `.rc` file (like `hi-IN.rc`) might not exist yet.
- **Solution**: `PreviewWindow.xaml.cs` checks if the target file exists. If it is missing, it falls back to loading `en-US.rc` automatically, ensuring the preview does not display empty labels.

### Issue 4: Empty Translations
- **Problem**: An AI glitch or connection loss might return blank translations, which would wipe out strings in the target file.
- **Solution**: The local `ValidationService` runs empty-checks. If a string is empty, the validation fails, displays a red error warning, and prevents saving corrupted files.
