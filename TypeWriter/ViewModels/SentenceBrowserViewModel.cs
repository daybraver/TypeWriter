// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Navigation;
using DryIoc;
using Prism.Events;
using TypeWriter.PubSubEvents;
using TypeWriter.Services;

namespace TypeWriter.ViewModels
{
    internal class SentenceBrowserViewModel : BindableBase, IDialogAware, IDisposable
    {
        private readonly AppConfigSource _appConfigSource;
        private readonly IEventAggregator _eventAggregator;
        private readonly SentenceSource _sentenceSource;
        private Color _backColor;
        private bool _disposed;
        private Color _toTypeFontColor;
        private FontFamily _toTypeFontFamily;
        private double _toTypeFontSize;
        private FontStretch _toTypeFontStretch;
        private FontStyle _toTypeFontStyle;
        private FontWeight _toTypeFontWeight;
        private string _toTypeString;
        private int _typeBoxHeight;
        private int _typeBoxWidth;
        private Color _typedColor;
        private FontFamily _typedFontFamily;
        private double _typedFontSize;
        private FontStretch _typedFontStretch;
        private FontStyle _typedFontStyle;
        private FontWeight _typedFontWeight;
        private string _typedString;

        public SentenceBrowserViewModel(AppConfigSource appConfigSource, SentenceSource sentenceSource, IEventAggregator eventAggregator)
        {
            _appConfigSource = appConfigSource;
            _sentenceSource = sentenceSource;
            _eventAggregator = eventAggregator;
            _sentenceSource.CharTyped += _sentenceSource_CharTyped;
            _eventAggregator.GetEvent<SentensesSourceLoadedEvent>().Subscribe(ResetSentenseSource);
            _eventAggregator.GetEvent<AppConfigChangedEvent>().Subscribe(ChangeConfig);
            TypeBoxHeight = _appConfigSource.GetConfig().TypeBoxHeight;
            TypeBoxWidth = _appConfigSource.GetConfig().TypeBoxWidth;
            BackColor = _appConfigSource.GetConfig().BackColor;
            ToTypeFontColor = _appConfigSource.GetConfig().ToTypeFont.BrushColor.Color;
            ToTypeFontFamily = _appConfigSource.GetConfig().ToTypeFont.Family;
            ToTypeFontSize = _appConfigSource.GetConfig().ToTypeFont.Size;
            ToTypeFontStretch = _appConfigSource.GetConfig().ToTypeFont.Stretch;
            ToTypeFontStyle = _appConfigSource.GetConfig().ToTypeFont.Style;
            ToTypeFontWeight = _appConfigSource.GetConfig().ToTypeFont.Weight;
            TypedFontColor = _appConfigSource.GetConfig().TypedFont.BrushColor.Color;
            TypedFontFamily = _appConfigSource.GetConfig().TypedFont.Family;
            TypedFontSize = _appConfigSource.GetConfig().TypedFont.Size;
            TypedFontStretch = _appConfigSource.GetConfig().TypedFont.Stretch;
            TypedFontStyle = _appConfigSource.GetConfig().TypedFont.Style;
            TypedFontWeight = _appConfigSource.GetConfig().TypedFont.Weight;
        }

        private void ChangeConfig()
        {
            TypeBoxHeight = _appConfigSource.GetConfig().TypeBoxHeight;
            TypeBoxWidth = _appConfigSource.GetConfig().TypeBoxWidth;
            BackColor = _appConfigSource.GetConfig().BackColor;
            ToTypeFontColor = _appConfigSource.GetConfig().ToTypeFont.BrushColor.Color;
            ToTypeFontFamily = _appConfigSource.GetConfig().ToTypeFont.Family;
            ToTypeFontSize = _appConfigSource.GetConfig().ToTypeFont.Size;
            ToTypeFontStretch = _appConfigSource.GetConfig().ToTypeFont.Stretch;
            ToTypeFontStyle = _appConfigSource.GetConfig().ToTypeFont.Style;
            ToTypeFontWeight = _appConfigSource.GetConfig().ToTypeFont.Weight;
            TypedFontColor = _appConfigSource.GetConfig().TypedFont.BrushColor.Color;
            TypedFontFamily = _appConfigSource.GetConfig().TypedFont.Family;
            TypedFontSize = _appConfigSource.GetConfig().TypedFont.Size;
            TypedFontStretch = _appConfigSource.GetConfig().TypedFont.Stretch;
            TypedFontStyle = _appConfigSource.GetConfig().TypedFont.Style;
            TypedFontWeight = _appConfigSource.GetConfig().TypedFont.Weight;
        }

        public bool CanCloseDialog() => true;

        public void Dispose()
        {
            if (!_disposed)
            {
                _sentenceSource.CharTyped -= _sentenceSource_CharTyped;
                _eventAggregator.GetEvent<SentensesSourceLoadedEvent>().Unsubscribe(ResetSentenseSource);
                _eventAggregator.GetEvent<AppConfigChangedEvent>().Unsubscribe(ChangeConfig);
                _disposed = true;
            }
        }

        public void NextSentence()
        {
            var next = _sentenceSource.NextSentence();
            ToTypeString = next ?? string.Empty;
            TypedString = string.Empty;
        }

        public void OnDialogClosed()
        {
        }

        public void OnDialogOpened(IDialogParameters parameters)
        {
        }

        public void PrevSentence()
        {
            var prve = _sentenceSource.PrevSentence();
            ToTypeString = prve ?? string.Empty;
            TypedString = string.Empty;
        }

        public void TextInput(TextCompositionEventArgs e)
        {
            if ((_appConfigSource.GetConfig().AutoHide && (e.Text[0] == ' ' || e.Text[0] == '\r')) || e.Text[0] == '\x1B')
            {
                TextBlock textBlock = e.Source as TextBlock;
                var parent = LogicalTreeHelper.GetParent(textBlock);
                while (parent is not Window)
                {
                    parent = LogicalTreeHelper.GetParent(parent);
                }
                        (parent as Window)?.Hide();
                return;
            }

            _sentenceSource.OnInputChar(e.Text[0]);
        }

        private void _sentenceSource_CharTyped((string typedString, string toTypeString) obj)
        {
            TypedString = obj.typedString;
            ToTypeString = obj.toTypeString;
        }

        private void ResetSentenseSource()
        {
            TypedString = string.Empty;
            ToTypeString = string.Empty;
            _sentenceSource.Reset();
            TypedString = _sentenceSource.TypedString;
            ToTypeString = _sentenceSource.ToTypeString;
        }

        #region Properties

        public Color BackColor
        {
            get => _backColor;
            set => SetProperty(ref _backColor, value);
        }

        public DialogCloseListener RequestClose { get; }

        public Color ToTypeFontColor
        {
            get => _toTypeFontColor;
            set => SetProperty(ref _toTypeFontColor, value);
        }

        public FontFamily ToTypeFontFamily
        {
            get => _toTypeFontFamily;
            set => SetProperty(ref _toTypeFontFamily, value);
        }

        public double ToTypeFontSize
        {
            get => _toTypeFontSize;
            set => SetProperty(ref _toTypeFontSize, value);
        }

        public FontStretch ToTypeFontStretch
        {
            get => _toTypeFontStretch;
            set => SetProperty(ref _toTypeFontStretch, value);
        }

        public FontStyle ToTypeFontStyle
        {
            get => _toTypeFontStyle;
            set => SetProperty(ref _toTypeFontStyle, value);
        }

        public FontWeight ToTypeFontWeight
        {
            get => _toTypeFontWeight;
            set => SetProperty(ref _toTypeFontWeight, value);
        }

        public string ToTypeString
        {
            get => _toTypeString;
            set => SetProperty(ref _toTypeString, value);
        }

        public int TypeBoxHeight
        {
            get { return _typeBoxHeight; }
            set { SetProperty(ref _typeBoxHeight, value); }
        }

        public int TypeBoxWidth
        {
            get { return _typeBoxWidth; }
            set { SetProperty(ref _typeBoxWidth, value); }
        }

        public Color TypedFontColor
        {
            get => _typedColor;
            set => SetProperty(ref _typedColor, value);
        }

        public FontFamily TypedFontFamily
        {
            get => _typedFontFamily;
            set => SetProperty(ref _typedFontFamily, value);
        }

        public double TypedFontSize
        {
            get => _typedFontSize;
            set => SetProperty(ref _typedFontSize, value);
        }

        public FontStretch TypedFontStretch
        {
            get => _typedFontStretch;
            set => SetProperty(ref _typedFontStretch, value);
        }

        public FontStyle TypedFontStyle
        {
            get => _typedFontStyle;
            set => SetProperty(ref _typedFontStyle, value);
        }

        public FontWeight TypedFontWeight
        {
            get => _typedFontWeight;
            set => SetProperty(ref _typedFontWeight, value);
        }

        public string TypedString
        {
            get => _typedString;
            set => SetProperty(ref _typedString, value);
        }

        #endregion Properties
    }
}
