using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Localizer_App.Models;
using Localizer_App.Services;

namespace Localizer_App.Views
{
    public partial class PreviewWindow : Window, INotifyPropertyChanged
    {
        // Why: Window to preview visual localization using the extracted resources.
        private readonly string _resourcesDir;
        private readonly RcResourceLoaderService _loader = new RcResourceLoaderService();
        private Dictionary<string, string> _resources = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        public event PropertyChangedEventHandler? PropertyChanged;

        public PreviewWindow(string resourcesDir)
        {
            // Why: Initialize components, set data context, load languages, and set default selection.
            InitializeComponent();
            _resourcesDir = resourcesDir;
            DataContext = this;
            LoadLanguages();
            LanguageCombo.SelectedIndex = 0;
        }

        private void LoadLanguages()
        {
            // Why: Populate available languages based on existing localized files.
            var list = new List<TargetLanguage>
            {
                new TargetLanguage { Name = "English", CultureCode = "en-US" },
                new TargetLanguage { Name = "Hindi", CultureCode = "hi-IN" },
                new TargetLanguage { Name = "Japanese", CultureCode = "ja-JP" },
                new TargetLanguage { Name = "French", CultureCode = "fr-FR" },
                new TargetLanguage { Name = "German", CultureCode = "de-DE" }
            };
            LanguageCombo.ItemsSource = list.Where(x => File.Exists(Path.Combine(_resourcesDir, x.CultureCode + ".rc"))).ToList();
        }

        private void OnLanguageSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Why: Trigger reload of localized strings when user selects a different language.
            var selected = LanguageCombo.SelectedItem as TargetLanguage;
            if (selected != null)
            {
                LoadResources(selected.CultureCode);
            }
        }

        private void OnReloadClick(object sender, RoutedEventArgs e)
        {
            // Why: Reload the resources from the current language file.
            var selected = LanguageCombo.SelectedItem as TargetLanguage;
            if (selected != null) LoadResources(selected.CultureCode);
        }

        private void LoadResources(string cultureCode)
        {
            // Why: Load resource file and fallback to English if the file is missing.
            string file = Path.Combine(_resourcesDir, cultureCode + ".rc");
            if (!File.Exists(file)) file = Path.Combine(_resourcesDir, "en-US.rc");
            _resources = _loader.LoadFromFile(file);
            StatusMessage = "Loaded: " + cultureCode + ".rc (" + _resources.Count + " strings)";
            NotifyAllProperties();
        }

        private string GetText(string key, string fallback)
        {
            // Why: Safely read resource values and return default label if not present.
            return _resources.TryGetValue(key, out string? value) ? value : fallback;
        }

        private void OnPropertyChanged(string name)
        {
            // Why: Notify binding engines that a property changed.
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        private void NotifyAllProperties()
        {
            // Why: Refresh all UI string bindings using a list iteration to fit under 10 lines.
            string[] names = { nameof(FileMenu), nameof(EditMenu), nameof(ViewMenu), nameof(HelpMenu), 
                               nameof(NewProjectTool), nameof(OpenTool), nameof(SaveTool), nameof(ExportTool), 
                               nameof(PropertiesTitle), nameof(NameLabel), nameof(DescriptionLabel), 
                               nameof(CategoryLabel), nameof(StatusLabel), nameof(SaveButton), 
                               nameof(CancelButton), nameof(ApplyButton), nameof(ReadyStatus) };
            foreach (string name in names) OnPropertyChanged(name);
        }

        // Properties bound in XAML
        private string _statusMessage = string.Empty;
        public string StatusMessage
        {
            get => _statusMessage;
            set { _statusMessage = value; OnPropertyChanged(nameof(StatusMessage)); }
        }

        public string FileMenu => GetText("IDS_FILE", "File");
        public string EditMenu => GetText("IDS_EDIT", "Edit");
        public string ViewMenu => GetText("IDS_VIEW", "View");
        public string HelpMenu => GetText("IDS_HELP", "Help");
        public string NewProjectTool => GetText("IDS_NEW_PROJECT", "New Project");
        public string OpenTool => GetText("IDS_OPEN", "Open");
        public string SaveTool => GetText("IDS_SAVE", "Save");
        public string ExportTool => GetText("IDS_EXPORT", "Export");
        public string PropertiesTitle => GetText("IDS_PROPERTIES", "Properties");
        public string NameLabel => GetText("IDS_NAME", "Name");
        public string DescriptionLabel => GetText("IDS_DESCRIPTION", "Description");
        public string CategoryLabel => GetText("IDS_CATEGORY", "Category");
        public string StatusLabel => GetText("IDS_STATUS", "Status");
        public string SaveButton => GetText("IDS_SAVE", "Save");
        public string CancelButton => GetText("IDS_CANCEL", "Cancel");
        public string ApplyButton => GetText("IDS_APPLY", "Apply");
        public string ReadyStatus => GetText("IDS_READY", "Ready");
    }
}
