using System;
using System.Threading;
using Microsoft.SPOT;
using Microsoft.SPOT.Presentation;
using Microsoft.SPOT.Presentation.Controls;

namespace TouchProgrammableRobotGadgeteer.View
{
    public enum ETerminalState
    {
        Start,
        End,
        Repeat,
        Reverse,
    }

    class IconTerminal : Border
    {
        private static readonly Bitmap ImageStart = new Bitmap(Resources.GetBytes(Resources.BinaryResources.icon_start), Bitmap.BitmapImageType.Bmp);
        private static readonly Bitmap ImageEnd = new Bitmap(Resources.GetBytes(Resources.BinaryResources.icon_fin), Bitmap.BitmapImageType.Bmp);
        private static readonly Bitmap ImageRepeat = new Bitmap(Resources.GetBytes(Resources.BinaryResources.icon_repeat), Bitmap.BitmapImageType.Bmp);
        private static readonly Bitmap ImageReverse = new Bitmap(Resources.GetBytes(Resources.BinaryResources.icon_reverse), Bitmap.BitmapImageType.Bmp);

        private Image _image;

        public IconTerminal(ETerminalState state)
        {
            Width = Common.CommandButtonSize;
            Height = Common.CommandButtonSize;

            SetBorderThickness(0);

            _image = new Image();
            _image.HorizontalAlignment = HorizontalAlignment.Center;
            _image.VerticalAlignment = VerticalAlignment.Center;

            Child = _image;
            
            SetImage(state);
        }

        public void SetImage(ETerminalState state)
        {
            switch (state)
            {
                case ETerminalState.End:
                    _image.Bitmap = ImageEnd;
                    break;
                case ETerminalState.Repeat:
                    _image.Bitmap = ImageRepeat;
                    break;
                case ETerminalState.Reverse:
                    _image.Bitmap = ImageReverse;
                    break;
                default:
                    _image.Bitmap = ImageStart;
                    break;
            }

        }
    }
}
