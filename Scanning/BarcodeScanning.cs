using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections;

namespace ChampionshipSolutions.ControlRoom.Scanning
{
    public static class BarcodeScanning
    {
        public static ArrayList ScanImgForBarcode ( System.Drawing.Bitmap image )
        {

            if ( image == null ) throw new ArgumentException("image can not be null");

            ArrayList barcodes = new ArrayList();

            int iScans = 100;

            BarcodeImaging.UseBarcodeZones = false;
            BarcodeImaging.FullScanBarcodeTypes = BarcodeImaging.BarcodeType.Code39;

            BarcodeImaging.FullScanPage(ref barcodes,image,iScans);

            return barcodes;

        }
    }
}
