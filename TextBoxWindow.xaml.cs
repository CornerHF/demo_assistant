using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace DemoAssistant
{
    /// <summary>
    /// Interaction logic for TextBoxWindow.xaml
    /// </summary>
    public partial class TextBoxWindow : Window
    {
        public event Action<string> OnDoubleClickText = null;

        private List<string> _textList = new List<string>();
        public List<string> TextList
        {
            get
            {
                return _textList;
            }

            set
            {
                _textList = value ?? new List<string>();

                listView_fileName.ItemsSource = _textList;
            }
        }


        public TextBoxWindow()
        {
            InitializeComponent();
        }

        public void OnDoubleClickTextControl(object sender, MouseButtonEventArgs e)
        {
            if (sender is ContentControl contentControl)
            {
                for (int i = 0; i < VisualTreeHelper.GetChildrenCount(contentControl); ++i)
                {
                    var _child = VisualTreeHelper.GetChild(contentControl, i);
                    if (_child is ContentPresenter contentPresenter)
                    {
                        for (int j = 0; j < VisualTreeHelper.GetChildrenCount(contentPresenter); j++)
                        {
                            var innerChild = VisualTreeHelper.GetChild(contentPresenter, j);

                            if (innerChild is TextBlock textBlock)
                            {
                                OnDoubleClickText?.Invoke(textBlock.Text);
                            }
                        }
                    }
                }
            }
        }
    }
}
