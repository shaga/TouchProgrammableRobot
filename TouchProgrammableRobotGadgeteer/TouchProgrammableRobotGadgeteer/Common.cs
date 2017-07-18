using System;
using Microsoft.SPOT;
using Microsoft.SPOT.Presentation;
using Microsoft.SPOT.Presentation.Controls;
using Microsoft.SPOT.Presentation.Media;
using TouchProgrammableRobotGadgeteer.View;

namespace TouchProgrammableRobotGadgeteer
{
    public enum ECommand
    {
        Stop,
        Fore,
        Back,
        TurnLeft,
        TurnRight,
        TurnBack,
    }

    static class Common
    {
        public const int CommandColumnCount = 5;

        public const int CommandRowCount = 4;

        public const int CommandCount = 20;

        public const int CommandButtonSize = 42;

        public const int CommandButtonBorderThickness = 3;

        public const int CommandButtonDistanceH = 48;

        public const int CommandButtonDistanceV = 44;

        public const int CommandButtonStartEven = 50;

        public const int CommandButtonStartOdd = 242;

        public const int CommandHLineWidth = 270;

        public const int CommandHLineHeight = 10;

        public const int CommandHLineOffsetH = 34;

        public const int CommandHLineOffsetV = 19;

        public const int CommandVLineWidth = 10;

        public const int CommandVLineHeight = 54;

        public const int CommandVLineOffsetHEven = 294;

        public const int CommandVLineOffsetHOdd = 34;

        public const int CommandVLineOffsetV = 19;

        public const int CommandStartOffsetLeft = 2;

        public const int CommandStartOffsetTop = 3;

        public const int CommandEndOffsetLeft = 3;

        public const int CommandEndOffsetTop = CommandButtonDistanceV * (CommandRowCount - 1) + 3;

        public static readonly SolidColorBrush BlackBrush = new SolidColorBrush(Colors.Black);

        public static readonly SolidColorBrush WhiteBrush = new SolidColorBrush(Colors.White);

        public static readonly SolidColorBrush RedBrush = new SolidColorBrush(Colors.Red);

        public static readonly SolidColorBrush BlueBrush = new SolidColorBrush(Colors.Blue);

        public static readonly TimeSpan DispatcherTimeout = new TimeSpan(0, 0, 1);

        public static void SetLastBorder(ref Border border, int idx)
        {
            if (idx == CommandCount) return;

            var col = idx % CommandColumnCount;
            var row = idx / CommandColumnCount;

            Canvas.SetTop(border, CommandHLineOffsetV + CommandButtonDistanceV * row);
            border.Width = CommandButtonDistanceH * (col + 1) - 20;
            border.Visibility = Visibility.Visible;
            if (row % 2 == 0)
            {
                Canvas.SetLeft(border, CommandHLineOffsetH);
            }
            else
            {
                Canvas.SetLeft(border, CommandVLineOffsetHEven - border.Width);
            }
        }

        public static void SetEndButton(ref IconTerminal button, int idx)
        {
            if (idx == CommandCount)
            {
                Canvas.SetLeft(button, CommandEndOffsetLeft);
                Canvas.SetTop(button, CommandEndOffsetTop);
                return;
            }

            var col = idx % CommandColumnCount;
            var row = idx / CommandColumnCount;

            Canvas.SetTop(button, 2 + CommandButtonDistanceV * row);

            if (row % 2 == 0)
            {
                Canvas.SetLeft(button, CommandButtonStartEven + 2 + CommandButtonDistanceH * col);
            }
            else
            {
                Canvas.SetLeft(button, CommandButtonStartOdd + 2 - CommandButtonDistanceH * col);
            }
        }
    }
}
