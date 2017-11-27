﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using LiveCharts;
using LiveCharts.Configurations;
using LiveCharts.Uwp;
using LiveCharts.Defaults;
using System.Threading.Tasks;
using Windows.UI.Core;
using Processing;

namespace RTGui
{
    /// <summary>
    /// Chart page showing variations of Marginal Hilbert Spectrum for one channel in real time 
    /// </summary>
    public sealed partial class ChartPage : Page
    {
        private ChartPageViewModel _vm;

        public ChartPage()
        {
            this.InitializeComponent();
            _vm = null;
        }
        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);
            if (_vm != null) {
                DataManager.ChartPagesStates.AddOrUpdate(_vm.Channel, _vm, (channel, prevVm) => _vm);
                DataManager.Current.SampleAnalysed -= OnSampleAnalysed;
                _vm = null;
            }
        }
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            int? channel = e.Parameter as int?;
            if (channel != null) {
                DataManager.ChartPagesStates.TryRemove((int)channel, out _vm);
                if (_vm == null)
                    _vm = new ChartPageViewModel((int)channel);

                DataContext = _vm;
                DataManager.Current.SampleAnalysed += OnSampleAnalysed;
            }
        }
        private void OnSampleAnalysed(IHilbertSpectrumDouble spectrum, int channel)
        {
            if (_vm != null && channel == _vm.Channel) {
                double maxFreq = spectrum.MaxFrequency;
                double minFreq = spectrum.MinFrequency;
                double step = (maxFreq - minFreq) / 1000;

                double avgSpectrum = 0.0;
                double maxSpectrum = spectrum.ComputeMarginalAt(minFreq);
                int i = 0;
                for (double w = minFreq; w <= maxFreq; w += step) {
                    double value = spectrum.ComputeMarginalAt(w);
                    avgSpectrum += value;
                    if (value > maxSpectrum)
                        maxSpectrum = value;
                    ++i;
                }
                avgSpectrum /= i;

                _vm.UpdateContent(maxFreq, minFreq, maxSpectrum, avgSpectrum, Dispatcher);
            }
        }
    }

    public class ChartPageViewModel : ViewModelBase
    {
        public const int X_LENGTH = 20;
        
        private readonly int _channel;

        public ChartPageViewModel(int channel)
        {
            _channel = channel;
            MaxFreqValues = new ChartValues<ObservableValue>();
            MinFreqValues = new ChartValues<ObservableValue>();
            MaxSpectrumValues = new ChartValues<ObservableValue>();
            AvgSpectrumValues = new ChartValues<ObservableValue>();
            Title = $"Channel {channel}";
        }
        public async void UpdateContent(double maxFreq, double minFreq, double maxSpectrum, double avgSpectrum, CoreDispatcher uiDisp)
        {
            await uiDisp.RunAsync(CoreDispatcherPriority.Normal, () => {
                MaxFreqValues.Add(new ObservableValue(maxFreq));
                if (MaxFreqValues.Count > X_LENGTH)
                    MaxFreqValues.RemoveAt(0);

                MinFreqValues.Add(new ObservableValue(minFreq));
                if (MinFreqValues.Count > X_LENGTH)
                    MinFreqValues.RemoveAt(0);

                MaxSpectrumValues.Add(new ObservableValue(maxSpectrum));
                if (MaxSpectrumValues.Count > X_LENGTH)
                    MaxSpectrumValues.RemoveAt(0);

                AvgSpectrumValues.Add(new ObservableValue(avgSpectrum));
                if (AvgSpectrumValues.Count > X_LENGTH)
                    AvgSpectrumValues.RemoveAt(0);

                MaxFreqText = $"Max inst frequency = {maxFreq}";
                MinFreqText = $"Min inst frequency = {minFreq}";
                MaxSpectrumText = $"Max Hilbert spectrum = {maxSpectrum}";
                AvgSpectrumText = $"Avg Hilbert spectrum = {avgSpectrum}";
            });
        }

        public int Channel
        { get => _channel; }

        public ChartValues<ObservableValue> MaxFreqValues
        { get; }
        public ChartValues<ObservableValue> MinFreqValues
        { get; }
        public ChartValues<ObservableValue> MaxSpectrumValues
        { get; }
        public ChartValues<ObservableValue> AvgSpectrumValues
        { get; }

        private string _title;
        public string Title
        {
            get => _title;
            set {
                _title = value;
                OnPropertyChanged(nameof(Title));
            }
        }
        private string _maxFreqText;
        public string MaxFreqText
        {
            get => _maxFreqText;
            set {
                _maxFreqText = value;
                OnPropertyChanged(nameof(MaxFreqText));
            }
        }
        private string _minFreqText;
        public string MinFreqText
        {
            get => _minFreqText;
            set {
                _minFreqText = value;
                OnPropertyChanged(nameof(MinFreqText));
            }
        }
        private string _avgSpectrumText;
        public string AvgSpectrumText
        {
            get => _avgSpectrumText;
            set {
                _avgSpectrumText = value;
                OnPropertyChanged(nameof(AvgSpectrumText));
            }
        }
        private string _maxSpectrumText;
        public string MaxSpectrumText
        {
            get => _maxSpectrumText;
            set {
                _maxSpectrumText = value;
                OnPropertyChanged(nameof(MaxSpectrumText));
            }
        }
    }
}
