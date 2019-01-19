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

namespace ChampionshipSolutions.DM.Dialogs
{
    /// <summary>
    /// Interaction logic for ContactDialogue.xaml
    /// </summary>
    public partial class CustomDataDialogue : Window
    {
        public CustomDataDialogue ( )
        {
            InitializeComponent ( );
        }

        public CustomDataDialogue ( ACustomDataValue Value ) : this ()
        {
            this.Value = Value;
            isNew = false;
        }

        public ACustomDataValue Value { get; private set; }
        private bool isNew = true;

        private void Window_Loaded ( object sender , RoutedEventArgs e )
        {
            this.rbtInt.IsEnabled = isNew;
            this.rbtStr.IsEnabled = isNew;
            this.Key.IsReadOnly = !isNew;

            if ( ! isNew )
            {
                this.Key.Text = Value.key;

                if ( Value is CustomDataValueInt )
                {
                    this.rbtInt.IsChecked = true;
                    this.rbtStr.IsChecked = false;

                    this.IntValue.Value = (int)Value.getValue ( );
                    this.IntValue.Visibility = Visibility.Visible;
                    return;
                }

                if ( Value is CustomDataValueString )
                {
                    this.rbtInt.IsChecked = false;
                    this.rbtStr.IsChecked = true;

                    this.StrValue.Text = (string)Value.getValue ( );
                    this.StrValue.Visibility = Visibility.Visible;
                    return;
                }
            }

        }

        private void RadioButton_Checked ( object sender , RoutedEventArgs e )
        {
            if ( rbtInt.IsChecked == true )
            {
                this.IntValue.Visibility = Visibility.Visible;
                this.StrValue.Visibility = Visibility.Collapsed;
                return;
            }

            if ( rbtStr.IsChecked == true )
            {
                this.StrValue.Visibility = Visibility.Visible;
                this.IntValue.Visibility = Visibility.Collapsed;
                return;
            }
        }

        private void Done_Click ( object sender , RoutedEventArgs e )
        {
            if ( string.IsNullOrWhiteSpace( Key.Text ) )
            {
                MessageBox.Show ( "This custom data must have a key" );
                return;
            }

            if ( ! isNew )
            {
                if (rbtInt.IsChecked == true )
                {
                    Value.setValue ( (int) IntValue.Value ) ;
                    goto finish;
                }

                if (rbtStr.IsChecked == true )
                {
                    Value.setValue ( StrValue.Text );
                    goto finish;
                }
            }
            else
            {
                if ( rbtInt.IsChecked == true )
                {
                    Value = new CustomDataValueInt ( ) { key = this.Key.Text };
                    Value.setValue ( (int) this.IntValue.Value );
                    goto finish;
                }

                if ( rbtStr.IsChecked == true )
                {
                    Value = new CustomDataValueString ( ) { key = this.Key.Text };
                    Value.setValue ( this.StrValue.Text );
                    goto finish;
                }
            }

            finish:

            if ( Value == null )
            {
                MessageBox.Show ( "An unexpected error occurred" );
                return;
            }


            DialogResult = true;

            this.Close ( );
            return;
        }

        private void NumericUpDown_ValueChanged ( object sender , RoutedPropertyChangedEventArgs<decimal> e )
        {

        }

        private void TextBox_TextChanged ( object sender , TextChangedEventArgs e )
        {

        }
    }
}
