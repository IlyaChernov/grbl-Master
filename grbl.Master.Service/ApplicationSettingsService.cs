namespace grbl.Master.Service
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Text;

    using grbl.Master.Common.Interfaces.Service;
    using grbl.Master.Model;
    using grbl.Master.Utilities;

    public class ApplicationSettingsService : IApplicationSettingsService
    {
        public ApplicationSettings Settings { get; internal set; } = new ApplicationSettings();

        public void Upgrade()
        {
            Model.Properties.Settings.Default.Upgrade();
            Model.Properties.Settings.Default.Reload();
        }

        public void Save()
        {
            var index = 0;
            foreach (var macros in Settings.Macroses)
            {
                macros.Index = index++;
            }

            Model.Properties.Settings.Default.MacrosesXML = Settings.Macroses.SerializeToXML();

            Model.Properties.Settings.Default.JoggingDistances = ListDoubleToString(Settings.JoggingDistances);

            Model.Properties.Settings.Default.JoggingSpeeds = ListDoubleToString(Settings.FeedRates);

            Model.Properties.Settings.Default.SelectedBaudRate = Settings.SelectedBaudRate;

            Model.Properties.Settings.Default.SelectedComPort = Settings.SelectedComPort;

            Model.Properties.Settings.Default.JoggingDistance = Settings.JoggingDistance;

            Model.Properties.Settings.Default.JoggingSpeed = Settings.FeedRate;

            Model.Properties.Settings.Default.Save();
            Model.Properties.Settings.Default.Reload();
        }

        private string ListDoubleToString(IEnumerable<double> value)
        {
            if (value is { } elements)
            {
                var sb = new StringBuilder();
                foreach (var element in elements)
                {
                    sb.AppendLine(element.ToString());
                }

                return sb.ToString();
            }

            return string.Empty;
        }

        private IEnumerable<double> ListDoubleFromString(string value)
        {
            return value.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries).Select(double.Parse);
        }

        public void Load()
        {
            Model.Properties.Settings.Default.Reload();
            if (!string.IsNullOrEmpty(Model.Properties.Settings.Default.MacrosesXML))
            {
                Settings.Macroses = Model.Properties.Settings.Default.MacrosesXML.XmlDeserialize<ObservableCollection<Macros>>();
            }

            if (Model.Properties.Settings.Default.JoggingDistances.Length > 0)
            {
                try
                {
                    Settings.JoggingDistances.Clear();
                    foreach (var variable in ListDoubleFromString(Model.Properties.Settings.Default.JoggingDistances))
                    {
                        Settings.JoggingDistances.Add(variable);
                    }
                }
                catch
                {
                    Save();
                }
            }

            if (Model.Properties.Settings.Default.JoggingSpeeds.Length > 0)
            {
                try
                {
                    Settings.FeedRates.Clear();
                    foreach (var variable in ListDoubleFromString(Model.Properties.Settings.Default.JoggingSpeeds))
                    {
                        Settings.FeedRates.Add(variable);
                    }
                }
                catch
                {
                    Save();
                }
            }

            Settings.SelectedBaudRate = Model.Properties.Settings.Default.SelectedBaudRate;

            Settings.SelectedComPort = Model.Properties.Settings.Default.SelectedComPort;

            Settings.JoggingDistance = Model.Properties.Settings.Default.JoggingDistance;

            Settings.FeedRate = Model.Properties.Settings.Default.JoggingSpeed;
        }
    }
}
