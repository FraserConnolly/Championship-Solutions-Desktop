using ChampionshipSolutions.ViewModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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

namespace ChampionshipSolutions
{
    /// <summary>
    /// Interaction logic for EventsPage.xaml
    /// </summary>
    public partial class EventsPage : Page
    {
        public EventsPage()
        {
            InitializeComponent();


        }

        public void ReloadPage()
        {
            if ( !IsInitialized ) return;

            Page_Loaded ( null , null );
        }

        ObservableCollection<EventVM> Events = (( App )Application.Current ).CurrentChampionship.Events;
        ObservableCollection<GroupVM> Groups = (( App )Application.Current ).CurrentChampionship.Groups;

        private static DependencyObject GetExpander ( DependencyObject container )
        {
            if ( container is Expander ) return container;

            for ( var i = 0 ; i < VisualTreeHelper.GetChildrenCount ( container ) ; i++ )
            {
                var child = VisualTreeHelper.GetChild(container, i);

                var result = GetExpander(child);
                if ( result != null )
                {
                    return result;
                }
            }

            return null;
        }

        private void Page_Loaded ( object sender , RoutedEventArgs e )
        {
            System.Diagnostics.Stopwatch watch = new System.Diagnostics.Stopwatch();

            watch.Start ( );

            Events = ( (App)Application.Current ).CurrentChampionship.Events;
            Groups = ( (App)Application.Current ).CurrentChampionship.Groups;

            foreach ( EventVM Event in Events )
            {
                Event.Groups.Count ( );
                Event.CountEntered.ToString ( );
                Event.CountSelected.ToString ( );
                Event.CountResults.ToString ( );
                Event.CustomData.Count ( );
                Event.Standards.Count ( );
                if ( Event.CertificateTemplate != null )
                    Event.CertificateTemplate.ToString ( );
                if ( Event.VestTemplate != null )
                    Event.VestTemplate.ToString ( );
                if ( Event.ResultsTemplate != null )
                    Event.ResultsTemplate.ToString ( );
                if ( Event.DataEntryTemplate != null )
                    Event.DataEntryTemplate.ToString ( );
            }

            if ( this.lvEvents.ItemsSource != null )
            {
                //    //List<GroupedEvent> openGroups;

                //    ////foreach( object o in this.lvEvents.Items )
                //    ////{
                //    ////    var listBoxItem =
                //    ////        lvEvents.ItemContainerGenerator.ContainerFromItem(o) as ContentPresenter;
                //    ////    var itemExpander = (Expander) GetExpander(listBoxItem);
                //    ////    if ( itemExpander != null )
                //    ////        itemExpander.IsExpanded = false;
                //    ////}

                //    //foreach ( object obj in this.lvEvents.ItemContainerGenerator.Items )
                //    //{
                //    //    //if ( !( obj is GroupedEvent ) ) continue;

                //    //    //GroupedEvent ge = (GroupedEvent)obj;

                //    //    //var listBoxItem =
                //    //        ////lvEvents.ItemContainerGenerator.ContainerFromItem(o) as ContentPresenter;
                //    //    var itemExpander = (Expander) GetExpander((DependencyObject) obj);
                //    //    if ( itemExpander != null )
                //    //        itemExpander.IsExpanded = false;

                //    //}
            }


            List<List<GroupVM>> GroupKeys = new List<List<GroupVM>>();
            List<GroupedEvent> GroupedEvents = new List<GroupedEvent>();

            watch.Stop ( );
            watch.Start ( );

            GroupVM Ungrouped = new GroupVM( new DM.Group () { Name = "Ungrouped" } , (( App )Application.Current ).CurrentChampionship);
            GroupVM All = new GroupVM( new DM.Group () { Name = "All" } , (( App )Application.Current ).CurrentChampionship);

            foreach ( EventVM Event in Events )
                GroupedEvents.Add ( new GroupedEvent ( ) { Event = Event , Groups = new List<GroupVM> ( ) { All } } );

            foreach ( EventVM Event in Events.Where ( ev => ev.Groups.Count == 0 ) )
                GroupedEvents.Add ( new GroupedEvent ( ) { Event = Event , Groups = new List<GroupVM> ( ) { Ungrouped } } );

            watch.Stop ( );
            watch.Start ( );

            // find groups that have events
            foreach ( GroupVM Group in Groups )
                if ( hasEvents ( new List<GroupVM> ( ) { Group } ) )
                    GroupKeys.Add ( new List<GroupVM>() { Group } );


            watch.Stop ( );
            watch.Start ( );

            // find group combinations

            bool moreGroupsAdded = true;
            int counter = 0;

            while ( moreGroupsAdded )
            {
                moreGroupsAdded = false;

                foreach ( List<GroupVM> GroupArray in GroupKeys.Where(c => c.Count > counter ).ToArray ( ) )
                {   
                    foreach ( GroupVM Group in Groups )
                    {
                        // Do not check for a group that is already in the array
                        if ( GroupArray.Contains ( Group ) ) continue;

                        List<GroupVM> tempArrary = new List<GroupVM>();
                        tempArrary.AddRange ( GroupArray );
                        tempArrary.Add ( Group );

                        if ( hasEvents ( tempArrary ) )
                        {
                            GroupKeys.Add ( tempArrary );
                            moreGroupsAdded = true;
                        }
                    }
                }
                counter++;
            }

            // order the lists so we can compare them
            for ( int i = 0 ; i < GroupKeys.Count ; i++) 
                GroupKeys[i] = GroupKeys[i].OrderBy ( g => g.Group.ID ).ToList();

            // null duplicates
            for ( int i = 0 ; i < GroupKeys.Count ; i++ ) 
                for ( int j = i + 1 ; j < GroupKeys.Count ; j++ )
                    if ( GroupKeys[i] != null && GroupKeys[j] != null )
                        if ( GroupKeys[j].Except ( GroupKeys[i] ).Count ( ) == 0 )
                            GroupKeys[j] = null;

            // Populate the GroupedEvents list.
            foreach ( List<GroupVM> GroupArray in GroupKeys )
            {
                if ( GroupArray == null ) continue;
                foreach ( EventVM Event in Events ) 
                {
                    foreach ( GroupVM Group in GroupArray )
                    {
                        if ( !Event.Groups.Contains ( Group ) )
                        {
                            goto Skip;
                        }
                    }

                    GroupedEvents.Add ( new GroupedEvent ( ) { Event = Event , Groups = GroupArray } );
                    Skip: continue;
                }
            }

            try
            {
                this.lvEvents.ItemsSource = GroupedEvents;
                CollectionView view = (CollectionView)CollectionViewSource.GetDefaultView(lvEvents.ItemsSource);
                view.GroupDescriptions.Add ( new PropertyGroupDescription ( "GroupHeading" ) );
            }
            catch ( Exception )
            {
                Console.WriteLine ( "Load exception" );
                //throw;
            }


            watch.Stop ( );

            Console.WriteLine ( "Event Page load took " + watch.Elapsed.TotalSeconds.ToString ( ) );

        }

        private bool hasEvents ( List<GroupVM> groupsToCheck )
        {

            foreach ( EventVM Event in Events.Where(ev => ev.Event.Groups.Count() > 0 ) )
            {
            bool hasEvents = true;
                foreach ( GroupVM Group in groupsToCheck )
                    if ( !Event.Groups.Contains ( Group ) )
                    {
                        hasEvents = false;
                        continue;
                    }
                //else
                //{
                //    return true;
                //}
                if ( hasEvents ) return true;
            }
            return false;
        }

        public struct GroupedEvent
        {
            public string GroupHeading
            {
                get
                {
                    if ( Groups == null ) return string.Empty;
                    if ( Groups.Count == 0 ) return string.Empty;

                    StringBuilder sb = new StringBuilder();

                    foreach ( GroupVM Group in Groups )
                    {
                        sb.Append ( Group.Name );
                        sb.Append ( " &  " );
                    }

                    return sb.ToString ( ).Remove ( sb.Length - 4 ); 
                }
            }
            public List<GroupVM> Groups { get; set; }
            public EventVM Event { get; set; }
            //public bool isOpen { get; set; }
        }

        private void Expander_Expanded ( object sender , RoutedEventArgs e )
        {

        }
    }
}
