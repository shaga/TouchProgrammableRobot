using System;
using Microsoft.SPOT;
using Microsoft.SPOT.Presentation;
using Microsoft.SPOT.Presentation.Controls;
using Microsoft.SPOT.Presentation.Media;
using Color = Gadgeteer.Color;

namespace TouchProgrammableRobotGadgeteer.View
{
    class ImageButton : MyButton
    {
        private Image _image;

        public Bitmap Image
        {
            get { return _image?.Bitmap; }
            set
            {
                if(_image == null) return;

                _image.Bitmap = value;
            }
        }

        public ImageButton() : base()
        {
            _image = new Image();
            _image.HorizontalAlignment = HorizontalAlignment.Center;
            _image.VerticalAlignment = VerticalAlignment.Center;

            Child = _image;
        }

        public ImageButton(Bitmap bmp) : this()
        {
            _image.Bitmap = bmp;
        }
    }
}
