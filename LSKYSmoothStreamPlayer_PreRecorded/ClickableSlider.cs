using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

namespace LSKYSmoothStreamPlayer_PreRecorded
{
    public class ClickableSlider : Slider
    {
        public ClickableSlider()
        {
            DefaultStyleKey = typeof(Slider);
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            
            RepeatButton rb;
            rb = base.GetTemplateChild("HorizontalTrackLargeChangeIncreaseRepeatButton") as RepeatButton;
            rb.MouseMove += new MouseEventHandler(rb_MouseMove);
            rb.Click += new RoutedEventHandler(rb_Click);
            rb = base.GetTemplateChild("HorizontalTrackLargeChangeDecreaseRepeatButton") as RepeatButton;
            rb.MouseMove += new MouseEventHandler(rb_MouseMove);
            rb.Click += new RoutedEventHandler(rb_Click);
            rb = base.GetTemplateChild("VerticalTrackLargeChangeIncreaseRepeatButton") as RepeatButton;
            rb.Click += new RoutedEventHandler(rb_Click);
            rb.MouseMove += new MouseEventHandler(rb_MouseMove);
            rb = base.GetTemplateChild("VerticalTrackLargeChangeDecreaseRepeatButton") as RepeatButton;
            rb.Click += new RoutedEventHandler(rb_Click);
            rb.MouseMove += new MouseEventHandler(rb_MouseMove);
        }

        Point ps;
        void rb_MouseMove(object sender, MouseEventArgs e)
        {
            ps = e.GetPosition(this);
        }

        public event EventHandler CustomClickEvent;
        void rb_Click(object sender, RoutedEventArgs e)
        {
            if (CustomClickEvent != null)
                CustomClickEvent(this, new PositionEventArgs(ps));
        }

        public class PositionEventArgs : EventArgs
        {
            public Point Position { set; get; }
            public PositionEventArgs() { }
            public PositionEventArgs(Point p) { Position = p; }
        }


    }
}
