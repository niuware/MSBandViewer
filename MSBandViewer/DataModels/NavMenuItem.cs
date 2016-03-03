using System;
using Windows.UI.Xaml.Controls;

namespace Niuware.MSBandViewer.DataModels
{
    /// <summary>
    /// Item for the SplitView pane navigation menu
    /// </summary>
    public class NavMenuItem
    {
        public string Label { get; set; }
        public Symbol Symbol { get; set; }
        public char SymbolAsChar
        {
            get
            {
                return (char)this.Symbol;
            }
        }

        public Type DestPage { get; set; }
        public object Arguments { get; set; }
    }
}
