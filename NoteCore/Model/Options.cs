using System.Windows.Media;
using FirstFloor.ModernUI.Presentation;
using GalaSoft.MvvmLight;
using NoteCore.Twitter.Model;
using ProtoBuf;

namespace NoteCore.Model
{
    [ProtoContract(SkipConstructor = true)]
    public class Options
        : ObservableObject
    {
        private bool _isTopMost;
        private bool _isShowInTaskBar;
        private double _height;
        private double _left;
        private double _top;
        private double _width;
        private Color _selectedAccentColor;
        private Link _selectedTheme;
        private OAuthTokens _tokens;

        public Options()
        {
            _tokens = new OAuthTokens();
            _isTopMost = false;
            _isShowInTaskBar = true;
            _height = 500;
            _width = 320;
        }

        [ProtoIgnore]
        public Link SelectedTheme
        {
            get { return _selectedTheme; }
            set
            {
                if (_selectedTheme != value)
                {
                    _selectedTheme = value;
                    RaisePropertyChanged(() => SelectedTheme);
                    //update the actual theme
                    AppearanceManager.Current.ThemeSource = value.Source;
                }
            }
        }

        [ProtoIgnore]
        public bool IsStartUp
        {
            get { return Utils.AutoStart.IsAutoStartEnabled; }
            set
            {
                Utils.AutoStart.SetAutoStart(value);
                RaisePropertyChanged(() => IsStartUp);
            }
        }

        [ProtoMember(1)]
        public bool IsShowInTaskbar
        {
            get { return _isShowInTaskBar; }
            set
            {
                _isShowInTaskBar = value;
                RaisePropertyChanged(()=> IsShowInTaskbar);
            }
        }

        [ProtoMember(2)]
        public bool IsTopMost
        {
            get { return _isTopMost; }
            set
            {
                _isTopMost = value;
                RaisePropertyChanged(()=> IsTopMost);
            }
        }

        [ProtoMember(3)]
        public double Left
        {
            get { return _left; }
            set
            {
                _left = value;
                RaisePropertyChanged(() => Left);
            }
        }

        [ProtoMember(4)]
        public double Height
        {
            get { return _height; }
            set
            {
                _height = value;
                RaisePropertyChanged(() => Height);
            }
        }

        [ProtoMember(5)]
        public double Top
        {
            get { return _top; }
            set
            {
                _top = value;
                RaisePropertyChanged(() => Top);
            }
        }

        [ProtoMember(6)]
        public double Width
        {
            get { return _width; }
            set
            {
                _width = value;
                RaisePropertyChanged(() => Width);
            }
        }

        [ProtoMember(7)]
        public Color SelectedAccentColor
        {
            get  {return _selectedAccentColor; }
            set
            {
                if (_selectedAccentColor != value)
                {
                    _selectedAccentColor = value;
                    RaisePropertyChanged(() => SelectedAccentColor);
                    AppearanceManager.Current.AccentColor = value;
                }
            }
        }

        [ProtoMember(8)]
        public Enumerations.ThemeSelections Theme { get; set; }

        [ProtoMember(9)]
        public OAuthTokens Tokens
        {
            get { return _tokens; }
            set
            {
                if (_tokens == value) return;
                _tokens = value;
                RaisePropertyChanged(()=> Tokens);
            }
        }

    }
}
