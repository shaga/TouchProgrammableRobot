using System;
using Microsoft.SPOT;
using Microsoft.SPOT.Presentation;
using Microsoft.SPOT.Presentation.Controls;
using Microsoft.SPOT.Presentation.Media;
using Color = Gadgeteer.Color;

namespace TouchProgrammableRobotGadgeteer.View
{
    class TextButton : MyButton
    {
        private readonly Text _label;

        public string Text
        {
            get { return _label.TextContent; }
            set { _label.TextContent = value; }
        }

        public TextButton() : base()
        {
            _label = new Text(Resources.GetFont(Resources.FontResources.NinaB), "");
            _label.VerticalAlignment = VerticalAlignment.Center;
            _label.TextAlignment = TextAlignment.Center;
            Child = _label;
        }

        public TextButton(string label) : this()
        {
            _label.TextContent = label;
        }
    }
}
