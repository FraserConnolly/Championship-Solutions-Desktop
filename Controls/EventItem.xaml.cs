using ChampionshipSolutions.ViewModel;
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
using System.Windows.Navigation;
using System.Windows.Shapes;
using ChampionshipSolutions.Controls;
using ChampionshipSolutions.ControlRoom;

namespace ChampionshipSolutions.Controls
{
    /// <summary>
    /// Interaction logic for EventItem.xaml
    /// </summary>
    public partial class EventItem : UserControl
    {
        public EventItem( )
        {
            InitializeComponent();
        }

        #region DependencyProperty Content

        /// <summary>
        /// Registers a dependency property as backing store for the Content property
        /// </summary>
        public static readonly new DependencyProperty ContentProperty =
           DependencyProperty.Register("Content", typeof(object), typeof(EventItem),
           new FrameworkPropertyMetadata(null,
                FrameworkPropertyMetadataOptions.AffectsRender |
                FrameworkPropertyMetadataOptions.AffectsParentMeasure));

        /// <summary>
        /// Gets or sets the Content.
        /// </summary>
        /// <value>The Content.</value>
        public new object Content
        {
            get { return (object)GetValue(ContentProperty); }
            set
            {
                if (value is EventsPage.GroupedEvent)
                {
                    SetValue(ContentProperty, value);
                    Event = (EventsPage.GroupedEvent)value;
                }
                else
                {
                    throw new ArgumentException("Content must be of type EventVM");
                }
            }
        }

        #endregion


        public EventsPage.GroupedEvent Event
        {
            get
            {
                return (EventsPage.GroupedEvent)this.DataContext;
            }
            private set
            {
                this.DataContext = value;
            }
        }

        private void UserControl_Loaded( object sender, RoutedEventArgs e )
        {
            this.DataContext = ((EventItem)sender).Content;
        }

    }
}
