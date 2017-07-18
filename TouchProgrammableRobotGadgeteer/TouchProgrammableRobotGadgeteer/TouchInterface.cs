using System;
using Gadgeteer.Modules.GHIElectronics;
using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;
using Microsoft.SPOT.Input;
using Microsoft.SPOT.Presentation;
using Microsoft.SPOT.Presentation.Controls;
using Microsoft.SPOT.Presentation.Media;
using Microsoft.SPOT.Presentation.Shapes;
using TouchProgrammableRobotGadgeteer.View;

namespace TouchProgrammableRobotGadgeteer
{
    enum EMode
    {
        Normal,
        Repeat,
        Reverse,
        ReverseRepeat,
    }

    class TouchInterface
    {
        private const string LabelButtonRun = "GO";
        private const string LabelButtonStop = "STOP";
        private const string LabelButtonClear = "C";

        private static readonly Bitmap ImageRun = new Bitmap(Resources.GetBytes(Resources.BinaryResources.btn_run), Bitmap.BitmapImageType.Bmp);
        private static readonly Bitmap ImageStop = new Bitmap(Resources.GetBytes(Resources.BinaryResources.btn_stop), Bitmap.BitmapImageType.Bmp);
        private static readonly Bitmap ImageNormal = new Bitmap(Resources.GetBytes(Resources.BinaryResources.btn_normal), Bitmap.BitmapImageType.Bmp);
        private static readonly Bitmap ImageRepeat = new Bitmap(Resources.GetBytes(Resources.BinaryResources.btn_repeat), Bitmap.BitmapImageType.Bmp);
        private static readonly Bitmap ImageReverse = new Bitmap(Resources.GetBytes(Resources.BinaryResources.btn_reverse), Bitmap.BitmapImageType.Bmp);
        private static readonly Bitmap ImageReverseRepeat = new Bitmap(Resources.GetBytes(Resources.BinaryResources.btn_reverse_repeat), Bitmap.BitmapImageType.Bmp);

        private DisplayT35 _display;

        private readonly CommandToggleButton[] _commands = new CommandToggleButton[Common.CommandCount];

        private ImageButton _buttonRun;

        private TextButton _buttonReset;

        private ImageButton _buttonMode;

        private EMode _mode;

        private bool _isRunning = false;

        public event EventHandler TouchRun;

        private bool _isReversing = false;

        private Canvas _canvas;

        private IconTerminal _iconStart;

        private IconTerminal _iconEnd;

        private Border[] CommandHLine = new Border[4];

        private Border[] CommandVLine = new Border[3];

        private Border CommandHLineRunningEnd;


        public event EventHandler UpdateCommandState
        {
            add
            {
                foreach (var command in _commands)
                {
                    command.ToggleStateChanged += value;
                }
            }
            remove
            {
                foreach (var command in _commands)
                {
                    command.ToggleStateChanged -= value;
                }
            }
        }

        public bool IsEnableCommand
        {
            set
            {
                SetResetVisivility(value ? Visibility.Visible : Visibility.Hidden);
                //SetRunButtonText(value ? LabelButtonRun : LabelButtonStop);
                SetRunButtonImage(value ? ImageRun : ImageStop);

                bool isStop = false;

                foreach (var command in _commands)
                {
                    if (value) command.SetVisibility(Visibility.Visible);
                    else
                    {
                        if (isStop || command.Command == ECommand.Stop)
                        {
                            isStop = true;
                            command.SetVisibility(Visibility.Hidden);
                        }
                    }
                    command.IsEnable = value;
                }



                _isRunning = !value;
            }
        }

        public bool IsRunning
        {
            get { return _isRunning; }
            set
            {
                if (_isRunning == value) return;
                _isRunning = value;
                SetResetVisivility(!value ? Visibility.Visible : Visibility.Collapsed);
                SetRunButtonImage(!value ? ImageRun : ImageStop);

                _canvas.Dispatcher.Invoke(Common.DispatcherTimeout, delegate(object p)
                {
                    ChangeCommandVisivility();
                    return null;
                }, null);
            }
        }

        public bool IsReversing
        {
            get { return _isReversing; }
            set
            {
                if (_isReversing == value) return;
                _isReversing = value;
                foreach (var command in _commands)
                {
                    if (command.Command == ECommand.Stop) break;
                    command.IsReversing = value;
                }

                _canvas.Dispatcher.Invoke(Common.DispatcherTimeout, delegate(object p)
                {
                    if (_isReversing)
                    {
                        _iconEnd.SetImage(ETerminalState.Start);

                        _iconStart.SetImage(Mode == EMode.Reverse ? ETerminalState.End : ETerminalState.Reverse);
                    }
                    else
                    {
                        _iconStart.SetImage(ETerminalState.Start);

                        switch (Mode)
                        {
                            case EMode.Normal:
                                _iconEnd.SetImage(ETerminalState.End);
                                break;
                            case EMode.Repeat:
                                _iconEnd.SetImage(ETerminalState.Repeat);
                                break;
                            case EMode.Reverse:
                            case EMode.ReverseRepeat:
                                _iconEnd.SetImage(ETerminalState.Reverse);
                                break;
                        }
                    }
                    return null;
                }, null);
            }
        }

        public EMode Mode
        {
            get { return _mode; }
            private set
            {
                _mode = value;
                SetModeButtonImage();
            }
        }

        public TouchInterface(DisplayT35 display)
        {
            _display = display;
        }

        public void Initialize()
        {
            _canvas = new Canvas();
            _canvas.Width = 320;
            _canvas.Height = 240;

            _display.WPFWindow.Child = _canvas;

            CommandHLineRunningEnd = new Border() {Background = Common.BlackBrush, Height = Common.CommandHLineHeight};
            _canvas.Children.Add(CommandHLineRunningEnd);

            InitHorizontalLine(_canvas);
            InitVerticalLine(_canvas);
            InitCommandStartEnd(_canvas);
            InitCommandButtons(_canvas);
            InitRunButton(_canvas);
            InitResetButton(_canvas);
            InitModeButton(_canvas);
        }

        public void ActivateCommandButton(int idx)
        {
            if (idx < 0 || Common.CommandCount <= idx) return;

            _commands[idx].Activate(true);
        }

        public void DeactiveCommandButton(int idx)
        {
            if (idx < 0 || Common.CommandCount <= idx) return;

            _commands[idx].Activate(false);
        }

        private void InitRunButton(Canvas canvas)
        {
            _buttonRun = new ImageButton(ImageRun)
            {
                Width = 110,
                Height = 50,
            };

            _buttonRun.TouchUp += OnTouchedRun;

            Canvas.SetRight(_buttonRun, 5);
            Canvas.SetBottom(_buttonRun, 5);

            canvas.Children.Add(_buttonRun);
        }

        private void InitModeButton(Canvas canvas)
        {
            _buttonMode = new ImageButton(ImageNormal)
            {
                Width = 110,
                Height = 50,
            };

            _buttonMode.TouchUp += (s, e) =>
            {
                if (_isRunning) return;

                Mode = (EMode) (((int) Mode + 1) % 4);
            };

            Canvas.SetLeft(_buttonMode, 75);
            Canvas.SetBottom(_buttonMode, 5);
            canvas.Children.Add(_buttonMode);
        }

        private void InitResetButton(Canvas canvas)
        {
            _buttonReset = new TextButton(LabelButtonClear)
            {
                Width = 65,
                Height = 50
            };

            _buttonReset.TouchUp += (s, e) =>
            {
                foreach (var command in _commands)
                {
                    command.Reset();
                }
            };

            Canvas.SetLeft(_buttonReset, 5);
            Canvas.SetBottom(_buttonReset, 5);
            canvas.Children.Add(_buttonReset);
        }

        private void InitCommandButtons(Canvas canvas)
        {
            for (var i = 0; i < _commands.Length; i++)
            {
                _commands[i] = new CommandToggleButton(i);
                canvas.Children.Add(_commands[i]);
            }
        }

        private void InitHorizontalLine(Canvas canvas)
        {
            var top = Common.CommandHLineOffsetV;

            for (var i = 0; i < Common.CommandRowCount; i++)
            {
                var border = new Border()
                {
                    Width = Common.CommandHLineWidth,
                    Height = Common.CommandHLineHeight,
                    Background = Common.BlackBrush,
                };

                Canvas.SetLeft(border, Common.CommandHLineOffsetH);
                Canvas.SetTop(border, top);
                canvas.Children.Add(border);

                top += Common.CommandButtonDistanceV;

                CommandHLine[i] = border;
            }
        }

        private void InitVerticalLine(Canvas canvas)
        {
            var top = Common.CommandVLineOffsetV;

            for (var i = 0; i < Common.CommandRowCount - 1; i++)
            {
                var border = new Border()
                {
                    Width = Common.CommandVLineWidth,
                    Height = Common.CommandVLineHeight,
                    Background = Common.BlackBrush,
                };

                var left = (i % 2) == 0 ? Common.CommandVLineOffsetHEven : Common.CommandVLineOffsetHOdd;
                Canvas.SetLeft(border, left);
                Canvas.SetTop(border, top);

                canvas.Children.Add(border);

                top += Common.CommandButtonDistanceV;

                CommandVLine[i] = border;
            }
        }

        private void InitCommandStartEnd(Canvas canvas)
        {
            _iconStart = new IconTerminal(ETerminalState.Start);
            Canvas.SetLeft(_iconStart, Common.CommandStartOffsetLeft);
            Canvas.SetTop(_iconStart, Common.CommandStartOffsetTop);
            _canvas.Children.Add(_iconStart);

            _iconEnd = new IconTerminal(ETerminalState.End);
            Canvas.SetLeft(_iconEnd, Common.CommandEndOffsetLeft);
            Canvas.SetTop(_iconEnd, Common.CommandEndOffsetTop);
            _canvas.Children.Add(_iconEnd);
        }

        private void SetResetVisivility(Visibility visibility)
        {
            _buttonReset.Dispatcher.Invoke(Common.DispatcherTimeout, delegate (object p)
            {
                _buttonReset.Visibility = visibility;
                return null;
            }, null);
        }

        private void SetRunButtonImage(Bitmap bmp)
        {
            _buttonRun.Dispatcher.Invoke(Common.DispatcherTimeout, delegate(object p)
            {
                _buttonRun.Image = bmp;
                return null;
            }, null);
        }

        private void ChangeCommandVisivility()
        {
            if (!IsRunning)
            {
                foreach (var command in _commands)
                {
                    command.IsEnable = true;
                    command.Visibility = Visibility.Visible;
                }

                _iconStart.SetImage(ETerminalState.Start);
                _iconEnd.SetImage(ETerminalState.End);

                return;
            }

            var isStop = false;

            foreach (var command in _commands)
            {
                command.IsEnable = false;

                if (isStop || command.Command == ECommand.Stop)
                {
                    isStop = true;
                    command.Visibility = Visibility.Collapsed;
                }
            }

            switch (Mode)
            {
                case EMode.Normal:
                    _iconEnd.SetImage(ETerminalState.End);
                    break;
                case EMode.Repeat:
                    _iconEnd.SetImage(ETerminalState.Repeat);
                    break;
                case EMode.Reverse:
                case EMode.ReverseRepeat:
                    _iconEnd.SetImage(ETerminalState.Reverse);
                    break;
            }
        }

        private void SetModeButtonImage()
        {
            Bitmap img;
            switch (_mode)
            {
                case EMode.Repeat:
                    img = ImageRepeat;
                    break;
                case EMode.Reverse:
                    img = ImageReverse;
                    break;
                case EMode.ReverseRepeat:
                    img = ImageReverseRepeat;
                    break;
                default:
                    img = ImageNormal;
                    break;
            }

            _buttonMode.Dispatcher.Invoke(Common.DispatcherTimeout, delegate(object p)
            {
                _buttonMode.Image = img;
                return null;
            }, null);
        }

        private void OnTouch(object sender, TouchEventArgs e)
        {
            Debug.Print("touched");
        }

        private void OnTouchedRun(object sender, TouchEventArgs e)
        {
            TouchRun?.Invoke(this, EventArgs.Empty);
        }
    }
}

