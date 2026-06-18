# RcLocalizer - C++ Resource (.rc) File Localizer POC

This is a desktop Proof of Concept (POC) application built with **C# .NET 10 (compatible with .NET 8)** and **WPF** following the **MVVM architecture**. It combines a custom C++ resource parser/generator with the **Gemini API** for automated text translation.

---

## Key Features

1. **RC Parser Service**: Efficiently tokenizes `.rc` files to scan and extract translatable strings exclusively from `STRINGTABLE` blocks. It tracks exact character spans so that other elements (includes, macros, comments, registry, and formatting) are preserved.
2. **Translation Service**: Calls the Gemini API in structured **JSON Mode** to translate text, preserving formatting and placeholders. It batches items in groups of 50 to optimize performance and context coherence.
3. **Validation Service**: Ensures that:
   - Resource count matches.
   - Translations are not empty.
   - Critical formatting placeholders (`%s`, `%d`, `%f`, `%1`, `%2`, `{0}`, `{1}`, `{2}`) and escape sequences (`\n`, `\t`) are retained.
4. **RC Generator Service**: Replaces string literals in-place by applying changes from back to front using character offsets.
5. **Modern WPF UI**: Sleek dark theme featuring a comprehensive DataGrid, live validation feedback badges, progress bars, and easy save options.

---

## Directory Structure

```
/Localizer_App
│   App.xaml
│   App.xaml.cs
│   AssemblyInfo.cs
│   Localizer_App.csproj
│   sample.rc                      <-- Sample Resource File for Testing
│   README.md                      <-- You are here
│
├───Models
│       ResourceString.cs          <-- Key, Text, Translated, Offset tracking
│       TargetLanguage.cs          <-- Language metadata
│       ValidationResult.cs        <-- Validation outcomes and error messages
│
├───Helpers
│       RelayCommand.cs            <-- Standard ICommand implementation
│       ViewModelBase.cs           <-- INotifyPropertyChanged helper
│
├───Services
│       RcGeneratorService.cs      <-- In-place text replacements
│       RcParserService.cs         <-- Tokenizer & extractor
│       TranslationService.cs      <-- Gemini API Client
│       ValidationService.cs       <-- Validation checks
│
├───ViewModels
│       MainViewModel.cs           <-- Main Application Logic Coordinator
│
└───Views
        MainWindow.xaml            <-- High-quality WPF layout and styles
        MainWindow.xaml.cs         <-- Code-behind
```

---

## Setup & Running Instructions

### Prerequisites
- [.NET SDK 10.0](https://dotnet.microsoft.com/download/dotnet/10.0) or compatible installed on your machine.
- A **Gemini API Key**. You can obtain one from [Google AI Studio](https://aistudio.google.com/).

### Running the App
1. Open a command prompt or terminal in the project directory:
   ```bash
   cd "F:\Drive I\CCtech_Documents\MCP\Localizer_App"
   ```
2. Run the application:
   ```bash
   dotnet run
   ```

### Running the Tests
To verify the services and parsing logic, run:
```bash
dotnet test LocalizerApp.Tests/LocalizerApp.Tests.csproj
```

---

## Walkthrough: Testing with `sample.rc`

A test file called `sample.rc` has been pre-packaged in the root folder of the project. Here is how to test the localization workflow:

1. **Launch the Application**: Start the app by running `dotnet run`.
2. **Select the File**: Click **[Select RC File...]** and choose `sample.rc` in the project root.
3. **Select Language**: Select **Hindi**, **Japanese**, **French**, or **German** from the dropdown menu.
4. **Extract Strings**: Click **[Extract Strings]**. You will see the DataGrid fill up with 9 extracted strings, displaying keys (like `IDS_APP_TITLE` or `IDS_WELCOME_MSG`) and their original English texts.
5. **Set API Key**: Enter your Gemini API Key in the top-right field.
6. **Translate**: Click **[Translate (Gemini)]**. A progress bar will show up, and the app will request translations in a single optimized batch.
7. **Review & Edit**: Once completed, the translated text will appear in the grid. You can double-click any cell in the "Translation" column to adjust the translation manually if needed.
8. **Validation Report**: The "Validation Results" panel will display checks for:
   - Resource Count Validation (`✔` or `❌`)
   - Empty Translation Validation (`✔` or `❌`)
   - Placeholder Validation (`✔` or `❌`)
   If any placeholder (like `%s` or `%d`) is accidentally modified or omitted by the AI, validation will fail, and the specific errors will show up in the warning list.
9. **Generate and Save**:
   - Click **[Generate Localized RC]** to merge your translations back into the original file structure.
   - Click **[Save Localized RC...]** to select a folder and save the resulting file (e.g. `sample_ja-JP.rc`).
