using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using System.Threading;

namespace ChampionshipSolutions
{
    public partial class Wait : Form
    {

        private object sync;

        public Progress Bar1 { get; set; }
        public Progress Bar2 { get; set; }
        public Progress Bar3 { get; set; }

        Thread refreshThread;

        private bool running;


        public Wait ( )
        {
            InitializeComponent( );

            sync = new object( );
            Bar1 = new Progress( );
            Bar2 = new Progress( );
            Bar3 = new Progress( );
        }

        public Wait ( Progress First , Progress Second , Progress Third )
        {
            InitializeComponent( );

            sync = new object( );

            Bar1 = First;
            Bar2 = Second;
            Bar3 = Third;
        }

        private void Wait_Load ( object sender , EventArgs e )
        {
            lock ( sync )
            {
                running = true;
                refreshThread = new Thread( new ThreadStart( RefreshBars ) );
                refreshThread.Start( );
            }
        }

        public void Complete ( )
        {
            lock ( sync )
            {
                running = false;
                this.Invoke( new Action( ( ) =>
              {
                  this.Close( );
              } ) );

                if ( refreshThread != null )
                    refreshThread.Abort( );
            }
        }


        public void Start ( object Wait )
        {
            Wait wait = (Wait)Wait;
            wait.ShowDialog( );
        }

        private void RefreshBars ( )
        {
            try
            {
                while ( running )
                {
                    lock ( sync )
                    {
                        if ( Bar1.Updated && running )
                        {
                            this.Invoke( (MethodInvoker) delegate
                              {
                                  this.progressBar1.Visible = Bar1.Enabled;
                                  this.label1.Visible = Bar1.Enabled;
                                  this.progressBar1.Maximum = Bar1.Max;
                                  this.progressBar1.Value = Bar1.Value;
                                  this.label1.Text = Bar1.Label;
                              } );
                            Bar1.Updated = false;
                        }
                        if ( Bar2.Updated && running )
                        {
                            this.Invoke( (MethodInvoker) delegate
                              {
                                  this.progressBar2.Visible = Bar2.Enabled;
                                  this.label2.Visible = Bar2.Enabled;
                                  this.progressBar2.Maximum = Bar2.Max;
                                  this.progressBar2.Value = Bar2.Value;
                                  this.label2.Text = Bar2.Label;
                              } );
                            Bar2.Updated = true;
                        }
                        if ( Bar3.Updated && running )
                        {
                            this.Invoke( (MethodInvoker) delegate
                              {
                                  this.progressBar3.Visible = Bar3.Enabled;
                                  this.label3.Visible = Bar3.Enabled;
                                  this.progressBar3.Maximum = Bar3.Max;
                                  this.progressBar3.Value = Bar3.Value;
                                  this.label3.Text = Bar3.Label;
                              } );
                            Bar3.Updated = true;
                        }
                    }
                    Thread.Sleep( 100 );
                }
            }
            catch ( Exception )
            {
            }
        }
    }

    public class Progress
    {

        public Progress()
        {
            Max = 1;
            Value = 0;
            Enabled = false;
            Label = "";
            Updated = true;
        }

        public Progress(string Name, int Maximum, int StartingValue, bool Show)
        {
            Max = Maximum;
            Value = StartingValue;
            Enabled = Show;
            Label = Name;
            Updated = true;
        }

        public int Max { get; private set; }
        public int Value { get; private set; }
        public bool Enabled { get; private set; }
        public string Label { get; private set; }

        public bool Updated { get; set; }

        public void setMax(int Maximum)
        {
            Max = Maximum;
            Updated = true;
        }

        public void setValue(int CurrentValue)
        {
            Value = CurrentValue;
            if (Value > Max) { Value = Max; }
            Updated = true;
        }

        public void setEnabled(bool Show)
        {
            Enabled = Show;
            Updated = true;
        }

        public void setLabel(string Text)
        {
            Label = Text;
            Updated = true;
        }

        public void increment()
        {
            Value++;
            if (Value > Max) { Value = Max; }
            Updated = true;
        }

        public void increment(int IncrementBy)
        {
            Value = Value + IncrementBy;
            if (Value > Max) { Value = Max; }
            Updated = true;
        }

    }

}
