﻿using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive.Linq;
using Avalonia.Controls;
using Avalonia.Layout;
using ReactiveUI;
using VKAvaloniaPlayer.Models.Interfaces;

namespace VKAvaloniaPlayer.ViewModels.Base
{
    public abstract class DataViewModelBase : ViewModelBase
    {
        private readonly ObservableCollection<IVkModelBase>? _AllDataCollection;
        private ObservableCollection<IVkModelBase>? _DataCollection;

        private bool _Loading = true;

        private IDisposable? _SearchDisposable = null;
        private string _SearchText = string.Empty;
        private int _SelectedIndex = -1;
        public IReactiveCommand? ScrollingListEventCommand { get; set; }

        public static Action? LoadMusicsAction { get; set; }

        public int ResponseCount { get; set; }

        public DataViewModelBase()
        {
            LoadMusicsAction = new Action(() =>
            {
                if (ResponseCount > 0 && Loading is false)
                    LoadData();
                else
                    return;
            });

            _AllDataCollection = new ObservableCollection<IVkModelBase>();
            DataCollection = _AllDataCollection;
        }

        public ObservableCollection<IVkModelBase>? DataCollection
        {
            get => _DataCollection;
            set => this.RaiseAndSetIfChanged(ref _DataCollection, value);
        }

        public bool Loading
        {
            get => _Loading;
            set => this.RaiseAndSetIfChanged(ref _Loading, value);
        }

        public int Offset { get; set; } = 0;

        public string SearchText
        {
            get => _SearchText;
            set => this.RaiseAndSetIfChanged(ref _SearchText, value);
        }

        public int SelectedIndex
        {
            get => _SelectedIndex;
            set
            {
                this.RaiseAndSetIfChanged(ref _SelectedIndex, value);
                SelectedItem();
            }
        }

        private double _LastHeight = 0;

        public void StartScrollChangedObservable(Action? action, Orientation orientation)
        {
            if (ScrollingListEventCommand is null)
                ScrollingListEventCommand = ReactiveCommand.Create((ScrollChangedEventArgs e) =>
                {
                    double max = 0;
                    double current = 0;

                    if (e.Source is ScrollViewer scrollViewer)
                    {
                        if (orientation == Orientation.Vertical)
                        {
                            max = scrollViewer.GetValue(ScrollViewer.VerticalScrollBarMaximumProperty);
                            current = scrollViewer.GetValue(ScrollViewer.VerticalScrollBarValueProperty);
                        }
                        else
                        {
                            max = scrollViewer.GetValue(ScrollViewer.HorizontalScrollBarMaximumProperty);
                            current = scrollViewer.GetValue(ScrollViewer.HorizontalScrollBarValueProperty);
                        }

                        // ReSharper disable once CompareOfFloatsByEqualityOperator
                        if (max == current) action?.Invoke();
                    }
                });
        }

        public void StopScrollChandegObserVable()
        {
            ScrollingListEventCommand.Dispose();
            ScrollingListEventCommand = null;
        }

        public virtual void LoadData()
        {
        }

        public virtual void Search(string text)
        {
            if (string.IsNullOrEmpty(SearchText))
            {
                SelectedIndex = -1;
                DataCollection = _AllDataCollection;
            }
            else if (_AllDataCollection != null && _AllDataCollection.Count() > 0)
            {
                var searchRes = _AllDataCollection.Where(x =>
                        x.Title.ToLower().Contains(SearchText.ToLower()) ||
                        x.Artist.ToLower().Contains(SearchText.ToLower()))
                    .Distinct();
                DataCollection = new ObservableCollection<IVkModelBase>(searchRes);
            }
        }

        public virtual void SelectedItem()
        {
            if (SelectedIndex > -1)
                PlayerControlViewModel.SetPlaylist(DataCollection.Cast<Models.AudioModel>().ToList(),
                    (Models.AudioModel) DataCollection[SelectedIndex]);
        }

        public virtual void StartSearchObservable()
        {
            if (_SearchDisposable is null)
                _SearchDisposable = this.WhenAnyValue(x => x.SearchText).Subscribe((text) => Search(text.ToLower()));
        }

        public virtual void StartSearchObservable(TimeSpan timeSpan)
        {
            if (_SearchDisposable is null)
                _SearchDisposable = this.WhenAnyValue(x => x.SearchText)
                    .Throttle(timeSpan)
                    .Subscribe((text) => Search(text.ToLower()));
        }

        public virtual void StopSearchObservable()
        {
            _SearchDisposable?.Dispose();
            _SearchDisposable = null;
        }
    }
}