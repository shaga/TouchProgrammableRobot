using System;
using Microsoft.SPOT;
using Microsoft.SPOT.Presentation.Controls;
using Microsoft.SPOT.Presentation.Media;
using Color = Gadgeteer.Color;

namespace TouchProgrammableRobotGadgeteer.View
{
    abstract class MyButton : Border
    {
        protected static readonly SolidColorBrush LineBrush = new SolidColorBrush(Color.DarkGray);
        protected static readonly SolidColorBrush BackBrush = new SolidColorBrush(Color.FromRGB(204,204,204));

        protected MyButton()
        {
            Background = BackBrush;
            BorderBrush = LineBrush;
            SetBorderThickness(2);
        }
    }
}
