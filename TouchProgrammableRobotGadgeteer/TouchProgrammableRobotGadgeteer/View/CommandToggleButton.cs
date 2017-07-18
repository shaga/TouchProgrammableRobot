using System;
using System.Threading;
using Microsoft.SPOT;
using Microsoft.SPOT.Input;
using Microsoft.SPOT.Presentation;
using Microsoft.SPOT.Presentation.Controls;
using Microsoft.SPOT.Presentation.Media;
using Microsoft.SPOT.Presentation.Shapes;

namespace TouchProgrammableRobotGadgeteer.View
{
    class CommandToggleButton : Border
    {
        private const int Size = Common.CommandButtonSize;

        private const int Thickness = Common.CommandButtonBorderThickness;

        private const int Offset = 2;

        private static readonly Bitmap ImageStop = new Bitmap(Resources.GetBytes(Resources.BinaryResources.arrow_s), Bitmap.BitmapImageType.Bmp);
        private static readonly Bitmap ImageFore = new Bitmap(Resources.GetBytes(Resources.BinaryResources.arrow_f), Bitmap.BitmapImageType.Bmp);
        private static readonly Bitmap ImageBack = new Bitmap(Resources.GetBytes(Resources.BinaryResources.arrow_b), Bitmap.BitmapImageType.Bmp);
        private static readonly Bitmap ImageLeft = new Bitmap(Resources.GetBytes(Resources.BinaryResources.arrow_l), Bitmap.BitmapImageType.Bmp);
        private static readonly Bitmap ImageRight = new Bitmap(Resources.GetBytes(Resources.BinaryResources.arrow_r), Bitmap.BitmapImageType.Bmp);
        private static readonly Bitmap ImageTurn = new Bitmap(Resources.GetBytes(Resources.BinaryResources.arrow_t), Bitmap.BitmapImageType.Bmp);

        private bool _isEnable = true;

        private bool _isReversing = false;

        public int Idx { get; }

        public ECommand Command { get; private set; }

        public event EventHandler ToggleStateChanged;

        private readonly Image _buttonImage;

        public bool IsEnable
        {
            get { return _isEnable; }
            set
            {
                _isEnable = value;
                if (_isEnable)
                {
                    IsReversing = false;
                    Activate();
                }
            }
        }

        public bool IsReversing
        {
            get { return _isReversing; }
            set
            {
                _isReversing = value; 
                SetImage();
            }
        }

        public CommandToggleButton(int idx) : base()
        {
            Idx = idx;
            Command = ECommand.Stop;

            Width = Size;
            Height = Size;

            BorderBrush = Common.BlackBrush;
            Background = Common.WhiteBrush;
            SetBorderThickness(Thickness);

            _buttonImage = new Image(ImageStop)
            {
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Center
            };
            Child = _buttonImage;

            TouchUp += OnTouchUp;

            //idx++;

            var top = (idx / Common.CommandColumnCount) * Common.CommandButtonDistanceV;
            var left = 0;
            if ((idx / Common.CommandColumnCount) % 2 == 0)
            {
                left = Common.CommandButtonStartEven +(idx % Common.CommandColumnCount) * Common.CommandButtonDistanceH;
            }
            else
            {
                left = Common.CommandButtonStartOdd - (idx % Common.CommandColumnCount) * Common.CommandButtonDistanceH;
            }

            Canvas.SetLeft(this, left + Offset);
            Canvas.SetTop(this, top + Offset);
        }

        public void SetVisibility(Visibility visibility)
        {
            Dispatcher.Invoke(Common.DispatcherTimeout, delegate(object p)
            {
                Visibility = visibility;
                return null;
            }, null);
        }

        public void Activate(bool isActive = false)
        {
            var d = new DispatcherOperationCallback(delegate(object p)
            {
                if (isActive && !IsEnable)
                {
                    BorderBrush = IsReversing ? Common.BlueBrush : Common.RedBrush;
                }
                else
                {
                    BorderBrush = Common.BlackBrush;
                }
                return null;
            });

            Dispatcher.Invoke(Common.DispatcherTimeout, d, null);
        }

        public void Reset()
        {
            Command = ECommand.Stop;
            _buttonImage.Bitmap = ImageStop;
            ToggleStateChanged?.Invoke(this, EventArgs.Empty);
        }

        private void SetImage()
        {
            Dispatcher.Invoke(Common.DispatcherTimeout, delegate (object p)
            {
                switch (Command)
                {
                    case ECommand.Fore:
                        _buttonImage.Bitmap = IsReversing ? ImageBack : ImageFore;
                        break;
                    case ECommand.Back:
                        _buttonImage.Bitmap = IsReversing ? ImageFore : ImageBack;
                        break;
                    case ECommand.TurnLeft:
                        _buttonImage.Bitmap = IsReversing ? ImageRight : ImageLeft;
                        break;
                    case ECommand.TurnRight:
                        _buttonImage.Bitmap = IsReversing ? ImageLeft : ImageRight;
                        break;
                    case ECommand.TurnBack:
                        _buttonImage.Bitmap = ImageTurn;
                        break;
                    default:
                        _buttonImage.Bitmap = ImageStop;
                        break;
                }

                return null;
            }, null);
        }

        private void OnTouchUp(object sender, TouchEventArgs e)
        {
            if (!IsEnable) return;

            if (Command < ECommand.TurnBack)
            {
                Command++;
            }
            else
            {
                Command = ECommand.Stop;
            }

            SetImage();

            ToggleStateChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}
