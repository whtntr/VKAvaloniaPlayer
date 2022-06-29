﻿using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Input;
using Avalonia.Layout;
using ReactiveUI;
using VKAvaloniaPlayer.ETC;
using VKAvaloniaPlayer.Models;
using VKAvaloniaPlayer.Models.Interfaces;
using VKAvaloniaPlayer.ViewModels.Exceptions;

namespace VKAvaloniaPlayer.ViewModels.Base
{
    public abstract class VkDataViewModelBase : ViewModelBase
    {
        public ObservableCollection<IVkModelBase>? _AllDataCollection;
        private ObservableCollection<IVkModelBase>? _DataCollection;
        private bool _IsError;
        private bool _Loading = true;

        private IDisposable? _SearchDisposable;

        private bool _SearchIsVisible = true;
        private string _SearchText = string.Empty;
        private int _SelectedIndex = -1;
       
        public VkDataViewModelBase()
        {
            AudioListButtons = new AudioListButtons();
            LoadMusicsAction = () =>
            {
                if (string.IsNullOrEmpty(_SearchText))
                    if (ResponseCount > 0 && IsLoading is false)
                        InvokeHandler.Start(new InvokeHandlerObject(LoadData, this));
            };
            
         
            _AllDataCollection = new ObservableCollection<IVkModelBase>();
            DataCollection = _AllDataCollection;
        }

        public AudioListButtons AudioListButtons { get; set; }
       
        private IDisposable ScrolledDisposible;
        private ScrollChangedEventArgs ScrolledEventArgs { get; set; }

        public bool IsError
        {
            get => _IsError;
            set => this.RaiseAndSetIfChanged(ref _IsError, value);
        }

        public bool SearchIsVisible
        {
            get => _SearchIsVisible;
            set => this.RaiseAndSetIfChanged(ref _SearchIsVisible, value);
        }

        public ExceptionViewModel ExceptionModel { get; set; }

        public static Action? LoadMusicsAction { get; set; }

        public int ResponseCount { get; set; }

        public ObservableCollection<IVkModelBase>? DataCollection
        {
            get => _DataCollection;
            set => this.RaiseAndSetIfChanged(ref _DataCollection, value);
        }

        public bool IsLoading
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
            set => this.RaiseAndSetIfChanged(ref _SelectedIndex, value);
        }

        public virtual void StartLoad()
        {
            InvokeHandler.Start(new InvokeHandlerObject(LoadData, this));
        }
        
        public void StartScrollChangedObservable(Action? action, Orientation orientation)
        {
           
            ScrolledDisposible =  this.WhenAnyValue(vm=>vm.ScrolledEventArgs)
                .Subscribe((e) =>
                {
                    try
                    {
                        double max = 0;
                        double current = 0;

                        if (e?.Source is ScrollViewer scrollViewer)
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

                            if (max == current) action?.Invoke();
                        }
                    }
                    catch(Exception ex) { }
                });
        
        }

        public void StopScrollChandegObserVable()
        {
            ScrolledDisposible.Dispose();
            ScrolledDisposible=null;
        }

        public virtual async void LoadData()
        {
        }

        public virtual void Search(string? text)
        {
            try
            {
                if (string.IsNullOrEmpty(text))
                {
                    SelectedIndex = -1;
                    DataCollection = _AllDataCollection;
                    StartScrollChangedObservable(LoadMusicsAction, Orientation.Vertical);
                }
                else if (_AllDataCollection != null && _AllDataCollection.Count() > 0)
                {
                    StopScrollChandegObserVable();

                    var searchRes = _AllDataCollection.Where(x =>
                            x.Title.ToLower().Contains(text.ToLower()) ||
                            x.Artist.ToLower().Contains(text.ToLower()))
                        .Distinct();
                    DataCollection = new ObservableCollection<IVkModelBase>(searchRes);
                }

                Task.Run(() => { DataCollection.StartLoadImages(); });
            }
            catch (Exception ex)
            {
                DataCollection = _AllDataCollection;
                SearchText = "";
            }
        }

        public virtual void SelectedItem()
        {
            Console.WriteLine("Item selected");
            if (SelectedIndex > -1)
                PlayerControlViewModel.SetPlaylist(
                    new ObservableCollection<AudioModel>(DataCollection.Cast<AudioModel>().ToList()),
                    SelectedIndex);
        }

        public virtual void StartSearchObservable()
        {
            if (_SearchDisposable is null)
                _SearchDisposable = this.WhenAnyValue(x => x.SearchText).Subscribe(text => Search(text?.ToLower()));
        }

        public virtual void StartSearchObservable(TimeSpan timeSpan)
        {
            if (_SearchDisposable is null)
                _SearchDisposable = this.WhenAnyValue(x => x.SearchText)
                    .Throttle(timeSpan)
                    .Subscribe(text => Search(text.ToLower()));
        }

        public virtual void StopSearchObservable()
        {
            _SearchDisposable?.Dispose();
            _SearchDisposable = null;
        }


        public virtual  void SelectedItem(object sender,PointerPressedEventArgs args)
        {
                Console.WriteLine("pressed");
                var contentpress = args?.Source as ContentPresenter;
                Console.WriteLine("null:"+ (contentpress==null?"true":"false"));
                
                var model = contentpress?.Content as IVkModelBase;
                
                if (model != null)
                {
                    Console.WriteLine("Model not null");
                    SelectedIndex = DataCollection.IndexOf(model);
                    SelectedItem();
                }
                else Console.WriteLine("model is null");
        }
        public virtual void Scrolled(object sender, ScrollChangedEventArgs args)=>
            ScrolledEventArgs = args;
            
        
    }
}