using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Data;
using ChampionshipSolutions.DM;
using System.Diagnostics;

using FConn.Reporting;
using ChampionshipSolutions.ControlRoom;

namespace ChampionshipSolutions
{
    class Print
    {
        private MainWindow _main;
        private static object ExcelLock = new object();

        //public Print(MainWindow main)
        //{
        //    //_main = main;
        //    //creatResultFolders();

        //    //events = MainWindow.CurrentChampionship.Events.ToList();
        //    //teams = MainWindow.CurrentChampionship.Teams.ToList();
        //}

        //public void creatResultFolders()
        //{
        //    createDirectory(@Properties.Settings.Default.ResultsPath);
        //    createDirectory(@Properties.Settings.Default.ResultsPDFPath);

        //    //AEvent events = new AEvent();

        //    foreach (AEvent e in MainWindow.CurrentChampionship.Events.ToList())
        //    {
        //        createDirectory(@Properties.Settings.Default.ResultsPath + "\\" + e.ShortName + "\\");
        //        createDirectory(@Properties.Settings.Default.ResultsPDFPath + "\\" + e.ShortName + "\\");
        //    }

        //}

        private void createDirectory(string dir)
        {
            if (!System.IO.Directory.Exists(dir))
            {
                System.IO.Directory.CreateDirectory(dir);
            }
        }

        // Moved to Exports
        //public void printVestNo(List<Team> areas, List<AEvent> events)
        //{
        //    Thread printVestThread = new Thread(new ParameterizedThreadStart(generateVestNumbersPDF));
        //    printVestThread.Start(new Instuction(events, areas, false));
        //}


        public void printProgrammePages()
        {
            Thread printProgrammeThread = new Thread(new ThreadStart(generateProgramme));
            printProgrammeThread.Start();
        }

        public void printBothResults(List<AEvent> events, bool open = false, bool Print = false)
        {
            //Thread printThread = new Thread(new ParameterizedThreadStart(printResultsSheet));
            //printThread.Start(new Instuction(events, false));

            //Thread printTeamThead = new Thread(new ParameterizedThreadStart(printTeamResultsSheet));
            //printTeamThead.Start(new Instuction(events, false));

            Instuction ins = new Instuction(events, Print);
            ins.open = open;

            printResultsSheet(ins);
            printTeamResultsSheet(ins);
        }

        /// <summary>
        /// Only returns File Names if the process is done synchronously
        /// </summary>
        public string[] printResults(List<AEvent> events, bool Asyncronis = true)
        {
            if (Asyncronis)
            {
                Thread printThread = new Thread(new ParameterizedThreadStart(printResultsSheetV));
                printThread.Start(new Instuction(events, false));
                return new string[] { };
            }
            else
            {
                return printResultsSheet(new Instuction(events, false));
            }
        }

        /// <summary>
        /// Only returns File Names if the process is done synchronously
        /// </summary>
        public string[] printTeamResults(List<AEvent> events, bool Asyncronis = true)
        {
            if (Asyncronis)
            {
                Thread printTeamThead = new Thread(new ParameterizedThreadStart(printTeamResultsSheetV));
                printTeamThead.Start(new Instuction(events, false));
                return new string[] { };
            }
            else
            {
                return printTeamResultsSheet(new Instuction(events, false));
            }
        }

        private void printResultsSheetV (object Instructions) { printResultsSheet(Instructions); }

        private void printTeamResultsSheetV(object Instructions) { printTeamResultsSheet(Instructions); }

        private string[] printTeamResultsSheet(object Instructions)
        {
            Instuction instructions = (Instuction)Instructions;

            List<AEvent> events = instructions.events;
            bool print = instructions.print;
            bool open = instructions.open;

            Wait wait = new Wait();
            wait.Bar1.setLabel("Competitors");
            wait.Bar2.setLabel("Areas");
            wait.Bar3.setLabel("Events");

            wait.Bar1.setEnabled(true);
            wait.Bar2.setEnabled(true);

            wait.Bar3.setMax(events.Count);
            wait.Bar3.setEnabled(true);

            Thread t = new Thread(new ParameterizedThreadStart(wait.Start));
            t.Start(wait);

            List<string> fileNames = new List<string>();

            lock (ExcelLock)
            {

                Microsoft.Office.Interop.Excel._Application excel = new Microsoft.Office.Interop.Excel.Application();
                Microsoft.Office.Interop.Excel.Workbook wb;
                Microsoft.Office.Interop.Excel.Worksheet sheet;

                try
                {
                    wb = excel.Workbooks.Open(@Properties.Settings.Default.OLD_RootPathTemplate + "TeamResults.xlsx", false, false);
                    sheet = wb.ActiveSheet;
                }
                catch
                {
                    wait.Complete();
                    t.Abort();
                    return new string[] { };
                }


                //AResult results;//= new AResult();
                foreach (AEvent e in events)
                {
                    // Clear cells from the last event

                    sheet.Range["Clear1"].Value = "";
                    sheet.Range["Clear2"].Value = "";

                    bool competitors = false;
                    List<Team> areas = MainWindow.CurrentChampionship.listAllTeams();

                    wait.Bar2.setValue(0);
                    wait.Bar2.setMax(areas.Count);

                    //// WSAA 2014-15
                    var topData = new object[14, 5];
                    var bottomData = new object[1, 5];

                    // SW 2015
                    //var topData = new object[14, 7];
                    //var bottomData = new object[1, 7];

                    foreach (Team a in areas)
                    {
                        List<AResult> lresults = e.Results.Where(f => f.printTeam() == a.Name && f.isComplete()).ToList();
                        // Condition removed 2016-01-15 so that empty results sheets will be made.
                        //if (lresults.Count > 0)
                        if (true)
                        {
                            competitors = true;
                            wait.Bar1.setValue(0);
                            wait.Bar1.setMax(lresults.Count);


                            int i = 0;  //changed from 1 when changing to range based updates
                            int column;
                            switch (a.Name)
                            {
                                case "Avon":
                                    column = 1;
                                    break;
                                case "Cornwall":
                                    column = 2;
                                    break;
                                case "Devon":
                                    column = 3;
                                    break;
                                case "Dorset":
                                    column = 4;
                                    break;
                                case "Gloucestershire":
                                    column = 5;
                                    break;
                                case "Somerset":
                                    column = 6;
                                    break;
                                case "Wiltshire":
                                    column = 7;
                                    break;


                                case "Swindon":
                                    column = 4;
                                    break;
                                case "North Wiltshire":
                                    column = 2;
                                    break;
                                case "West Wiltshire":
                                    column = 5;
                                    break;
                                case "Kennet":
                                    column = 1;
                                    break;
                                case "Salisbury":
                                    column = 3;
                                    break;
                                default:
                                    continue;
                            }


                            //int column = a.getLogicalNumber();

                            foreach (AResult r in lresults.OrderBy(f => f.Rank.Value))
                            {
                                topData[i, column - 1] = string.Format("{0} / {1} / Y{4} {2} {3}", r.Rank.Value.ToString(), r.printVestNo(), "\n", r.Competitor.getName(), r.printParameter("YearGroup")); //((StudentCompetitor)r.Competitor).YearGroup);

                                wait.Bar1.increment();
                                i++;
                            }

                            foreach (ScoringTeam ar in e.getScoringTeams().Where(f => f.ScoringTeamName == "A" && f.Team == a))// results.getAreaResults(e, "A", a))
                            {
                                // row chanced for SW 2014-15
                                //topData[10, column - 1] = ar.printSumOfPositions();

                                topData[13, column - 1] = ar.printSumOfPositions();
                                // not required for SW 2014-15
                                bottomData[0, column - 1] = ar.printPoints();
                            }

                            //i = 1;

                            //foreach (AreaResult ar in results.getAreaResults("B"))
                            //{
                            //    string range = "BTeamHeader";
                            //    sheet.Range[range].Offset[i, 0].Value = ar.area.getCode();
                            //    sheet.Range[range].Offset[i, 1].Value = ar.positions;
                            //    sheet.Range[range].Offset[i, 2].Value = ar.total;
                            //    sheet.Range[range].Offset[i, 3].Value = ar.points;

                            //    i++;
                            //}

                            var writeRange = sheet.Range["Clear1"];
                            writeRange.Value2 = topData;

                            // not required for SW 2014-15
                            writeRange = sheet.Range["Clear2"];
                            writeRange.Value2 = bottomData;

                            sheet.Range["AreaName"].Value = e.Name;


                        }
                        wait.Bar2.increment();
                    }
                    if (competitors)
                    {
                        try
                        {
                            if (@Properties.Settings.Default.SaveExcel)
                            {
                                wb.SaveAs(@Properties.Settings.Default.ResultsPath + e.Name + " Team.xlsx");
                                fileNames.Add(@Properties.Settings.Default.ResultsPath + e.Name + " Team.xlsx");
                            }

                            wb.ExportAsFixedFormat(Microsoft.Office.Interop.Excel.XlFixedFormatType.xlTypePDF, @Properties.Settings.Default.ResultsPDFPath + e.Name + " Team.pdf");
                            fileNames.Add(@Properties.Settings.Default.ResultsPDFPath + e.Name + " Team.pdf");
                            //new AdobePrn(@Properties.Settings.Default.ResultsPDFPath + e.ShortName + "\\Team.pdf", @Properties.Settings.Default.prnOpA3ResultsByTeam, _main);

                            if (open)
                                Process.Start(@Properties.Settings.Default.ResultsPDFPath + e.Name + " Team.pdf");

                            if (print)
                                Printing.PrintPDF(@Properties.Settings.Default.ResultsPDFPath + e.Name + " Team.pdf");
                        }
                        catch
                        {
                            MessageBox.Show("Could not save the file " + @Properties.Settings.Default.ResultsPDFPath + e.Name + " Team.pdf");
                        }
                    }
                    wait.Bar3.increment();
                }// for each event

                wb.Close(false);
                excel.Quit();
            }// end ExcelLock

            wait.Complete();
            t.Abort();
            return fileNames.ToArray();

        }

        private string[] printResultsSheet(object Instructions)
        {

            Instuction instructions = (Instuction)Instructions;

            List<AEvent> events = instructions.events;
            bool print = instructions.print;
            bool open = instructions.open;

            Wait wait = new Wait();
            wait.Bar1.setLabel("Competitors");
            wait.Bar2.setLabel("Events");
            wait.Bar1.setEnabled(true);
            wait.Bar2.setEnabled(true);
            wait.Bar2.setMax(events.Count);

            Thread t = new Thread(new ParameterizedThreadStart(wait.Start));
            t.Start(wait);

            List<string> fileNames = new List<string>();

            lock (ExcelLock)
            {

                Microsoft.Office.Interop.Excel._Application excel = new Microsoft.Office.Interop.Excel.Application();
                Microsoft.Office.Interop.Excel.Workbook wb;
                Microsoft.Office.Interop.Excel.Worksheet sheet;

                try
                {
                    wb = excel.Workbooks.Open(@Properties.Settings.Default.OLD_RootPathTemplate + "EventResults.xlsx", false, false);
                    sheet = wb.ActiveSheet;
                }
                catch
                {
                    wait.Complete();
                    t.Abort();
                    return new string[] { };
                }

                //AResult results;// = new Results();

                foreach (AEvent e in events)
                {
                    List<AResult> lresults = e.Results.Where(r => r.isComplete()).ToList();
                    
                    // Condition removed 2016-01-15 so that empty results sheets will be made.
                    //if (lresults.Count > 0)
                    if (true)
                    {
                        wait.Bar1.setValue(0);
                        wait.Bar1.setMax(lresults.Count);


                        // WSAA 2014-15
                        var leftData = new object[46, 5];
                        var rightData = new object[26, 5];
                        var ATeamData = new object[5, 5];
                        var BTeamData = new object[5, 5];
                        var OverallData = new object[5, 5];
                        // Clear cells from the last event
                        sheet.Range["Clear1"].Value = ""; // left
                        sheet.Range["Clear2"].Value = ""; // right
                        sheet.Range["Clear3"].Value = ""; // ATeam
                        sheet.Range["Clear4"].Value = ""; // BTeam
                        sheet.Range["Clear5"].Value = ""; // Overall


                        //// SW 2014-15
                        //var leftData = new object[46, 5];
                        //var rightData = new object[32, 5];
                        //var ATeamData = new object[7, 5];
                        ////var OverallData = new object[5, 5];
                        //// Clear cells from the last event
                        //sheet.Range["Clear1"].Value = ""; // left
                        //sheet.Range["Clear2"].Value = ""; // right
                        //sheet.Range["Clear3"].Value = ""; // ATeam

                        int highestRank = 0;

                        foreach (AResult r in lresults)
                        {

                            if (r.Rank.Value < 47)
                            {
                                // use leftData
                                int row = r.Rank.Value;

                                leftData[row - 1, 0] = r.printRank;
                                leftData[row - 1, 1] = r.printVestNo();
                                leftData[row - 1, 2] = string.Format("{0}{1} - {2} - {3}", (r.CertificateEarned ? "*" : ""), r.printName(), r.printResultValueString(), r.printParameter("Attends"));
                                leftData[row - 1, 3] = r.printParameter("YearGroup"); //((StudentCompetitor) r.Competitor).YearGroup;
                                leftData[row - 1, 4] = r.printTeamShort();

                            }
                            else
                            {
                                int row = r.Rank.Value - 46;

                                // use rightData
                                rightData[row - 1, 0] = r.printRank;
                                rightData[row - 1, 1] = r.printVestNo();
                                rightData[row - 1, 2] = string.Format("{0}{1} - {2} - {3}", (r.CertificateEarned ? "*" : ""), r.printName(), r.printResultValueString(), r.printParameter("Attends"));
                                rightData[row - 1, 3] = r.printParameter("YearGroup"); //((StudentCompetitor)r.Competitor).YearGroup;
                                rightData[row - 1, 4] = r.printTeamShort();

                            }

                            if (r.Rank > highestRank)
                                highestRank = r.Rank.Value;

                            wait.Bar1.increment();
                        }

                        // move away from the last entered rank
                        highestRank++;

                        List<AResult> DNFresults = e.Results.Where(r => r.getTypeDescription() == ResultTypeDescription.CompetativeDNF).ToList();

                        foreach (AResult r in DNFresults)
                        {

                            if (highestRank < 47)
                            {
                                // use leftData
                                int row = highestRank;

                                leftData[row - 1, 0] = r.printRank;
                                leftData[row - 1, 1] = r.printVestNo();
                                leftData[row - 1, 2] = string.Format("{0}{1} - {2} - {3}", (r.CertificateEarned ? "*" : ""), r.printName(), r.printResultValueString(), r.printParameter("Attends"));
                                leftData[row - 1, 3] = r.printParameter("YearGroup"); //((StudentCompetitor) r.Competitor).YearGroup;
                                leftData[row - 1, 4] = r.printTeamShort();

                            }
                            else
                            {
                                int row = highestRank - 46;

                                // use rightData
                                rightData[row - 1, 0] = r.printRank;
                                rightData[row - 1, 1] = r.printVestNo();
                                rightData[row - 1, 2] = string.Format("{0}{1} - {2} - {3}", (r.CertificateEarned ? "*" : ""), r.printName(), r.printResultValueString(), r.printParameter("Attends"));
                                rightData[row - 1, 3] = r.printParameter("YearGroup"); //((StudentCompetitor)r.Competitor).YearGroup;
                                rightData[row - 1, 4] = r.printTeamShort();

                            }

                            highestRank++;

                        }

                        int i = 0;

                        foreach (ScoringTeam ar in e.getScoringTeams().Where(f => f.ScoringTeamName == "A").OrderBy(f => f.orderableRank()))// results.getAreaResults(e, "A"))
                        {

                            if (ar.Points == 0) continue;

                            ATeamData[i, 0] = "    " + ar.Team.ShortName;
                            ATeamData[i, 2] = ar.printPositions();
                            ATeamData[i, 3] = ar.printSumOfPositions();
                            ATeamData[i, 4] = ar.printPoints();

                            i++;
                        }

                        i = 0;

                        // Not used for SW 2014-15
                        foreach (ScoringTeam ar in e.getScoringTeams().Where(f => f.ScoringTeamName == "B").OrderBy(f => f.orderableRank()))
                        {
                            if (ar.Points == 0) continue;

                            BTeamData[i, 0] = "    " + ar.Team.ShortName;
                            BTeamData[i, 2] = ar.printPositions();
                            BTeamData[i, 3] = ar.printSumOfPositions();
                            BTeamData[i, 4] = ar.printPoints();

                            i++;
                        }

                        i = 0;

                        foreach (ChampionshipTeamResult or in e.Championship.getOverallSores().Where(f => f.ScoringTeamName == "A").OrderBy(f => f.orderableRank()))// results.getOverAllResults("A"))
                        {
                            OverallData[i, 0] = "    " + or.Team.Name;

                            ChampionshipTeamResult bResult = e.Championship.getOverallSores().Where(f => f.ScoringTeamName == "B" && f.Team == or.Team).FirstOrDefault();

                            if (bResult != null)
                            {
                                OverallData[i, 3] = string.Format("{0} / {1}", or.Points, bResult.Points);
                            }
                            else
                            {
                                OverallData[i, 3] = string.Format("{0} / {1}", or.Points, 0);
                            }

                            i++;
                        }

                        sheet.Range["TeamName"].Value = e.Name;

                        var writeRange = sheet.Range["Clear1"];
                        writeRange.Value2 = leftData;

                        writeRange = sheet.Range["Clear2"];
                        writeRange.Value2 = rightData;

                        writeRange = sheet.Range["Clear3"];
                        writeRange.Value2 = ATeamData;

                        // not used for SW 2014-15
                        writeRange = sheet.Range["Clear4"];
                        writeRange.Value2 = BTeamData;

                        writeRange = sheet.Range["Clear5"];
                        writeRange.Value2 = OverallData;

                        try
                        {
                            if (@Properties.Settings.Default.SaveExcel)
                            {
                                wb.SaveAs(@Properties.Settings.Default.ResultsPath + e.Name + " Full.xlsx");
                                fileNames.Add(@Properties.Settings.Default.ResultsPath + e.Name + " Full.xlsx");
                            }
                            wb.ExportAsFixedFormat(Microsoft.Office.Interop.Excel.XlFixedFormatType.xlTypePDF, @Properties.Settings.Default.ResultsPDFPath + e.Name + " Full.pdf");
                            fileNames.Add(@Properties.Settings.Default.ResultsPDFPath + e.Name + " Full.pdf");
                            //new AdobePrn(@Properties.Settings.Default.ResultsPDFPath + e.ShortName + "\\Full.pdf", @Properties.Settings.Default.prnOpA3Results, _main);

                            if (open)
                                Process.Start(@Properties.Settings.Default.ResultsPDFPath + e.Name + " Full.pdf");

                            if (print)
                            {
                                Printing.PrintPDF(@Properties.Settings.Default.ResultsPDFPath + e.Name + " Full.pdf");
                            }
                        }
                        catch
                        {
                            MessageBox.Show("Could not save the file " + Properties.Settings.Default.ResultsPDFPath + e.Name + " Full.pdf");
                        }
                        wait.Bar2.increment();

                    }
                }

                wb.Close(false);
                excel.Quit();
            } // end Excel Lock
            wait.Complete();
            t.Abort();
            return fileNames.ToArray();
        }



        ///// <summary>
        ///// This may not work in 2015 because _main does not have an Invoke method
        ///// </summary>
        ///// <returns></returns>
        //private bool requestFolder()
        //{
        //    FolderBrowserDialog obd;
        //    DialogResult result = new DialogResult();
        //    obd = new FolderBrowserDialog();

        //    //_main.Invoke((MethodInvoker)delegate
        //    //{
        //    //    //obd = new FolderBrowserDialog();
        //    //    result = obd.ShowDialog();
        //    //});

        //    result = obd.ShowDialog();

        //    if (result == DialogResult.OK)
        //    {
        //        FolderPath = obd.SelectedPath + "\\";
        //        FolderPDFPath = FolderPath + "PDF\\";
        //        try
        //        {
        //            System.IO.Directory.CreateDirectory(FolderPDFPath);
        //        }
        //        catch
        //        {
        //            MessageBox.Show("An error occurred creating a directory for the PDFs to be stored");
        //            return false;
        //        }
        //    }
        //    else
        //    {
        //        return false;
        //    }
        //    return true;

        //}

        /// <summary>
        /// 
        /// </summary>
        /// <param name="Competitor"></param>
        /// <param name="TemplatePath"></param>
        /// <returns>File path to PDF.</returns>
        /// To do merge this function with the Exports._generateVestNumbersPDF
        public string generateSingleVestNumberPDF(ACompetitor Competitor, string TemplatePath)
        {
            if (Competitor == null) return null;

            PDFReport VestPDF;
            DataTable dataTable = new DataTable();

            dataTable.Columns.Add("Name");
            dataTable.Columns.Add("School");
            dataTable.Columns.Add("AreaShortName");
            dataTable.Columns.Add("AreaName");
            dataTable.Columns.Add("VestNo");
            //dataTable.Columns.Add("VestBarcode");
            dataTable.Columns.Add("EventCode");

            try
            {
                TemplatePath = "XCVestNumber";
                VestPDF = (PDFReport)MainWindow.ReportLibrary[TemplatePath];
            }
            catch
            {
                return null;
            }

            dataTable.Rows.Clear();

            Competitor c = (Competitor)Competitor;

            DataRow dr = dataTable.NewRow();

            dr["AreaName"] = c.getTeam().Name.ToUpper();
            dr["EventCode"] = c.CompetingIn.ShortName;
            dr["VestNo"] = c.Vest.ToString();

            string SchoolName = "";

            if (c.checkParameter("Attends") != null)
                SchoolName = c.checkParameter("Attends").ToString();

            object yearGroup = c.checkParameter("YearGroup");
            dr["Name"] = c.Athlete.PrintName() + " - Year " + (yearGroup != null ? yearGroup.ToString() : " ").ToString(); // sc.formatYearGroup();
            dr["School"] = SchoolName;
            dr["AreaShortName"] = c.getTeam().ShortName;
            //string BarCode = "*" + "V-" + c.Vest.ToString() + "%E-" + c.CompetingIn.ShortName + "*";
            //dr["VestBarcode"] = BarCode;

            dataTable.Rows.Add(dr);

            string filePath = System.IO.Path.GetTempFileName() + ".pdf";

            VestPDF.ExportPath = filePath;
            VestPDF.Generate(dataTable);
            VestPDF.Save();

            return filePath;

        }

        // moved to Exports Class
        //private void generateVestNumbersPDF(object Instructions)
        //{
        //    string lFolderPath = @Properties.Settings.Default.VestExportPath;
        //    string TemplatePath = @Properties.Settings.Default.OLD_RootPathTemplate + "2014-15 Vest";

        //    Stopwatch sw = new Stopwatch();

        //    sw.Start();

        //    PDFReport VestPDF;
        //    //PDFReport VestPDF = new PDFReport();
        //    DataTable dataTable = new DataTable();

        //    //VestPDF.fields.Add(new RectagleField() { Name = "Full Name",    Column = "Name",            FontSize = 14,  Rectangle = new System.Drawing.Rectangle(30, 255, 200, 20), Alignment = PdfSharp.Drawing.Layout.XParagraphAlignment.Left });
        //    dataTable.Columns.Add("Name");
        //    //VestPDF.fields.Add(new RectagleField() { Name = "School",       Column = "School",          FontSize = 14,  Rectangle = new System.Drawing.Rectangle(230, 255, 140, 20), Alignment = PdfSharp.Drawing.Layout.XParagraphAlignment.Center });
        //    dataTable.Columns.Add("School");
        //    //VestPDF.fields.Add(new RectagleField() { Name = "AreaCode", Column = "AreaShortName", FontSize = 100, Rectangle = new System.Drawing.Rectangle(19, -15, 172, 63), Alignment = PdfSharp.Drawing.Layout.XParagraphAlignment.Center });
        //    dataTable.Columns.Add("AreaShortName");
        //    //VestPDF.fields.Add(new RectagleField() { Name = "Area",         Column = "AreaName",        FontSize = 14,  Rectangle = new System.Drawing.Rectangle(430, 255, 130, 20), Alignment = PdfSharp.Drawing.Layout.XParagraphAlignment.Right });
        //    dataTable.Columns.Add("AreaName");
        //    //VestPDF.fields.Add(new RectagleField() { Name = "VestNo",       Column = "VestNo",          FontSize = 260, Rectangle = new System.Drawing.Rectangle(200, -65, 390, 170), Alignment = PdfSharp.Drawing.Layout.XParagraphAlignment.Center });
        //    dataTable.Columns.Add("VestNo");
        //    //VestPDF.fields.Add(new RectagleField() { Name = "VestBarcode",  Column = "VestBarcode",     FontSize = 72,  Rectangle = new System.Drawing.Rectangle(10, 190, 570, 60), FontName = "Free 3 of 9", Alignment = PdfSharp.Drawing.Layout.XParagraphAlignment.Center });
        //    //dataTable.Columns.Add("VestBarcode");
        //    //VestPDF.fields.Add(new RectagleField() { Name = "EventCode",    Column = "EventCode",       FontSize = 120, Rectangle = new System.Drawing.Rectangle(19, 50, 172, 80), Alignment = PdfSharp.Drawing.Layout.XParagraphAlignment.Center });
        //    dataTable.Columns.Add("EventCode");

        //    Instuction instructions = (Instuction)Instructions;

        //    List<AEvent> events = instructions.events;
        //    List<Team> areas = instructions.areas;
        //    bool print = instructions.print;

        //    Wait wait = new Wait();
        //    wait.Bar1.setLabel("Competitors");
        //    wait.Bar2.setLabel("Events");
        //    wait.Bar3.setLabel("teams");
        //    wait.Bar1.setEnabled(true);
        //    wait.Bar2.setEnabled(true);
        //    wait.Bar3.setEnabled(true);

        //    wait.Bar3.setMax(areas.Count);

        //    Thread t = new Thread(new ParameterizedThreadStart(wait.Start));
        //    t.Start(wait);

        //    try
        //    {
        //        VestPDF = (PDFReport) AReport.LoadTemplate(TemplatePath);
        //        //VestPDF.TemplatePath = @Properties.Settings.Default.RootPathTemplate + "BlankVests.pdf";
        //    }
        //    catch 
        //    {
        //        wait.Complete();
        //        t.Abort();
        //        return;
        //    }

        //    foreach (Team a in areas)
        //    {
        //        wait.Bar2.setValue(0);
        //        foreach (AEvent e in events)
        //        {

        //            List<ACompetitor> lsCompetitors = e.getEnteredCompetitors(a);  // e.EnteredCompetitors.Where( c => c.getTeam() == a).ToList();

        //            if (lsCompetitors.Count > 0)
        //            {
        //                wait.Bar1.setValue(0);
        //                wait.Bar1.setMax(lsCompetitors.Count);

        //                dataTable.Rows.Clear();

        //                foreach (Competitor c in lsCompetitors)
        //                {
        //                    //StudentCompetitor sc = (StudentCompetitor)c;

        //                    DataRow dr = dataTable.NewRow();

                            
        //                    dr["AreaName"] = a.Name.ToUpper();
        //                    dr["EventCode"] = e.ShortName;
        //                    dr["VestNo"] = c.Vest.ToString();

        //                    string SchoolName = "";
        //                    if (((StudentAthlete)c.Athlete).Attends != null)
        //                        SchoolName = ((StudentAthlete)c.Athlete).Attends.ToString();

        //                    object yearGroup = c.checkParameter("YearGroup");
        //                    dr["Name"] = c.Athlete.PrintName() + " - Year " + ( yearGroup != null ? yearGroup.ToString() : " " ).ToString(); // sc.formatYearGroup();
        //                    dr["School"] = SchoolName;
        //                    dr["AreaShortName"] = a.ShortName;
        //                    //string BarCode = "*" + "V-" + c.Vest.ToString() + "%E-" + e.ShortName + "*";
        //                    //dr["VestBarcode"] = BarCode;

        //                    dataTable.Rows.Add(dr);

        //                    wait.Bar1.increment();

        //                } // end of for competitors


        //                VestPDF.ExportPath = lFolderPath + a.Name + " " + e.ShortName + " Vests.pdf";
        //                VestPDF.Generate(dataTable);
        //                VestPDF.Save();
        //            }// end if there are competitors
        //            wait.Bar2.increment();
        //        } // end of for events
        //        wait.Bar3.increment();
        //    } // end of for areas

        //    wait.Complete();
        //    t.Abort();

        //    sw.Stop();


        //}

        //private string FolderPath = @"D:\Temp\";
        //private string FolderPDFPath = @"D:\TempPDF\";

        private void generateProgramme()
        {
            if (!System.IO.File.Exists(@Properties.Settings.Default.OLD_RootPathTemplate + "ProgrammePageTemplate.docx"))
                return;

            Championship champs = MainWindow.CurrentChampionship;

            Wait wait = new Wait();
            wait.Bar1.setLabel("Competitors");
            wait.Bar2.setLabel("Teams");
            wait.Bar3.setLabel("Events");

            wait.Bar1.setEnabled(true);
            wait.Bar2.setEnabled(true);
            wait.Bar3.setEnabled(true);

            wait.Bar3.setMax(champs.listAllTeams().Count);

            Thread thread = new Thread(new ParameterizedThreadStart(wait.Start));
            thread.Start(wait);

            Microsoft.Office.Interop.Word.Application word = new Microsoft.Office.Interop.Word.Application();

            Microsoft.Office.Interop.Word.Document file; // = new Microsoft.Office.Interop.Word.Document();

            try
            {
                word.Documents.Open(@Properties.Settings.Default.OLD_RootPathTemplate + "ProgrammePageTemplate.docx");
                file = word.ActiveDocument;
            }
            catch
            {
                wait.Complete();
                thread.Start();
                return;
            }

            Dictionary<string, ProgramCell> cellIndexes = new Dictionary<string, ProgramCell>();

            Microsoft.Office.Interop.Word.Table tab = file.Tables[2];

            string lFolderPath = @Properties.Settings.Default.ProgrammePath;
            string lFolderPDFPath = lFolderPath;

            wait.Bar3.setMax(champs.listAllEvents().Count());
            wait.Bar2.setMax(champs.listAllTeams().Count);
            wait.Bar3.setValue(0);

            foreach (AEvent Event in champs.listAllEvents())
            {

                file.Tables[1].Cell(1, 1).Range.Text = Event.Name;
                file.Tables[1].Cell(3, 1).Range.Text = Event.Description;

                wait.Bar2.setValue(0);

                cellIndexes.Clear();
                //cellIndexes.Add("Kennet", new ProgramCell() { Heading = tab.Cell(1, 1), Vests = tab.Cell(2, 1), Names = tab.Cell(2, 2), NoOfVests = 16 });
                //cellIndexes.Add("North Wiltshire", new ProgramCell() { Heading = tab.Cell(1, 3), Vests = tab.Cell(2, 4), Names = tab.Cell(2, 5), NoOfVests = 16 });
                //cellIndexes.Add("Salisbury", new ProgramCell() { Heading = tab.Cell(3, 1), Vests = tab.Cell(4, 1), Names = tab.Cell(4, 2), NoOfVests = 16 });
                //cellIndexes.Add("Swindon", new ProgramCell() { Heading = tab.Cell(3, 3), Vests = tab.Cell(4, 4), Names = tab.Cell(4, 5), NoOfVests = 16 });
                //cellIndexes.Add("West Wiltshire", new ProgramCell()
                //{
                //    Heading = tab.Cell(5, 1),
                //    Vests = tab.Cell(6, 1),
                //    Names = tab.Cell(6, 2),
                //    NoOfVests = 8,
                //    OverFlowCell = new ProgramCell() { Heading = tab.Cell(6, 4), Vests = tab.Cell(6, 4), Names = tab.Cell(6, 5), NoOfVests = 8 }
                //});

                cellIndexes.Add("Avon", new ProgramCell()               { Heading = tab.Cell(1, 1), Vests = tab.Cell(2, 1), Names = tab.Cell(2, 2), NoOfVests = 16 });
                cellIndexes.Add("Cornwall", new ProgramCell()           { Heading = tab.Cell(1, 3), Vests = tab.Cell(2, 4), Names = tab.Cell(2, 5), NoOfVests = 16 });
                cellIndexes.Add("Devon", new ProgramCell()              { Heading = tab.Cell(1, 5), Vests = tab.Cell(2, 7), Names = tab.Cell(2, 8), NoOfVests = 16 });
                cellIndexes.Add("Dorset", new ProgramCell()             { Heading = tab.Cell(3, 1), Vests = tab.Cell(4, 1), Names = tab.Cell(4, 2), NoOfVests = 16 });
                cellIndexes.Add("Gloucestershire", new ProgramCell()    { Heading = tab.Cell(3, 3), Vests = tab.Cell(4, 4), Names = tab.Cell(4, 5), NoOfVests = 16 });
                cellIndexes.Add("Somerset", new ProgramCell()           { Heading = tab.Cell(3, 5), Vests = tab.Cell(4, 7), Names = tab.Cell(4, 8), NoOfVests = 16 });
                cellIndexes.Add("Wiltshire", new ProgramCell()
                {
                    Heading = tab.Cell(5, 1),
                    Vests = tab.Cell(6, 1),
                    Names = tab.Cell(6, 2),
                    NoOfVests = 6,
                    OverFlowCell = new ProgramCell()
                    {
                        Heading = tab.Cell(6, 4),
                        Vests = tab.Cell(6, 4),
                        Names = tab.Cell(6, 5),
                        NoOfVests = 6,
                        OverFlowCell = new ProgramCell()
                            {
                                Heading = tab.Cell(6, 7),
                                Vests = tab.Cell(6, 7),
                                Names = tab.Cell(6, 8),
                                NoOfVests = 4,
                            }
                    }
                });


                foreach (Team team in champs.listAllTeams())
                {
                    List<ACompetitor> AllAthletes = Event.getEnteredCompetitors(team);

                    System.Diagnostics.Debug.WriteLine("{0} {1} Competitors {2}", Event.Name, team.Name, AllAthletes.Count);

                    ProgramCell cells = cellIndexes[team.Name];

                    cells.setHeading(team.Name);

                    wait.Bar1.setMax(AllAthletes.Count);
                    wait.Bar1.setValue(0);

                    foreach (Competitor athlete in AllAthletes)
                    {
                        wait.Bar1.increment();
                        cells.addName(athlete.getName());
                        cells.addVest(athlete.Vest.ToString());
                    }

                    cells.ApplyStrings();
                    wait.Bar2.increment();
                }//Each team

                file.SaveAs2(lFolderPath + Event.ShortName + " Programme Page.docx");
                file.ExportAsFixedFormat(lFolderPDFPath + Event.ShortName + " Programme Page.pdf", Microsoft.Office.Interop.Word.WdExportFormat.wdExportFormatPDF);
                wait.Bar3.increment();
            }// each event

            ((Microsoft.Office.Interop.Word._Document)file).Close();
            ((Microsoft.Office.Interop.Word._Application)word).Quit();

            wait.Complete();
            thread.Abort();

        }

        private class ProgramCell
        {
            public Microsoft.Office.Interop.Word.Cell Heading { get; set; }
            public Microsoft.Office.Interop.Word.Cell Vests { get; set; }
            public Microsoft.Office.Interop.Word.Cell Names { get; set; }

            public int vestCounter, nameCounter;
            private string heading, vests, names;

            public void addVest(string vest)
            {
                if (vestCounter >= NoOfVests)
                {
                    // this cell is full
                    if (hasOverflow())
                        OverFlowCell.addVest(vest);
                    else
                    {
                        System.Diagnostics.Debug.WriteLine("Too many vests");
                     //   throw new OverflowException("Too many vests");
                    }
                }
                else
                {
                    vests += vest + '\r';
                    vestCounter++;
                }
            }

            public void addName(string name)
            {
                if (nameCounter >= NoOfVests)
                {
                    // this cell is full
                    if (hasOverflow())
                        OverFlowCell.addName(name);
                    else
                    {
                        System.Diagnostics.Debug.WriteLine("Too many athletes");
                    }
                        //throw new OverflowException("Too many names");
                }
                else
                {
                    names += name + '\r';
                    nameCounter++;
                }
            }

            public void setHeading(string heading)
            {
                this.heading = heading;
            }

            public int getTotalSpaces()
            {
                int x;

                if (hasOverflow())
                {
                    x = OverFlowCell.getTotalSpaces();
                }

                x =+ NoOfVests;

                return x;
            }

            public void ApplyStrings()
            {
                Heading.Range.Text = heading;
                Vests.Range.Text = vests;
                Names.Range.Text = names;

                if (hasOverflow())
                {
                    OverFlowCell.ApplyStrings();
                }
            }

            public int NoOfVests { get; set; }

            public bool hasOverflow()
            {
                return overFlowCell != null;
            }

            private ProgramCell overFlowCell;

            public ProgramCell OverFlowCell
            {
                get
                {
                    if (overFlowCell != null)
                        return overFlowCell;
                    else
                    {
                        System.Diagnostics.Debug.WriteLine("Overflow Cell not available");
                        return new ProgramCell();
                        //throw new NullReferenceException("Overflow cell is null");
                    }
                }
                set
                {
                    overFlowCell = value;
                }
            }
        }

        private static string QuoteValue(string value)
        {
            return String.Concat("\"", value.Replace("\"", "\"\""), "\"");
        }

    }// end of class


    class Instuction
    {

        //public Instruction(List<AEvent> Events)
        //{
        //    print = false;
        //    events = Events;
        //    areas = new List<Team>();
        //}

        public Instuction(List<AEvent> Events, Boolean Print = false)
        {
            print = Print;
            events = Events;
            areas = new List<Team>();
        }

        public Instuction(List<AEvent> Events, List<Team> Areas, Boolean Print)
        {
            print = Print;
            events = Events;
            areas = Areas;
        }

        public Instuction(List<Team> Areas, Boolean Print)
        {
            print = Print;
            events = new List<AEvent>();
            areas = Areas;
        }

        public Instuction(List<Team> Areas)
        {
            print = false;
            events = new List<AEvent>();
            areas = Areas;
        }

        public Instuction(List<AEvent> Events, string CertType, char Team = 'A', int TeamPosition = 1, Boolean Print = false)
        {
            print = Print;
            events = Events;
            areas = new List<Team>();
            team = Team;
            certType = CertType;
            teamPosition = TeamPosition;
        }

        public string strTeamPosition()
        {
            return new IntToStr().ToStr(teamPosition);
        }

        public List<AEvent> events { get; set; }
        public List<Team> areas { get; set; }
        public Boolean print { get; set; }
        public char team { get; set; }
        public string certType { get; set; }
        public int teamPosition { get; set; }
        public Boolean open { get; set; }
    }

    class IntToStr
    {
        public IntToStr()
        {
        }
        public string ToStr(int Integer)
        {
            switch (Integer)
            {
                case 1:
                    return "First";
                case 2:
                    return "Second";
                case 3:
                    return "Third";
                case 4:
                    return "Fourth";
                case 5:
                    return "Fifth";
                case 6:
                    return "Sixth";
                default:
                    return Integer.ToString();
            }
        }
    }

}
