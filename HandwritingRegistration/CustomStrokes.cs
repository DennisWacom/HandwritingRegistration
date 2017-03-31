using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Ink;
using System.Windows;

namespace HandwritingRegistration
{
    [Serializable]
    public sealed class CustomStrokes
    {
        public Point[][] StrokeCollection;

        public CustomStrokes()
        {
        }
        
    }
}
